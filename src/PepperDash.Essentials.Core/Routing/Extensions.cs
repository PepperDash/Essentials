using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using PepperDash.Essentials.Core.Queues;
using PepperDash.Essentials.Core.Routing;
using Debug = PepperDash.Core.Debug;


namespace PepperDash.Essentials.Core
{

    /// <summary>
    /// Extensions added to any IRoutingInputs classes to provide discovery-based routing
    /// on those destinations.
    /// </summary>
    public static class Extensions
    {

        /// <summary>
        /// A collection of RouteDescriptors for each signal type.
        /// </summary>
        public static readonly Dictionary<eRoutingSignalType, RouteDescriptorCollection> RouteDescriptors = new Dictionary<eRoutingSignalType, RouteDescriptorCollection>()
        {
            { eRoutingSignalType.Audio, new RouteDescriptorCollection() },
            { eRoutingSignalType.Video, new RouteDescriptorCollection() },
            { eRoutingSignalType.SecondaryAudio, new RouteDescriptorCollection() },
            { eRoutingSignalType.AudioVideo, new RouteDescriptorCollection() },
            { eRoutingSignalType.UsbInput, new RouteDescriptorCollection() },
            { eRoutingSignalType.UsbOutput, new RouteDescriptorCollection() }
        };

        /// <summary>
        /// Stores pending route requests, keyed by the destination device key.
        /// Used primarily to handle routing requests while a device is cooling down.
        /// </summary>
        private static readonly Dictionary<string, RouteRequest> RouteRequests = new Dictionary<string, RouteRequest>();

        /// <summary>
        /// A queue to process route requests and releases sequentially.
        /// </summary>
        private static readonly GenericQueue routeRequestQueue = new GenericQueue("routingQueue");

        /// <summary>
        /// Indexed lookup of TieLines by destination device key for faster queries.
        /// </summary>
        private static Dictionary<string, List<TieLine>> _tieLinesByDestination;

        /// <summary>
        /// Indexed lookup of TieLines by source device key for faster queries.
        /// </summary>
        private static Dictionary<string, List<TieLine>> _tieLinesBySource;

        /// <summary>
        /// Cache of failed route attempts to avoid re-checking impossible paths.
        /// Format: "sourceKey|destKey|signalType"
        /// Uses ConcurrentDictionary as a thread-safe set (byte value is unused).
        /// </summary>
        private static readonly ConcurrentDictionary<string, byte> _impossibleRoutes = new ConcurrentDictionary<string, byte>();

        /// <summary>
        /// Indexes all TieLines by source and destination device keys for faster lookups.
        /// Should be called once at system startup after all TieLines are created.
        /// </summary>
        public static void IndexTieLines()
        {
            try
            {
                Debug.LogInformation("Indexing TieLines for faster route discovery");

                _tieLinesByDestination = TieLineCollection.Default
                    .GroupBy(t => t.DestinationPort.ParentDevice.Key)
                    .ToDictionary(g => g.Key, g => g.ToList());

                _tieLinesBySource = TieLineCollection.Default
                    .GroupBy(t => t.SourcePort.ParentDevice.Key)
                    .ToDictionary(g => g.Key, g => g.ToList());

                Debug.LogInformation("TieLine indexing complete. {0} destination keys, {1} source keys",
                    _tieLinesByDestination.Count, _tieLinesBySource.Count);
            }
            catch (Exception ex)
            {
                Debug.LogError("Exception indexing TieLines: {exception}", ex.Message);
                Debug.LogDebug(ex, "Stack Trace: ");
            }
        }

        /// <summary>
        /// Gets TieLines connected to a destination device.
        /// Uses indexed lookup if available, otherwise falls back to LINQ query.
        /// </summary>
        /// <param name="destinationKey">The destination device key</param>
        /// <returns>List of TieLines connected to the destination</returns>
        private static IEnumerable<TieLine> GetTieLinesForDestination(string destinationKey)
        {
            if (_tieLinesByDestination != null && _tieLinesByDestination.TryGetValue(destinationKey, out List<TieLine> tieLines))
            {
                return tieLines;
            }

            // Fallback to LINQ if index not available
            return TieLineCollection.Default.Where(t => t.DestinationPort.ParentDevice.Key == destinationKey);
        }

