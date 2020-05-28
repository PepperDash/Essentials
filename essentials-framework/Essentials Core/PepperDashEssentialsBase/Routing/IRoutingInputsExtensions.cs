using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Crestron.SimplSharp;
using Crestron.SimplSharpPro;
using Crestron.SimplSharpPro.DM;

using PepperDash.Core;


namespace PepperDash.Essentials.Core
{
	/// <summary>
	/// Extensions added to any IRoutingInputs classes to provide discovery-based routing
	/// on those destinations.
	/// </summary>
	public static class IRoutingInputsExtensions
	{
		/// <summary>
		/// Gets any existing RouteDescriptor for a destination, clears it using ReleaseRoute
		/// and then attempts a new Route and if sucessful, stores that RouteDescriptor
		/// in RouteDescriptorCollection.DefaultCollection
		/// </summary>
		public static void ReleaseAndMakeRoute(this IRoutingSink destination, IRoutingOutputs source, eRoutingSignalType signalType)
		{
			destination.ReleaseRoute();

			if (source == null) return;
			var newRoute = destination.GetRouteToSource(source, signalType);
			if (newRoute == null) return;
			RouteDescriptorCollection.DefaultCollection.AddRouteDescriptor(newRoute);
			Debug.Console(2, destination, "Executing full route");
			newRoute.ExecuteRoutes();
		}

