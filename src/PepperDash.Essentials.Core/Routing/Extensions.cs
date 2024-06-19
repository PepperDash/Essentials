using PepperDash.Core;
using Serilog.Events;
using System.Collections.Generic;
using System.Linq;


namespace PepperDash.Essentials.Core
{

    /// <summary>
    /// Extensions added to any IRoutingInputs classes to provide discovery-based routing
    /// on those destinations.
    /// </summary>
    public static class Extensions
    {
        private static readonly Dictionary<string, RouteRequest> RouteRequests = new Dictionary<string, RouteRequest>();

        /// <summary>
        /// Gets any existing RouteDescriptor for a destination, clears it using ReleaseRoute
        /// and then attempts a new Route and if sucessful, stores that RouteDescriptor
        /// in RouteDescriptorCollection.DefaultCollection
        /// </summary>
        public static void ReleaseAndMakeRoute(this IRoutingInputs destination, IRoutingOutputs source, eRoutingSignalType signalType, string destinationPortKey = "", string sourcePortKey = "")
        {
            var inputPort = string.IsNullOrEmpty(destinationPortKey) ? null : destination.InputPorts.FirstOrDefault(p => p.Key == destinationPortKey);
            var outputPort = string.IsNullOrEmpty(sourcePortKey) ? null : source.OutputPorts.FirstOrDefault(p => p.Key == sourcePortKey);

            ReleaseAndMakeRoute(destination, source, signalType, inputPort, outputPort);
        }

        private static void ReleaseAndMakeRoute(IRoutingInputs destination, IRoutingOutputs source, eRoutingSignalType signalType, RoutingInputPort destinationPort = null, RoutingOutputPort sourcePort = null)
        {
            var routeRequest = new RouteRequest
            {
                Destination = destination,
                DestinationPort = destinationPort,
                Source = source,
                SourcePort = sourcePort,
                SignalType = signalType
            };

            var coolingDevice = destination as IWarmingCooling;


            //We already have a route request for this device, and it's a cooling device and is cooling
            if (RouteRequests.TryGetValue(destination.Key, out RouteRequest existingRouteRequest) && coolingDevice != null && coolingDevice.IsCoolingDownFeedback.BoolValue == true)
            {
                coolingDevice.IsCoolingDownFeedback.OutputChange -= existingRouteRequest.HandleCooldown;

                coolingDevice.IsCoolingDownFeedback.OutputChange += routeRequest.HandleCooldown;

                RouteRequests[destination.Key] = routeRequest;

                Debug.LogMessage(LogEventLevel.Verbose, "Device: {0} is cooling down and already has a routing request stored.  Storing new route request to route to source key: {1}", null, destination.Key, routeRequest.Source.Key);

                return;
            }

            //New Request
            if (coolingDevice != null && coolingDevice.IsCoolingDownFeedback.BoolValue == true)
            {
                coolingDevice.IsCoolingDownFeedback.OutputChange -= routeRequest.HandleCooldown;

                coolingDevice.IsCoolingDownFeedback.OutputChange += routeRequest.HandleCooldown;

                RouteRequests.Add(destination.Key, routeRequest);

                Debug.LogMessage(LogEventLevel.Verbose, "Device: {0} is cooling down.  Storing route request to route to source key: {1}", null, destination.Key, routeRequest.Source.Key);
                return;
            }

            if (RouteRequests.ContainsKey(destination.Key) && coolingDevice != null && coolingDevice.IsCoolingDownFeedback.BoolValue == false)
            {
                RouteRequests.Remove(destination.Key);
                Debug.LogMessage(LogEventLevel.Verbose, "Device: {0} is NOT cooling down.  Removing stored route request and routing to source key: {1}", null, destination.Key, routeRequest.Source.Key);
            }

            destination.ReleaseRoute();

            RunRouteRequest(routeRequest);
        }