        /// <summary>
        /// Gets TieLines connected to a source device.
        /// Uses indexed lookup if available, otherwise falls back to LINQ query.
        /// </summary>
        /// <param name="sourceKey">The source device key</param>
        /// <returns>List of TieLines connected to the source</returns>
        private static IEnumerable<TieLine> GetTieLinesForSource(string sourceKey)
        {
            if (_tieLinesBySource != null && _tieLinesBySource.TryGetValue(sourceKey, out List<TieLine> tieLines))
            {
                return tieLines;
            }

            // Fallback to LINQ if index not available
            return TieLineCollection.Default.Where(t => t.SourcePort.ParentDevice.Key == sourceKey);
        }

        /// <summary>
        /// Creates a cache key for route impossibility tracking.
        /// </summary>
        /// <param name="sourceKey">Source device key</param>
        /// <param name="destKey">Destination device key</param>
        /// <param name="sourcePortKey">Source port key</param>
        /// <param name="destinationPortKey">Destination port key</param>
        /// <param name="type">Signal type</param>
        /// <returns>Cache key string</returns>
        private static string GetRouteKey(string sourceKey, string destKey, string sourcePortKey, string destinationPortKey, eRoutingSignalType type)
        {
            return $"{sourceKey}|{destKey}|{sourcePortKey}|{destinationPortKey}|{type}";
        }

        /// <summary>
        /// Clears the impossible routes cache. Should be called if TieLines are added/removed at runtime.
        /// </summary>
        public static void ClearImpossibleRoutesCache()
        {
            _impossibleRoutes.Clear();
            Debug.LogInformation("Impossible routes cache cleared");
        }

        /// <summary>
        /// Gets any existing RouteDescriptor for a destination, clears it using ReleaseRoute
        /// and then attempts a new Route and if sucessful, stores that RouteDescriptor
        /// in RouteDescriptorCollection.DefaultCollection
        /// </summary>        
        public static void ReleaseAndMakeRoute(this IRoutingInputs destination, IRoutingOutputs source, eRoutingSignalType signalType, string destinationPortKey = "", string sourcePortKey = "")
        {
            // Remove this line before committing!!!!!
            var frame = new StackFrame(1, true);
            Debug.LogInformation("ReleaseAndMakeRoute Called from {method} with params {destinationKey}:{sourceKey}:{signalType}:{destinationPortKey}:{sourcePortKey}", frame.GetMethod().Name, destination.Key, source.Key, signalType.ToString(), destinationPortKey, sourcePortKey);

            var inputPort = string.IsNullOrEmpty(destinationPortKey) ? null : destination.InputPorts.FirstOrDefault(p => p.Key == destinationPortKey);
            var outputPort = string.IsNullOrEmpty(sourcePortKey) ? null : source.OutputPorts.FirstOrDefault(p => p.Key == sourcePortKey);

            ReleaseAndMakeRoute(destination, source, signalType, inputPort, outputPort);
        }

        /// <summary>
        /// Will release the existing route to the destination, if a route is found. This does not CLEAR the route, only stop counting usage time on any output ports that have a usage tracker set.
        /// </summary>
        /// <param name="destination">destination to clear</param>
        public static void ReleaseRoute(this IRoutingInputs destination)
        {
            routeRequestQueue.Enqueue(new ReleaseRouteQueueItem(ReleaseRouteInternal, destination, string.Empty, false));
        }

        /// <summary>
        /// Will release the existing route to the destination, if a route is found. This does not CLEAR the route, only stop counting usage time on any output ports that have a usage tracker set
        /// </summary>
        /// <param name="destination">destination to clear</param>
        /// <param name="inputPortKey">Input to use to find existing route</param>
        /// <summary>
        /// ReleaseRoute method
        /// </summary>
        public static void ReleaseRoute(this IRoutingInputs destination, string inputPortKey)
        {
            routeRequestQueue.Enqueue(new ReleaseRouteQueueItem(ReleaseRouteInternal, destination, inputPortKey, false));
        }