		/// <summary>
		/// Will release the existing route on the destination, if it is found in 
		/// RouteDescriptorCollection.DefaultCollection
		/// </summary>
		/// <param name="destination"></param>
        public static void ReleaseRoute(this IRoutingSink destination)
		{
			var current = RouteDescriptorCollection.DefaultCollection.RemoveRouteDescriptor(destination);
			if (current != null)
			{
				Debug.Console(1, destination, "Releasing current route: {0}", current.Source.Key);
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
        public static RouteDescriptor GetRouteToSource(this IRoutingSink destination, IRoutingOutputs source, eRoutingSignalType signalType)
		{
			var routeDescr = new RouteDescriptor(source, destination, signalType);
			// if it's a single signal type, find the route
			if ((signalType & (eRoutingSignalType.Audio & eRoutingSignalType.Video)) == (eRoutingSignalType.Audio & eRoutingSignalType.Video))
			{
				Debug.Console(1, destination, "Attempting to build source route from {0}", source.Key);
				if (!destination.GetRouteToSource(source, null, null, signalType, 0, routeDescr))
					routeDescr = null;
			}
			// otherwise, audioVideo needs to be handled as two steps.
			else
			{
				Debug.Console(1, destination, "Attempting to build audio and video routes from {0}", source.Key);
				var audioSuccess = destination.GetRouteToSource(source, null, null, eRoutingSignalType.Audio, 0, routeDescr);
				if (!audioSuccess)
					Debug.Console(1, destination, "Cannot find audio route to {0}", source.Key);
				var videoSuccess = destination.GetRouteToSource(source, null, null, eRoutingSignalType.Video, 0, routeDescr);
				if (!videoSuccess)
					Debug.Console(1, destination, "Cannot find video route to {0}", source.Key);
				if (!audioSuccess && !videoSuccess)
					routeDescr = null;
			}

            //Debug.Console(1, destination, "Route{0} discovered", routeDescr == null ? " NOT" : "");
			return routeDescr;
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
		static bool GetRouteToSource(this IRoutingInputs destination, IRoutingOutputs source,
			RoutingOutputPort outputPortToUse, List<IRoutingInputsOutputs> alreadyCheckedDevices, 
				eRoutingSignalType signalType, int cycle, RouteDescriptor routeTable)
		{
			cycle++;
			Debug.Console(2, "GetRouteToSource: {0} {1}--> {2}", cycle, source.Key, destination.Key);

			RoutingInputPort goodInputPort = null;
			var destDevInputTies = TieLineCollection.Default.Where(t =>
                t.DestinationPort.ParentDevice == destination && (t.Type == signalType || (t.Type & (eRoutingSignalType.Audio | eRoutingSignalType.Video)) == (eRoutingSignalType.Audio | eRoutingSignalType.Video)));

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
				Debug.Console(2, destination, "is not directly connected to {0}. Walking down tie lines", source.Key);

				// No direct tie? Run back out on the inputs' attached devices... 
				// Only the ones that are routing devices
				var attachedMidpoints = destDevInputTies.Where(t => t.SourcePort.ParentDevice is IRoutingInputsOutputs);
				foreach (var inputTieToTry in attachedMidpoints)
				{
					Debug.Console(2, destination, "Trying to find route on {0}", inputTieToTry.SourcePort.ParentDevice.Key);
					var upstreamDeviceOutputPort = inputTieToTry.SourcePort;
					var upstreamRoutingDevice = upstreamDeviceOutputPort.ParentDevice as IRoutingInputsOutputs;
					// Check if this previous device has already been walked
					if (!(alreadyCheckedDevices != null && alreadyCheckedDevices.Contains(upstreamRoutingDevice)))
					{
						// haven't seen this device yet.  Do it.  Pass the output port to the next
						// level to enable switching on success
						var upstreamRoutingSuccess = upstreamRoutingDevice.GetRouteToSource(source, upstreamDeviceOutputPort, 
							alreadyCheckedDevices, signalType, cycle, routeTable);
						if (upstreamRoutingSuccess)
						{
							Debug.Console(2, destination, "Upstream device route found");
							goodInputPort = inputTieToTry.DestinationPort;
							break; // Stop looping the inputs in this cycle
						}
					}
				}
			}

			// we have a route on corresponding inputPort. *** Do the route ***
			if (goodInputPort != null) 
			{
                //Debug.Console(2, destination, "adding RouteDescriptor");
				if (outputPortToUse == null)
				{
					// it's a sink device
                        routeTable.Routes.Add(new RouteSwitchDescriptor(goodInputPort));
				}
				else if (destination is IRouting)
				{
					routeTable.Routes.Add(new RouteSwitchDescriptor (outputPortToUse, goodInputPort));
				}
				else // device is merely IRoutingInputOutputs
					Debug.Console(2, destination, "    No routing. Passthrough device");
                //Debug.Console(2, destination, "Exiting cycle {0}", cycle);
				return true;
			}
	
			if(alreadyCheckedDevices == null)
				alreadyCheckedDevices = new List<IRoutingInputsOutputs>();
			alreadyCheckedDevices.Add(destination as IRoutingInputsOutputs);

			Debug.Console(2, destination, "No route found to {0}", source.Key);
			return false;
		}
	}





	// MOVE MOVE MOVE MOVE MOVE MOVE MOVE  MOVE MOVE MOVE MOVE MOVE MOVE MOVE  MOVE MOVE MOVE MOVE MOVE MOVE MOVE 


	/// <summary>
	/// A collection of RouteDescriptors - typically the static DefaultCollection is used
	/// </summary>
	public class RouteDescriptorCollection
	{
		public static RouteDescriptorCollection DefaultCollection
		{
			get
			{
				if (_DefaultCollection == null)
					_DefaultCollection = new RouteDescriptorCollection();
				return _DefaultCollection;
			}
		}
		static RouteDescriptorCollection _DefaultCollection;

		List<RouteDescriptor> RouteDescriptors = new List<RouteDescriptor>();

		/// <summary>
		/// Adds a RouteDescriptor to the list.  If an existing RouteDescriptor for the
		/// destination exists already, it will not be added - in order to preserve
		/// proper route releasing.
		/// </summary>
		/// <param name="descriptor"></param>
		public void AddRouteDescriptor(RouteDescriptor descriptor)
		{
			if (RouteDescriptors.Any(t => t.Destination == descriptor.Destination))
			{
				Debug.Console(1, descriptor.Destination, 
					"Route to [{0}] already exists in global routes table", descriptor.Source.Key);
				return;
			}
			RouteDescriptors.Add(descriptor);
		}

		/// <summary>
		/// Gets the RouteDescriptor for a destination
		/// </summary>
		/// <returns>null if no RouteDescriptor for a destination exists</returns>
		public RouteDescriptor GetRouteDescriptorForDestination(IRoutingInputs destination)
		{
			return RouteDescriptors.FirstOrDefault(rd => rd.Destination == destination);
		}

		/// <summary>
		/// Returns the RouteDescriptor for a given destination AND removes it from collection.
		/// Returns null if no route with the provided destination exists.
		/// </summary>
		public RouteDescriptor RemoveRouteDescriptor(IRoutingInputs destination)
		{
			var descr = GetRouteDescriptorForDestination(destination);
			if (descr != null)
				RouteDescriptors.Remove(descr);
			return descr;
		}
	}

	/// <summary>
	/// Represents an collection of individual route steps between Source and Destination
	/// </summary>
	public class RouteDescriptor
	{
		public IRoutingInputs Destination { get; private set; }
		public IRoutingOutputs Source { get; private set; }
		public eRoutingSignalType SignalType { get; private set; }
		public List<RouteSwitchDescriptor> Routes { get; private set; }


		public RouteDescriptor(IRoutingOutputs source, IRoutingInputs destination, eRoutingSignalType signalType)
		{
			Destination = destination;
			Source = source;
			SignalType = signalType;
			Routes = new List<RouteSwitchDescriptor>();
		}

		/// <summary>
		/// Executes all routes described in this collection.  Typically called via
		/// extension method IRoutingInputs.ReleaseAndMakeRoute()
		/// </summary>
		public void ExecuteRoutes()
		{
			foreach (var route in Routes)
			{
				Debug.Console(2, "ExecuteRoutes: {0}", route.ToString());
                if (route.SwitchingDevice is IRoutingSink)
                {
                    var device = route.SwitchingDevice as IRoutingSinkWithSwitching;
                    if (device == null)
                        continue;

                    device.ExecuteSwitch(route.InputPort.Selector);
                }
                else if (route.SwitchingDevice is IRouting)
                {
                    (route.SwitchingDevice as IRouting).ExecuteSwitch(route.InputPort.Selector, route.OutputPort.Selector, SignalType);
                    route.OutputPort.InUseTracker.AddUser(Destination, "destination-" + SignalType);
                    Debug.Console(2, "Output port {0} routing. Count={1}", route.OutputPort.Key, route.OutputPort.InUseTracker.InUseCountFeedback.UShortValue);
                }
			}
		}

		/// <summary>
		/// Releases all routes in this collection. Typically called via
		/// extension method IRoutingInputs.ReleaseAndMakeRoute()
		/// </summary>
		public void ReleaseRoutes()
		{
			foreach (var route in Routes)
			{
				if (route.SwitchingDevice is IRouting)
				{
					// Pull the route from the port.  Whatever is watching the output's in use tracker is
					// responsible for responding appropriately.
					route.OutputPort.InUseTracker.RemoveUser(Destination, "destination-" + SignalType);
					Debug.Console(2, "Port {0} releasing. Count={1}", route.OutputPort.Key, route.OutputPort.InUseTracker.InUseCountFeedback.UShortValue);
				}
			}
		}

		public override string ToString()
		{
			var routesText = Routes.Select(r => r.ToString()).ToArray();
			return string.Format("Route table from {0} to {1}:\r{2}", Source.Key, Destination.Key, string.Join("\r", routesText));
		}
	}

	/// <summary>
	/// Represents an individual link for a route
	/// </summary>
	public class RouteSwitchDescriptor
	{
		public IRoutingInputs SwitchingDevice { get { return InputPort.ParentDevice; } }
		public RoutingOutputPort OutputPort { get; set; }
		public RoutingInputPort InputPort { get; set; }

		public RouteSwitchDescriptor(RoutingInputPort inputPort)
		{
			InputPort = inputPort;
		}

		public RouteSwitchDescriptor(RoutingOutputPort outputPort, RoutingInputPort inputPort)
		{
			InputPort = inputPort;
			OutputPort = outputPort;
		}

		public override string ToString()
		{
			if(SwitchingDevice is IRouting)
				return string.Format("{0} switches output '{1}' to input '{2}'", SwitchingDevice.Key, OutputPort.Selector, InputPort.Selector);
			else
				return string.Format("{0} switches to input '{1}'", SwitchingDevice.Key, InputPort.Selector);

		}
	}
}