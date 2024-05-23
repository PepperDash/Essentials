using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Crestron.SimplSharp;
using Crestron.SimplSharpPro;
using Crestron.SimplSharpPro.DM;

using PepperDash.Core;
using Serilog.Events;


namespace PepperDash.Essentials.Core
{

    /// <summary>
    /// Extensions added to any IRoutingInputs classes to provide discovery-based routing
    /// on those destinations.
    /// </summary>
    public static class GenericExtensions
	{
        //private static readonly Dictionary<string, RouteRequest<TInputSelector, TOutputSelector>> RouteRequests = new();

		/// <summary>
		/// Gets any existing RouteDescriptor for a destination, clears it using ReleaseRoute
		/// and then attempts a new Route and if sucessful, stores that RouteDescriptor
		/// in RouteDescriptorCollection.DefaultCollection
		/// </summary>
		public static void ReleaseAndMakeRoute<TInputSelector, TOutputSelector>(this IRoutingSink<TInputSelector> destination, IRoutingOutputs<TOutputSelector> source, eRoutingSignalType signalType)
		{
            var routeRequest = new RouteRequest<TInputSelector, TOutputSelector> { 
                        Destination = destination,
                        Source = source,
                        SignalType = signalType
                    };

            var coolingDevice = destination as IWarmingCooling;
            var existingRouteRequest = destination.GetRouteRequest<TOutputSelector>();

            //We already have a route request for this device, and it's a cooling device and is cooling
            if (existingRouteRequest != null && coolingDevice != null && coolingDevice.IsCoolingDownFeedback.BoolValue == true)
            {
                coolingDevice.IsCoolingDownFeedback.OutputChange -= existingRouteRequest.HandleCooldown;

                coolingDevice.IsCoolingDownFeedback.OutputChange += routeRequest.HandleCooldown;

                destination.UpdateRouteRequest(routeRequest);

                Debug.LogMessage(LogEventLevel.Verbose, "Device: {0} is cooling down and already has a routing request stored.  Storing new route request to route to source key: {1}", null, destination.Key, routeRequest.Source.Key);

                return;
            }

            //New Request
            if (coolingDevice != null && coolingDevice.IsCoolingDownFeedback.BoolValue == true)
            {
                coolingDevice.IsCoolingDownFeedback.OutputChange -= routeRequest.HandleCooldown;

                coolingDevice.IsCoolingDownFeedback.OutputChange += routeRequest.HandleCooldown;

                destination.UpdateRouteRequest(routeRequest);

                Debug.LogMessage(LogEventLevel.Verbose, "Device: {0} is cooling down.  Storing route request to route to source key: {1}", null, destination.Key, routeRequest.Source.Key);
                return;
            }

            if (existingRouteRequest != null && coolingDevice != null && coolingDevice.IsCoolingDownFeedback.BoolValue == false)
            {
                destination.UpdateRouteRequest<TOutputSelector>(null);

                Debug.LogMessage(LogEventLevel.Verbose, "Device: {0} is NOT cooling down.  Removing stored route request and routing to source key: {1}", null, destination.Key, routeRequest.Source.Key);
            }

            destination.ReleaseRoute();

			RunRouteRequest(routeRequest);
		}

        public static void RunRouteRequest<TInputSelector, TOutputSelector>(RouteRequest<TInputSelector, TOutputSelector> request)
        {
            if (request.Source == null)
                return;
            
            var newRoute = request.Destination.GetRouteToSource(request.Source, request.SignalType);

            if (newRoute == null)
                return;

            RouteDescriptorCollection.DefaultCollection.AddRouteDescriptor(newRoute);

            Debug.LogMessage(LogEventLevel.Verbose, "Executing full route", request.Destination);

            newRoute.ExecuteRoutes();
        }