        /// <summary>
        /// Clears the route on the destination.  This will remove any routes that are currently in use
        /// </summary>
        /// <param name="destination">Destination</param>
        public static void ClearRoute(this IRoutingInputs destination)
        {
            routeRequestQueue.Enqueue(new ReleaseRouteQueueItem(ReleaseRouteInternal, destination, string.Empty, true));
        }

        /// <summary>
        /// Clears the route on the destination.  This will remove any routes that are currently in use
        /// </summary>
        /// <param name="destination">destination</param>
        /// <param name="inputPortKey">input to use to find existing route</param>
        /// <summary>
        /// ClearRoute method
        /// </summary>
        public static void ClearRoute(this IRoutingInputs destination, string inputPortKey)
        {
            routeRequestQueue.Enqueue(new ReleaseRouteQueueItem(ReleaseRouteInternal, destination, inputPortKey, true));
        }

        /// <summary>
        /// Removes the route request for the destination.  This will remove any routes that are currently in use
        /// </summary>
        /// <param name="destinationKey">destination device key</param>
        public static void RemoveRouteRequestForDestination(string destinationKey)
        {
            Debug.LogInformation("Removing route request for {destination}", destinationKey);

            var result = RouteRequests.Remove(destinationKey);

            var messageTemplate = result ? "Route Request for {destination} removed" : "Route Request for {destination} not found";

            Debug.LogInformation(messageTemplate, destinationKey);
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
            if (!signalType.HasFlag(eRoutingSignalType.AudioVideo) &&
                !(signalType.HasFlag(eRoutingSignalType.Video) && signalType.HasFlag(eRoutingSignalType.SecondaryAudio)))
            {
                var singleTypeRouteDescriptor = new RouteDescriptor(source, destination, destinationPort, sourcePort, signalType);
                Debug.LogDebug(destination, "Attempting to build source route from {sourceKey} of type {type}", source.Key, signalType);

                if (!destination.GetRouteToSource(source, null, null, signalType, 0, singleTypeRouteDescriptor, destinationPort, sourcePort))
                    singleTypeRouteDescriptor = null;

                var routes = singleTypeRouteDescriptor?.Routes ?? new List<RouteSwitchDescriptor>();
                foreach (var route in routes)
                {
                    Debug.LogVerbose(destination, "Route for device: {route}", route.ToString());
                }

                return (singleTypeRouteDescriptor, null);
            }
            // otherwise, audioVideo needs to be handled as two steps.

            Debug.LogDebug(destination, "Attempting to build source route from {destinationKey} to {sourceKey} of type {type}", source.Key, signalType);

            RouteDescriptor audioRouteDescriptor;

            if (signalType.HasFlag(eRoutingSignalType.SecondaryAudio))
            {
                audioRouteDescriptor = new RouteDescriptor(source, destination, destinationPort, sourcePort, eRoutingSignalType.SecondaryAudio);
            }
            else
            {
                audioRouteDescriptor = new RouteDescriptor(source, destination, destinationPort, sourcePort, eRoutingSignalType.Audio);
            }

            var audioSuccess = destination.GetRouteToSource(source, null, null, signalType.HasFlag(eRoutingSignalType.SecondaryAudio) ? eRoutingSignalType.SecondaryAudio : eRoutingSignalType.Audio, 0, audioRouteDescriptor, destinationPort, sourcePort);

            if (!audioSuccess)
                Debug.LogDebug(destination, "Cannot find audio route to {0}", source.Key);

            var videoRouteDescriptor = new RouteDescriptor(source, destination, destinationPort, sourcePort, eRoutingSignalType.Video);

            var videoSuccess = destination.GetRouteToSource(source, null, null, eRoutingSignalType.Video, 0, videoRouteDescriptor, destinationPort, sourcePort);

            if (!videoSuccess)
                Debug.LogDebug(destination, "Cannot find video route to {0}", source.Key);

            foreach (var route in audioRouteDescriptor.Routes)
            {
                Debug.LogVerbose(destination, "Audio route for device: {route}", route.ToString());
            }

            foreach (var route in videoRouteDescriptor.Routes)
            {
                Debug.LogVerbose(destination, "Video route for device: {route}", route.ToString());
            }


            if (!audioSuccess && !videoSuccess)
                return (null, null);

            // Return null for descriptors that have no routes
            return (audioSuccess && audioRouteDescriptor.Routes.Count > 0 ? audioRouteDescriptor : null,
                    videoSuccess && videoRouteDescriptor.Routes.Count > 0 ? videoRouteDescriptor : null);
        }