        private static void RunRouteRequest(RouteRequest request)
        {
            if (request.Source == null)
                return;

            var newRoute = request.Destination.GetRouteToSource(request.Source, request.SignalType, request.DestinationPort, request.SourcePort);

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
        public static void ReleaseRoute(this IRoutingInputs destination)
        {

            if (RouteRequests.TryGetValue(destination.Key, out RouteRequest existingRequest) && destination is IWarmingCooling)
            {
                var coolingDevice = destination as IWarmingCooling;

                coolingDevice.IsCoolingDownFeedback.OutputChange -= existingRequest.HandleCooldown;
            }

            RouteRequests.Remove(destination.Key);

            var current = RouteDescriptorCollection.DefaultCollection.RemoveRouteDescriptor(destination);
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
        public static RouteDescriptor GetRouteToSource(this IRoutingInputs destination, IRoutingOutputs source, eRoutingSignalType signalType, RoutingInputPort destinationPort, RoutingOutputPort sourcePort)
        {
            var routeDescriptor = new RouteDescriptor(source, destination, signalType);

            // if it's a single signal type, find the route
            if (!signalType.HasFlag(eRoutingSignalType.AudioVideo))
            {
                Debug.LogMessage(LogEventLevel.Debug, "Attempting to build source route from {0}", null, source.Key);

                if (!destination.GetRouteToSource(source, sourcePort, null, signalType, 0, routeDescriptor, destinationPort))
                    routeDescriptor = null;

                return routeDescriptor;
            }
            // otherwise, audioVideo needs to be handled as two steps.

            Debug.LogMessage(LogEventLevel.Debug, "Attempting to build audio and video routes from {0}", destination, source.Key);

            var audioSuccess = destination.GetRouteToSource(source, sourcePort, null, eRoutingSignalType.Audio, 0, routeDescriptor, destinationPort);

            if (!audioSuccess)
                Debug.LogMessage(LogEventLevel.Debug, "Cannot find audio route to {0}", destination, source.Key);

            var videoSuccess = destination.GetRouteToSource(source, sourcePort, null, eRoutingSignalType.Video, 0, routeDescriptor, destinationPort);

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
        /// <param name="destinationPort">The RoutingOutputPort whose link is being checked for a route</param>
        /// <param name="alreadyCheckedDevices">Prevents Devices from being twice-checked</param>
        /// <param name="signalType">This recursive function should not be called with AudioVideo</param>
        /// <param name="cycle">Just an informational counter</param>
        /// <param name="routeTable">The RouteDescriptor being populated as the route is discovered</param>
        /// <returns>true if source is hit</returns>
        private static bool GetRouteToSource(this IRoutingInputs destination, IRoutingOutputs source,
            RoutingOutputPort sourcePort, List<IRoutingInputsOutputs> alreadyCheckedDevices,
                eRoutingSignalType signalType, int cycle, RouteDescriptor routeTable, RoutingInputPort destinationPort)
        {
            cycle++;

            Debug.LogMessage(LogEventLevel.Verbose, "GetRouteToSource: {0} {1}--> {2}", null, cycle, source.Key, destination.Key);

            RoutingInputPort goodInputPort = null;

            IEnumerable<TieLine> destinationTieLines;
            TieLine directTie = null;

            if (destinationPort == null)
            {

                destinationTieLines = TieLineCollection.Default.Where(t =>
                    t.DestinationPort.ParentDevice.Key == destination.Key && (t.Type == signalType || t.Type.HasFlag(eRoutingSignalType.AudioVideo)));
            }
            else
            {
                destinationTieLines = TieLineCollection.Default.Where(t => t.DestinationPort.ParentDevice.Key == destination.Key && (t.Type == signalType || t.Type.HasFlag(eRoutingSignalType.AudioVideo)));
            }

            // find the TieLine without a port
            if (destinationPort == null && sourcePort == null)
            {
                directTie = destinationTieLines.FirstOrDefault(t => t.DestinationPort.ParentDevice.Key == destination.Key && t.SourcePort.ParentDevice.Key == source.Key);
            }
            // find a tieLine to a specific destination port without a specific source port
            else if (destinationPort != null && sourcePort == null)
            {
                directTie = destinationTieLines.FirstOrDefault(t => t.DestinationPort.Key == destinationPort.Key && t.SourcePort.ParentDevice.Key == source.Key);
            }
            // find a tieline to a specific source port without a specific destination port
            else if (destinationPort == null & sourcePort != null)
            {
                directTie = destinationTieLines.FirstOrDefault(t => t.DestinationPort.ParentDevice.Key == destination.Key && t.SourcePort.Key == sourcePort.Key);
            }
            // find a tieline to a specific source port and destination port
            else if (destinationPort != null && sourcePort != null)
            {
                directTie = destinationTieLines.FirstOrDefault(t => t.DestinationPort.Key == destinationPort.Key && t.SourcePort.Key == sourcePort.Key);
            }

            if (directTie != null) // Found a tie directly to the source
            {
                goodInputPort = directTie.DestinationPort;
            }
            else // no direct-connect.  Walk back devices.
            {
                Debug.LogMessage(LogEventLevel.Verbose, "is not directly connected to {0}. Walking down tie lines", destination, source.Key);

                // No direct tie? Run back out on the inputs' attached devices... 
                // Only the ones that are routing devices
                var midpointTieLines = destinationTieLines.Where(t => t.SourcePort.ParentDevice is IRoutingInputsOutputs);

                //Create a list for tracking already checked devices to avoid loops, if it doesn't already exist from previous iteration
                if (alreadyCheckedDevices == null)
                    alreadyCheckedDevices = new List<IRoutingInputsOutputs>();
                alreadyCheckedDevices.Add(destination as IRoutingInputsOutputs);

                foreach (var tieLine in midpointTieLines)
                {
                    var midpointDevice = tieLine.SourcePort.ParentDevice as IRoutingInputsOutputs;

                    // Check if this previous device has already been walked
                    if (alreadyCheckedDevices.Contains(midpointDevice))
                    {
                        Debug.LogMessage(LogEventLevel.Verbose, "Skipping input {0} on {1}, this was already checked", destination, midpointDevice.Key, destination.Key);
                        continue;
                    }

                    var midpointOutputPort = sourcePort ?? tieLine.SourcePort;

                    Debug.LogMessage(LogEventLevel.Verbose, "Trying to find route on {0}", destination, midpointDevice.Key);

                    // haven't seen this device yet.  Do it.  Pass the output port to the next
                    // level to enable switching on success
                    var upstreamRoutingSuccess = midpointDevice.GetRouteToSource(source, midpointOutputPort,
                        alreadyCheckedDevices, signalType, cycle, routeTable, null);

                    if (upstreamRoutingSuccess)
                    {
                        Debug.LogMessage(LogEventLevel.Verbose, "Upstream device route found", destination);
                        goodInputPort = tieLine.DestinationPort;
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

            if (sourcePort == null)
            {
                // it's a sink device
                routeTable.Routes.Add(new RouteSwitchDescriptor(goodInputPort));
            }
            else if (destination is IRouting)
            {
                routeTable.Routes.Add(new RouteSwitchDescriptor(sourcePort, goodInputPort));
            }
            else // device is merely IRoutingInputOutputs
                Debug.LogMessage(LogEventLevel.Verbose, "No routing. Passthrough device", destination);

            return true;
        }
    }
}