		/// <summary>
		/// Will release the existing route on the destination, if it is found in 
		/// RouteDescriptorCollection.DefaultCollection
		/// </summary>
		/// <param name="destination"></param>
        public static void ReleaseRoute<TInputSelector, TOutputSelector>(this IRoutingSink<TInputSelector> destination)
		{
            var existingRouteRequest = destination.GetRouteRequest<TOutputSelector>();

            if (existingRouteRequest != null && destination is IWarmingCooling)
            {
                var coolingDevice = destination as IWarmingCooling;

                coolingDevice.IsCoolingDownFeedback.OutputChange -= existingRouteRequest.HandleCooldown;
            }

            destination.UpdateRouteRequest<TOutputSelector>(null);

			var current = RouteDescriptorCollection<TInputSelector, TOutputSelector>.DefaultCollection.RemoveRouteDescriptor(destination);
			if (current != null)
			{
				Debug.LogMessage(LogEventLevel.Debug, "Releasing current route: {0}", destination, current.Source.Key);
				current.ReleaseRoutes();
			}
		}

		/// <summary>
		/// Builds a RouteDescriptor that contains the steps necessary to make a route between devices.  
		/// Routes of type AudioVideo will be built as two separate routes, audio and video. If
		/// a route is discovered, a new RouteDescriptor is returned.  If one or both parts
		/// of an audio/video route are discovered a route descriptor is returned.  If no route is 
		/// discovered, then null is returned
		/// </summary>
        public static RouteDescriptor<TInputSelector, TOutputSelector> GetRouteToSource<TInputSelector, TOutputSelector>(this IRoutingSink<TInputSelector> destination, IRoutingOutputs<TOutputSelector> source, eRoutingSignalType signalType)
		{
			var routeDescriptor = new RouteDescriptor<TInputSelector, TOutputSelector>(source, destination, signalType);

			// if it's a single signal type, find the route
			if (!signalType.HasFlag(eRoutingSignalType.AudioVideo))
			{
				Debug.LogMessage(LogEventLevel.Debug, "Attempting to build source route from {0}", null, source.Key);

				if (!destination.GetRouteToSource(source, null, null, signalType, 0, routeDescriptor))
					routeDescriptor = null;

                return routeDescriptor;
			}
			// otherwise, audioVideo needs to be handled as two steps.
			
			Debug.LogMessage(LogEventLevel.Debug, "Attempting to build audio and video routes from {0}", destination, source.Key);

			var audioSuccess = destination.GetRouteToSource(source, null, null, eRoutingSignalType.Audio, 0, routeDescriptor);

			if (!audioSuccess)
				Debug.LogMessage(LogEventLevel.Debug, "Cannot find audio route to {0}", destination, source.Key);

			var videoSuccess = destination.GetRouteToSource(source, null, null, eRoutingSignalType.Video, 0, routeDescriptor);

			if (!videoSuccess)
				Debug.LogMessage(LogEventLevel.Debug, "Cannot find video route to {0}", destination, source.Key);

			if (!audioSuccess && !videoSuccess)
				routeDescriptor = null;
			
            
			return routeDescriptor;
		}