        /// <summary>
        /// Internal method to handle the logic for releasing an existing route and making a new one.
        /// Handles devices with cooling states by queueing the request.
        /// </summary>
        /// <param name="destination">The destination device.</param>
        /// <param name="source">The source device.</param>
        /// <param name="signalType">The type of signal to route.</param>
        /// <param name="destinationPort">The specific destination input port (optional).</param>
        /// <param name="sourcePort">The specific source output port (optional).</param>
        private static void ReleaseAndMakeRoute(IRoutingInputs destination, IRoutingOutputs source, eRoutingSignalType signalType, RoutingInputPort destinationPort = null, RoutingOutputPort sourcePort = null)
        {
            if (destination == null) throw new ArgumentNullException(nameof(destination));
            if (source == null) throw new ArgumentNullException(nameof(source));
            if (destinationPort == null) Debug.LogDebug("Destination port is null");
            if (sourcePort == null) Debug.LogDebug("Source port is null");

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

                Debug.LogInformation("Device: {destination} is cooling down and already has a routing request stored.  Storing new route request to route to source key: {sourceKey}", destination.Key, routeRequest.Source.Key);

                return;
            }

            //New Request
            if (coolingDevice != null && coolingDevice.IsCoolingDownFeedback.BoolValue == true)
            {
                coolingDevice.IsCoolingDownFeedback.OutputChange += routeRequest.HandleCooldown;

                RouteRequests.Add(destination.Key, routeRequest);

                Debug.LogInformation("Device: {destination} is cooling down. Storing route request to route to source key: {sourceKey}", destination.Key, routeRequest.Source.Key);
                return;
            }

            if (RouteRequests.ContainsKey(destination.Key) && coolingDevice != null && coolingDevice.IsCoolingDownFeedback.BoolValue == false)
            {
                var handledRequest = RouteRequests[destination.Key];

                coolingDevice.IsCoolingDownFeedback.OutputChange -= handledRequest.HandleCooldown;

                RouteRequests.Remove(destination.Key);

                Debug.LogInformation("Device: {destination} is NOT cooling down.  Removing stored route request and routing to source key: {sourceKey}", destination.Key, routeRequest.Source.Key);
            }

            routeRequestQueue.Enqueue(new ReleaseRouteQueueItem(ReleaseRouteInternal, destination, destinationPort?.Key ?? string.Empty, false));

            routeRequestQueue.Enqueue(new RouteRequestQueueItem(RunRouteRequest, routeRequest));
        }

