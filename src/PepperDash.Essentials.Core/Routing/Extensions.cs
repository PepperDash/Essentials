﻿using Serilog.Events;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Debug = PepperDash.Core.Debug;


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
            // Remove this line before committing!!!!!
            var frame = new StackFrame(1, true);
            Debug.LogMessage(LogEventLevel.Information, "ReleaseAndMakeRoute Called from {method} with params {destinationKey}:{sourceKey}:{signalType}:{destinationPortKey}:{sourcePortKey}", frame.GetMethod().Name, destination.Key, source.Key, signalType.ToString(), destinationPortKey, sourcePortKey);

            var inputPort = string.IsNullOrEmpty(destinationPortKey) ? null : destination.InputPorts.FirstOrDefault(p => p.Key == destinationPortKey);
            var outputPort = string.IsNullOrEmpty(sourcePortKey) ? null : source.OutputPorts.FirstOrDefault(p => p.Key == sourcePortKey);

            ReleaseAndMakeRoute(destination, source, signalType, inputPort, outputPort);
        }

        public static void RemoveRouteRequestForDestination(string destinationKey)
        {
            Debug.LogMessage(LogEventLevel.Information, "Removing route request for {destination}", null, destinationKey);

            var result = RouteRequests.Remove(destinationKey);

            var messageTemplate = result ? "Route Request for {destination} removed" : "Route Request for {destination} not found";

            Debug.LogMessage(LogEventLevel.Information, messageTemplate, null, destinationKey);
        }

        private static void ReleaseAndMakeRoute(IRoutingInputs destination, IRoutingOutputs source, eRoutingSignalType signalType, RoutingInputPort destinationPort = null, RoutingOutputPort sourcePort = null)
        {
            if (destination == null) throw new ArgumentNullException(nameof(destination));
            if (source == null) throw new ArgumentNullException(nameof(source));
            if (destinationPort == null) Debug.LogMessage(LogEventLevel.Information, "Destination port is null");
            if (sourcePort == null) Debug.LogMessage(LogEventLevel.Information, "Source port is null");

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

                Debug.LogMessage(LogEventLevel.Information, "Device: {destination} is cooling down and already has a routing request stored.  Storing new route request to route to source key: {sourceKey}", null, destination.Key, routeRequest.Source.Key);

                return;
            }

            //New Request
            if (coolingDevice != null && coolingDevice.IsCoolingDownFeedback.BoolValue == true)
            {
                coolingDevice.IsCoolingDownFeedback.OutputChange -= routeRequest.HandleCooldown;

                coolingDevice.IsCoolingDownFeedback.OutputChange += routeRequest.HandleCooldown;

                RouteRequests.Add(destination.Key, routeRequest);

                Debug.LogMessage(LogEventLevel.Information, "Device: {destination} is cooling down.  Storing route request to route to source key: {sourceKey}", null, destination.Key, routeRequest.Source.Key);
                return;
            }

            if (RouteRequests.ContainsKey(destination.Key) && coolingDevice != null && coolingDevice.IsCoolingDownFeedback.BoolValue == false)
            {
                RouteRequests.Remove(destination.Key);
                Debug.LogMessage(LogEventLevel.Information, "Device: {destination} is NOT cooling down.  Removing stored route request and routing to source key: {sourceKey}", null, destination.Key, routeRequest.Source.Key);
            }

            destination.ReleaseRoute();

            RunRouteRequest(routeRequest);
        }

        private static void RunRouteRequest(RouteRequest request)
        {
            if (request.Source == null)
                return;

            var (audioOrSingleRoute, videoRoute) = request.Destination.GetRouteToSource(request.Source, request.SignalType, request.DestinationPort, request.SourcePort);

            if (audioOrSingleRoute == null && videoRoute == null)
                return;

            RouteDescriptorCollection.DefaultCollection.AddRouteDescriptor(audioOrSingleRoute);

            if (videoRoute != null)
            {
                RouteDescriptorCollection.DefaultCollection.AddRouteDescriptor(videoRoute);
            }

            Debug.LogMessage(LogEventLevel.Verbose, "Executing full route", request.Destination);

            audioOrSingleRoute.ExecuteRoutes();
            videoRoute?.ExecuteRoutes();
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
        public static (RouteDescriptor, RouteDescriptor) GetRouteToSource(this IRoutingInputs destination, IRoutingOutputs source, eRoutingSignalType signalType, RoutingInputPort destinationPort, RoutingOutputPort sourcePort)
        {
            // if it's a single signal type, find the route
            if (!signalType.HasFlag(eRoutingSignalType.AudioVideo))
            {
                var singleTypeRouteDescriptor = new RouteDescriptor(source, destination, signalType);
                Debug.LogMessage(LogEventLevel.Debug, "Attempting to build source route from {sourceKey} of type {type}", destination, source.Key, signalType);

                if (!destination.GetRouteToSource(source, null, null, signalType, 0, singleTypeRouteDescriptor, destinationPort, sourcePort))
                    singleTypeRouteDescriptor = null;

                foreach (var route in singleTypeRouteDescriptor.Routes)
                {
                    Debug.LogMessage(LogEventLevel.Verbose, "Route for device: {route}", destination, route.ToString());
                }

                return (singleTypeRouteDescriptor, null);
            }
            // otherwise, audioVideo needs to be handled as two steps.

            Debug.LogMessage(LogEventLevel.Debug, "Attempting to build source route from {sourceKey} of type {type}", destination, source.Key);

            var audioRouteDescriptor = new RouteDescriptor(source, destination, eRoutingSignalType.Audio);

            var audioSuccess = destination.GetRouteToSource(source, null, null, eRoutingSignalType.Audio, 0, audioRouteDescriptor, destinationPort, sourcePort);

            if (!audioSuccess)
                Debug.LogMessage(LogEventLevel.Debug, "Cannot find audio route to {0}", destination, source.Key);

            var videoRouteDescriptor = new RouteDescriptor(source, destination, eRoutingSignalType.Video);

            var videoSuccess = destination.GetRouteToSource(source, null, null, eRoutingSignalType.Video, 0, videoRouteDescriptor, destinationPort, sourcePort);

            if (!videoSuccess)
                Debug.LogMessage(LogEventLevel.Debug, "Cannot find video route to {0}", destination, source.Key);

            foreach (var route in audioRouteDescriptor.Routes)
            {
                Debug.LogMessage(LogEventLevel.Verbose, "Audio route for device: {route}", destination, route.ToString());
            }

            foreach (var route in videoRouteDescriptor.Routes)
            {
                Debug.LogMessage(LogEventLevel.Verbose, "Video route for device: {route}", destination, route.ToString());
            }


            if (!audioSuccess && !videoSuccess)
                return (null, null);


            return (audioRouteDescriptor, videoRouteDescriptor);
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
            RoutingOutputPort outputPortToUse, List<IRoutingInputsOutputs> alreadyCheckedDevices,
                eRoutingSignalType signalType, int cycle, RouteDescriptor routeTable, RoutingInputPort destinationPort, RoutingOutputPort sourcePort)
        {
            cycle++;

            Debug.LogMessage(LogEventLevel.Verbose, "GetRouteToSource: {cycle} {sourceKey}:{sourcePortKey}--> {destinationKey}:{destinationPortKey} {type}", null, cycle, source.Key, sourcePort?.Key ?? "auto", destination.Key, destinationPort?.Key ?? "auto", signalType.ToString());

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
                destinationTieLines = TieLineCollection.Default.Where(t => t.DestinationPort.ParentDevice.Key == destination.Key && t.DestinationPort.Key == destinationPort.Key && (t.Type == signalType || t.Type.HasFlag(eRoutingSignalType.AudioVideo)));
            }

            // find the TieLine without a port
            if (destinationPort == null && sourcePort == null)
            {
                directTie = destinationTieLines.FirstOrDefault(t => t.DestinationPort.ParentDevice.Key == destination.Key && t.SourcePort.ParentDevice.Key == source.Key);
            }
            // find a tieLine to a specific destination port without a specific source port
            else if (destinationPort != null && sourcePort == null)
            {
                directTie = destinationTieLines.FirstOrDefault(t => t.DestinationPort.ParentDevice.Key == destination.Key && t.DestinationPort.Key == destinationPort.Key && t.SourcePort.ParentDevice.Key == source.Key);
            }
            // find a tieline to a specific source port without a specific destination port
            else if (destinationPort == null & sourcePort != null)
            {
                directTie = destinationTieLines.FirstOrDefault(t => t.DestinationPort.ParentDevice.Key == destination.Key && t.SourcePort.ParentDevice.Key == source.Key && t.SourcePort.Key == sourcePort.Key);
            }
            // find a tieline to a specific source port and destination port
            else if (destinationPort != null && sourcePort != null)
            {
                directTie = destinationTieLines.FirstOrDefault(t => t.DestinationPort.ParentDevice.Key == destination.Key && t.DestinationPort.Key == destinationPort.Key && t.SourcePort.ParentDevice.Key == source.Key && t.SourcePort.Key == sourcePort.Key);
            }

            if (directTie != null) // Found a tie directly to the source
            {
                goodInputPort = directTie.DestinationPort;
            }
            else // no direct-connect.  Walk back devices.
            {
                Debug.LogMessage(LogEventLevel.Verbose, "is not directly connected to {sourceKey}. Walking down tie lines", destination, source.Key);

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
                        Debug.LogMessage(LogEventLevel.Verbose, "Skipping input {midpointDeviceKey} on {destinationKey}, this was already checked", destination, midpointDevice.Key, destination.Key);
                        continue;
                    }

                    var midpointOutputPort = tieLine.SourcePort;

                    Debug.LogMessage(LogEventLevel.Verbose, "Trying to find route on {midpointDeviceKey}", destination, midpointDevice.Key);

                    // haven't seen this device yet.  Do it.  Pass the output port to the next
                    // level to enable switching on success
                    var upstreamRoutingSuccess = midpointDevice.GetRouteToSource(source, midpointOutputPort,
                        alreadyCheckedDevices, signalType, cycle, routeTable, null, sourcePort);

                    if (upstreamRoutingSuccess)
                    {
                        Debug.LogMessage(LogEventLevel.Verbose, "Upstream device route found", destination);
                        Debug.LogMessage(LogEventLevel.Verbose, "Route found on {midpointDeviceKey}", destination, midpointDevice.Key);
                        Debug.LogMessage(LogEventLevel.Verbose, "TieLine: SourcePort: {SourcePort} DestinationPort: {DestinationPort}", destination, tieLine.SourcePort, tieLine.DestinationPort);
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

            if (destination is IRoutingSink)
            {
                // it's a sink device
                routeTable.Routes.Add(new RouteSwitchDescriptor(goodInputPort));
            }
            else if (destination is IRouting)
            {
                routeTable.Routes.Add(new RouteSwitchDescriptor(outputPortToUse, goodInputPort));
            }
            else // device is merely IRoutingInputOutputs
                Debug.LogMessage(LogEventLevel.Verbose, "No routing. Passthrough device", destination);

            return true;
        }
    }
}