		/// <summary>
		/// The recursive part of this.  Will stop on each device, search its inputs for the 
		/// desired source and if not found, invoke this function for the each input port
		/// hoping to find the source.
		/// </summary>
		/// <param name="destination"></param>
		/// <param name="source"></param>
		/// <param name="outputPortToUse">The RoutingOutputPort whose link is being checked for a route</param>
		/// <param name="alreadyCheckedDevices">Prevents Devices from being twice-checked</param>
		/// <param name="signalType">This recursive function should not be called with AudioVideo</param>
		/// <param name="cycle">Just an informational counter</param>
		/// <param name="routeTable">The RouteDescriptor being populated as the route is discovered</param>
		/// <returns>true if source is hit</returns>
		static bool GetRouteToSource<TInputSelector, TOutputSelector>(this IRoutingInputs<TInputSelector> destination, IRoutingOutputs<TOutputSelector> source,
			RoutingOutputPort<TOutputSelector> outputPortToUse, List<IRoutingInputsOutputs<TInputSelector, TOutputSelector>> alreadyCheckedDevices, 
				eRoutingSignalType signalType, int cycle, RouteDescriptor<TInputSelector, TOutputSelector> routeTable)
		{

			cycle++;

			Debug.LogMessage(LogEventLevel.Verbose, "GetRouteToSource: {0} {1}--> {2}", null, cycle, source.Key, destination.Key);

			RoutingInputPort<TInputSelector> goodInputPort = null;

			var destDevInputTies = TieLineCollection.Default.Where(t =>
                t.DestinationPort.ParentDevice == destination && (t.Type == signalType || t.Type.HasFlag(eRoutingSignalType.AudioVideo)));

			// find a direct tie
			var directTie = destDevInputTies.FirstOrDefault(
				t => t.DestinationPort.ParentDevice == destination 
					&& t.SourcePort.ParentDevice == source);
			if (directTie != null) // Found a tie directly to the source
			{
				goodInputPort = directTie.DestinationPort;
			}
			else // no direct-connect.  Walk back devices.
			{
				Debug.LogMessage(LogEventLevel.Verbose, "is not directly connected to {0}. Walking down tie lines", destination, source.Key);

				// No direct tie? Run back out on the inputs' attached devices... 
				// Only the ones that are routing devices
				var attachedMidpoints = destDevInputTies.Where(t => t.SourcePort.ParentDevice is IRoutingInputsOutputs);

                //Create a list for tracking already checked devices to avoid loops, if it doesn't already exist from previous iteration
                if (alreadyCheckedDevices == null)
                    alreadyCheckedDevices = new List<IRoutingInputsOutputs<TInputSelector, TOutputSelector>>();
                alreadyCheckedDevices.Add(destination as IRoutingInputsOutputs<TInputSelector, TOutputSelector>);

				foreach (var inputTieToTry in attachedMidpoints)
				{
					var upstreamDeviceOutputPort = inputTieToTry.SourcePort;
					var upstreamRoutingDevice = upstreamDeviceOutputPort.ParentDevice as IRoutingInputsOutputs<TInputSelector, TOutputSelector>;
                    Debug.LogMessage(LogEventLevel.Verbose,  "Trying to find route on {0}", destination, upstreamRoutingDevice.Key);

					// Check if this previous device has already been walked
                    if (alreadyCheckedDevices.Contains(upstreamRoutingDevice))
                    {
                        Debug.LogMessage(LogEventLevel.Verbose, "Skipping input {0} on {1}, this was already checked", destination, upstreamRoutingDevice.Key, destination.Key);
                        continue;
                    }
                    // haven't seen this device yet.  Do it.  Pass the output port to the next
                    // level to enable switching on success
                    var upstreamRoutingSuccess = upstreamRoutingDevice.GetRouteToSource(source, upstreamDeviceOutputPort,
                        alreadyCheckedDevices, signalType, cycle, routeTable);
                    if (upstreamRoutingSuccess)
                    {
                        Debug.LogMessage(LogEventLevel.Verbose, "Upstream device route found", destination);
                        goodInputPort = inputTieToTry.DestinationPort;
                        break; // Stop looping the inputs in this cycle
                    }
				}
			}

			
			if (goodInputPort == null) 
			{
                Debug.LogMessage(LogEventLevel.Verbose, "No route found to {0}", destination, source.Key);
                return false;
			}

            // we have a route on corresponding inputPort. *** Do the route ***

            if (outputPortToUse == null)
            {
                // it's a sink device
                routeTable.Routes.Add(new RouteSwitchDescriptor<TInputSelector, TOutputSelector>(goodInputPort));
            }
            else if (destination is IRouting)
            {
                routeTable.Routes.Add(new RouteSwitchDescriptor<TInputSelector, TOutputSelector>(outputPortToUse, goodInputPort));
            }
            else // device is merely IRoutingInputOutputs
                Debug.LogMessage(LogEventLevel.Verbose, "No routing. Passthrough device", destination);
            
            return true;
        }
	}	
}