        /// <summary>
        /// Maps destination input ports to source output ports for all routing devices.
        /// </summary>
        public static void MapDestinationsToSources()
        {
            try
            {
                // Index TieLines before mapping if not already done
                if (_tieLinesByDestination == null || _tieLinesBySource == null)
                {
                    IndexTieLines();
                }

                var sinks = DeviceManager.AllDevices.OfType<IRoutingInputs>().Where(d => !(d is IRoutingInputsOutputs));
                var sources = DeviceManager.AllDevices.OfType<IRoutingOutputs>().Where(d => !(d is IRoutingInputsOutputs));

                foreach (var sink in sinks)
                {
                    foreach (var source in sources)
                    {
                        foreach (var inputPort in sink.InputPorts)
                        {
                            foreach (var outputPort in source.OutputPorts)
                            {
                                var (audioOrSingleRoute, videoRoute) = sink.GetRouteToSource(source, inputPort.Type, inputPort, outputPort);

                                if (audioOrSingleRoute == null && videoRoute == null)
                                {
                                    continue;
                                }

                                if (audioOrSingleRoute != null)
                                {
                                    // Only add routes that have actual switching steps
                                    if (audioOrSingleRoute.Routes == null || audioOrSingleRoute.Routes.Count == 0)
                                    {
                                        continue;
                                    }

                                    // Add to the appropriate collection(s) based on signal type
                                    // Note: A single route descriptor with combined flags (e.g., AudioVideo) will be added once per matching signal type
                                    if (audioOrSingleRoute.SignalType.HasFlag(eRoutingSignalType.Audio))
                                    {
                                        RouteDescriptors[eRoutingSignalType.Audio].AddRouteDescriptor(audioOrSingleRoute);
                                    }
                                    if (audioOrSingleRoute.SignalType.HasFlag(eRoutingSignalType.Video))
                                    {
                                        RouteDescriptors[eRoutingSignalType.Video].AddRouteDescriptor(audioOrSingleRoute);
                                    }
                                    if (audioOrSingleRoute.SignalType.HasFlag(eRoutingSignalType.SecondaryAudio))
                                    {
                                        RouteDescriptors[eRoutingSignalType.SecondaryAudio].AddRouteDescriptor(audioOrSingleRoute);
                                    }
                                    if (audioOrSingleRoute.SignalType.HasFlag(eRoutingSignalType.UsbInput))
                                    {
                                        RouteDescriptors[eRoutingSignalType.UsbInput].AddRouteDescriptor(audioOrSingleRoute);
                                    }
                                    if (audioOrSingleRoute.SignalType.HasFlag(eRoutingSignalType.UsbOutput))
                                    {
                                        RouteDescriptors[eRoutingSignalType.UsbOutput].AddRouteDescriptor(audioOrSingleRoute);
                                    }
                                }
                                if (videoRoute != null)
                                {
                                    // Only add routes that have actual switching steps
                                    if (videoRoute.Routes == null || videoRoute.Routes.Count == 0)
                                    {
                                        continue;
                                    }

                                    RouteDescriptors[eRoutingSignalType.Video].AddRouteDescriptor(videoRoute);
                                }
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Debug.LogError("Exception mapping routes: {exception}", ex.Message);
                Debug.LogDebug(ex, "Stack Trace: ");
            }
        }

        /// <summary>
        /// Executes the actual routing based on a <see cref="RouteRequest"/>.
        /// Finds the route path, adds it to the collection, and executes the switches.
        /// </summary>
        /// <param name="request">The route request details.</param>
        private static void RunRouteRequest(RouteRequest request)
        {
            try
            {
                if (request.Source == null)
                    return;

                RouteDescriptor audioOrSingleRoute = null;
                RouteDescriptor videoRoute = null;

                // Try to use pre-loaded route descriptors first
                if (request.SignalType.HasFlag(eRoutingSignalType.AudioVideo))
                {
                    // For AudioVideo routes, check both Audio and Video collections
                    if (RouteDescriptors.TryGetValue(eRoutingSignalType.Audio, out RouteDescriptorCollection audioCollection))
                    {
                        audioOrSingleRoute = audioCollection.Descriptors.FirstOrDefault(d =>
                            d.Source.Key == request.Source.Key &&
                            d.Destination.Key == request.Destination.Key &&
                            (request.DestinationPort == null || d.InputPort?.Key == request.DestinationPort.Key) &&
                            (request.SourcePort == null || d.OutputPort?.Key == request.SourcePort.Key));
                    }

                    if (RouteDescriptors.TryGetValue(eRoutingSignalType.Video, out RouteDescriptorCollection videoCollection))
                    {
                        videoRoute = videoCollection.Descriptors.FirstOrDefault(d =>
                            d.Source.Key == request.Source.Key &&
                            d.Destination.Key == request.Destination.Key &&
                            (request.DestinationPort == null || d.InputPort?.Key == request.DestinationPort.Key) &&
                            (request.SourcePort == null || d.OutputPort?.Key == request.SourcePort.Key));
                    }
                }
                else
                {
                    // For single signal type routes
                    var signalTypeToCheck = request.SignalType.HasFlag(eRoutingSignalType.SecondaryAudio)
                        ? eRoutingSignalType.SecondaryAudio
                        : request.SignalType;

                    if (RouteDescriptors.TryGetValue(signalTypeToCheck, out RouteDescriptorCollection collection))
                    {
                        audioOrSingleRoute = collection.Descriptors.FirstOrDefault(d =>
                            d.Source.Key == request.Source.Key &&
                            d.Destination.Key == request.Destination.Key &&
                            (request.DestinationPort == null || d.InputPort?.Key == request.DestinationPort.Key) &&
                            (request.SourcePort == null || d.OutputPort?.Key == request.SourcePort.Key));
                    }
                }

                // If no pre-loaded route found, build it dynamically
                if (audioOrSingleRoute == null && videoRoute == null)
                {
                    Debug.LogDebug(request.Destination, "No pre-loaded route found, building dynamically");
                    (audioOrSingleRoute, videoRoute) = request.Destination.GetRouteToSource(request.Source, request.SignalType, request.DestinationPort, request.SourcePort);
                }

                if (audioOrSingleRoute == null && videoRoute == null)
                    return;

                RouteDescriptorCollection.DefaultCollection.AddRouteDescriptor(audioOrSingleRoute);

                if (videoRoute != null)
                {
                    RouteDescriptorCollection.DefaultCollection.AddRouteDescriptor(videoRoute);
                }

                Debug.LogVerbose(request.Destination, "Executing full route");

                audioOrSingleRoute.ExecuteRoutes();
                videoRoute?.ExecuteRoutes();
            }
            catch (Exception ex)
            {
                Debug.LogError("Exception Running Route Request {request}: {exception}", request, ex.Message);
                Debug.LogDebug(ex, "Stack Trace: ");
            }
        }

        /// <summary>
        /// Will release the existing route on the destination, if it is found in RouteDescriptorCollection.DefaultCollection
        /// </summary>
        /// <param name="destination"></param>     
        /// <param name="inputPortKey"> The input port key to use to find the route.  If empty, will use the first available input port</param>
        /// <param name="clearRoute"> If true, will clear the route on the destination.  This will remove any routes that are currently in use</param>
        private static void ReleaseRouteInternal(IRoutingInputs destination, string inputPortKey, bool clearRoute)
        {
            try
            {
                Debug.LogInformation(destination, "Release route for '{destination}':'{inputPortKey}'", destination?.Key ?? null, string.IsNullOrEmpty(inputPortKey) ? "auto" : inputPortKey);

                if (RouteRequests.TryGetValue(destination.Key, out RouteRequest existingRequest) && destination is IWarmingCooling)
                {
                    var coolingDevice = destination as IWarmingCooling;

                    coolingDevice.IsCoolingDownFeedback.OutputChange -= existingRequest.HandleCooldown;
                }

                RouteRequests.Remove(destination.Key);

                var current = RouteDescriptorCollection.DefaultCollection.RemoveRouteDescriptor(destination, inputPortKey);
                if (current != null)
                {
                    Debug.LogInformation(destination, "Releasing current route: {0}", current.Source.Key);
                    current.ReleaseRoutes(clearRoute);
                }
            }
            catch (Exception ex)
            {
                Debug.LogError("Exception releasing route for '{destination}':'{inputPortKey}': {exception}", destination?.Key ?? null, string.IsNullOrEmpty(inputPortKey) ? "auto" : inputPortKey, ex.Message);
                Debug.LogDebug(ex, "Stack Trace: ");
            }
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
        /// <param name="destinationPort">The RoutingOutputPort whose link is being checked for a route</param>
        /// <param name="sourcePort">The source output port (optional)</param>
        /// <returns>true if source is hit</returns>
        private static bool GetRouteToSource(this IRoutingInputs destination, IRoutingOutputs source,
            RoutingOutputPort outputPortToUse, List<IRoutingInputsOutputs> alreadyCheckedDevices,
                eRoutingSignalType signalType, int cycle, RouteDescriptor routeTable, RoutingInputPort destinationPort, RoutingOutputPort sourcePort)
        {
            cycle++;

            // Check if this route has already been determined to be impossible
            var routeKey = GetRouteKey(source.Key, destination.Key, sourcePort?.Key ?? "auto", destinationPort?.Key ?? "auto", signalType);
            if (_impossibleRoutes.ContainsKey(routeKey))
            {
                Debug.LogVerbose("Route {0} is cached as impossible, skipping", routeKey);
                return false;
            }

            Debug.LogVerbose("GetRouteToSource: {cycle} {sourceKey}:{sourcePortKey}--> {destinationKey}:{destinationPortKey} {type}", null, cycle, source.Key, sourcePort?.Key ?? "auto", destination.Key, destinationPort?.Key ?? "auto", signalType.ToString());

            RoutingInputPort goodInputPort = null;

            // Use indexed lookup instead of LINQ query
            var allDestinationTieLines = GetTieLinesForDestination(destination.Key);

            IEnumerable<TieLine> destinationTieLines;
            TieLine directTie = null;

            if (destinationPort == null)
            {
                destinationTieLines = allDestinationTieLines.Where(t =>
                    t.Type.HasFlag(signalType) || signalType == eRoutingSignalType.AudioVideo);
            }
            else
            {
                destinationTieLines = allDestinationTieLines.Where(t =>
                    t.DestinationPort.Key == destinationPort.Key && t.Type.HasFlag(signalType));
            }

            // find the TieLine without a port
            if (destinationPort == null && sourcePort == null)
            {
                directTie = destinationTieLines.FirstOrDefault(t => t.SourcePort.ParentDevice.Key == source.Key);
            }
            // find a tieLine to a specific destination port without a specific source port
            else if (destinationPort != null && sourcePort == null)
            {
                directTie = destinationTieLines.FirstOrDefault(t => t.DestinationPort.Key == destinationPort.Key && t.SourcePort.ParentDevice.Key == source.Key);
            }
            // find a tieline to a specific source port without a specific destination port
            else if (destinationPort == null & sourcePort != null)
            {
                directTie = destinationTieLines.FirstOrDefault(t => t.SourcePort.ParentDevice.Key == source.Key && t.SourcePort.Key == sourcePort.Key);
            }
            // find a tieline to a specific source port and destination port
            else if (destinationPort != null && sourcePort != null)
            {
                directTie = destinationTieLines.FirstOrDefault(t => t.DestinationPort.Key == destinationPort.Key && t.SourcePort.ParentDevice.Key == source.Key && t.SourcePort.Key == sourcePort.Key);
            }

            if (directTie != null) // Found a tie directly to the source
            {
                goodInputPort = directTie.DestinationPort;
            }
            else // no direct-connect.  Walk back devices.
            {
                Debug.LogVerbose(destination, "is not directly connected to {sourceKey}. Walking down tie lines", source.Key);

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
                        Debug.LogVerbose(destination, "Skipping input {midpointDeviceKey} on {destinationKey}, this was already checked", midpointDevice.Key, destination.Key);
                        continue;
                    }

                    var midpointOutputPort = tieLine.SourcePort;

                    Debug.LogVerbose(destination, "Trying to find route on {midpointDeviceKey}", midpointDevice.Key);

                    // haven't seen this device yet.  Do it.  Pass the output port to the next
                    // level to enable switching on success
                    var upstreamRoutingSuccess = midpointDevice.GetRouteToSource(source, midpointOutputPort,
                        alreadyCheckedDevices, signalType, cycle, routeTable, null, sourcePort);

                    if (upstreamRoutingSuccess)
                    {
                        Debug.LogVerbose(destination, "Upstream device route found");
                        Debug.LogVerbose(destination, "Route found on {midpointDeviceKey}", midpointDevice.Key);
                        Debug.LogVerbose(destination, "TieLine: SourcePort: {SourcePort} DestinationPort: {DestinationPort}", tieLine.SourcePort, tieLine.DestinationPort);
                        goodInputPort = tieLine.DestinationPort;
                        break; // Stop looping the inputs in this cycle
                    }
                }
            }


            if (goodInputPort == null)
            {
                Debug.LogVerbose(destination, "No route found to {0}", source.Key);

                // Cache this as an impossible route
                _impossibleRoutes.TryAdd(routeKey, 0);

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
                Debug.LogVerbose(destination, "No routing. Passthrough device");

            return true;
        }
    }
}