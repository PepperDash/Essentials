using System;
using System.Collections.Generic;
using System.Linq;
using System.Timers;
using PepperDash.Core;
using PepperDash.Essentials.Core.Config;

namespace PepperDash.Essentials.Core.Routing
{
    /// <summary>
    /// Manages routing feedback by subscribing to route changes on midpoint and sink devices,
    /// tracing the route back to the original source, and updating the CurrentSourceInfo on sink devices.
    /// </summary>
    public class RoutingFeedbackManager : EssentialsDevice
    {
        /// <summary>
        /// Maps midpoint device keys to the set of sink device keys that are downstream
        /// </summary>
        private Dictionary<string, HashSet<string>> midpointToSinksMap;

        /// <summary>
        /// Debounce timers for each sink device to prevent rapid successive updates
        /// </summary>
        private readonly Dictionary<string, Timer> updateTimers = new Dictionary<string, Timer>();

        /// <summary>
        /// Lock object protecting all access to <see cref="updateTimers"/>.
        /// </summary>
        private readonly object _timerLock = new object();

        /// <summary>
        /// Debounce delay in milliseconds
        /// </summary>
        private const long DEBOUNCE_MS = 500;

        /// <summary>
        /// Initializes a new instance of the <see cref="RoutingFeedbackManager"/> class.
        /// </summary>
        /// <param name="key">The unique key for this manager device.</param>
        /// <param name="name">The name of this manager device.</param>
        public RoutingFeedbackManager(string key, string name)
            : base(key, name)
        {
            AddPreActivationAction(BuildMidpointSinkMap);
            AddPreActivationAction(SubscribeForMidpointFeedback);
            AddPreActivationAction(SubscribeForSinkFeedback);
        }

        /// <summary>
        /// Builds a map of which sink devices are downstream of each midpoint device
        /// for performance optimization in HandleMidpointUpdate
        /// </summary>
        private void BuildMidpointSinkMap()
        {
            midpointToSinksMap = new Dictionary<string, HashSet<string>>();

            var sinks = DeviceManager.AllDevices.OfType<IRoutingSinkWithSwitchingWithInputPort>();

            foreach (var sink in sinks)
            {
                if (sink.CurrentInputPort == null)
                    continue;

                // Find all upstream midpoints for this sink
                var upstreamMidpoints = GetUpstreamMidpoints(sink);

                foreach (var midpointKey in upstreamMidpoints)
                {
                    if (!midpointToSinksMap.ContainsKey(midpointKey))
                        midpointToSinksMap[midpointKey] = new HashSet<string>();

                    midpointToSinksMap[midpointKey].Add(sink.Key);
                }
            }

            Debug.LogMessage(
                Serilog.Events.LogEventLevel.Information,
                "Built midpoint-to-sink map with {count} midpoints",
                this,
                midpointToSinksMap.Count
            );
        }

        /// <summary>
        /// Gets all upstream midpoint device keys for a given sink
        /// </summary>
        private HashSet<string> GetUpstreamMidpoints(IRoutingSinkWithSwitchingWithInputPort sink)
        {
            var result = new HashSet<string>();
            var visited = new HashSet<string>();

            if (sink.CurrentInputPort == null)
                return result;

            var tieLine = TieLineCollection.Default.FirstOrDefault(tl =>
                tl.DestinationPort.Key == sink.CurrentInputPort.Key &&
                tl.DestinationPort.ParentDevice.Key == sink.CurrentInputPort.ParentDevice.Key);

            if (tieLine == null)
                return result;

            TraceUpstreamMidpoints(tieLine, result, visited);
            return result;
        }

        /// <summary>
        /// Recursively traces upstream to find all midpoint devices
        /// </summary>
        private void TraceUpstreamMidpoints(TieLine tieLine, HashSet<string> midpoints, HashSet<string> visited)
        {
            if (tieLine == null || visited.Contains(tieLine.SourcePort.ParentDevice.Key))
                return;

            visited.Add(tieLine.SourcePort.ParentDevice.Key);

            if (tieLine.SourcePort.ParentDevice is IRoutingWithFeedback midpoint)
            {
                midpoints.Add(midpoint.Key);

                // Find upstream TieLines connected to this midpoint's inputs
                var midpointInputs = (midpoint as IRoutingInputs)?.InputPorts;
                if (midpointInputs != null)
                {
                    foreach (var inputPort in midpointInputs)
                    {
                        var upstreamTieLine = TieLineCollection.Default.FirstOrDefault(tl =>
                            tl.DestinationPort.Key == inputPort.Key &&
                            tl.DestinationPort.ParentDevice.Key == inputPort.ParentDevice.Key);

                        if (upstreamTieLine != null)
                            TraceUpstreamMidpoints(upstreamTieLine, midpoints, visited);
                    }
                }
            }
        }

        /// <summary>
        /// Subscribes to the RouteChanged event on all devices implementing <see cref="IRoutingWithFeedback"/>.
        /// </summary>
        private void SubscribeForMidpointFeedback()
        {
            var midpointDevices = DeviceManager.AllDevices.OfType<IRoutingWithFeedback>();

            foreach (var device in midpointDevices)
            {
                device.RouteChanged += HandleMidpointUpdate;
            }
        }

        /// <summary>
        /// Subscribes to the InputChanged event on all devices implementing <see cref="IRoutingSinkWithSwitchingWithInputPort"/>.
        /// </summary>
        private void SubscribeForSinkFeedback()
        {
            var sinkDevices =
                DeviceManager.AllDevices.OfType<IRoutingSinkWithSwitchingWithInputPort>();

            foreach (var device in sinkDevices)
            {
                device.InputChanged += HandleSinkUpdate;
            }
        }

        /// <summary>
        /// Handles the RouteChanged event from a midpoint device.
        /// Only triggers updates for sink devices that are downstream of this midpoint.
        /// </summary>
        /// <param name="midpoint">The midpoint device that reported a route change.</param>
        /// <param name="newRoute">The descriptor of the new route.</param>
        private void HandleMidpointUpdate(
            IRoutingWithFeedback midpoint,
            RouteSwitchDescriptor newRoute
        )
        {
            try
            {
                // Only update affected sinks (performance optimization)
                if (midpointToSinksMap != null && midpointToSinksMap.TryGetValue(midpoint.Key, out var affectedSinkKeys))
                {
                    Debug.LogMessage(
                        Serilog.Events.LogEventLevel.Debug,
                        "Midpoint {midpoint} changed, updating {count} downstream sinks",
                        this,
                        midpoint.Key,
                        affectedSinkKeys.Count
                    );

                    foreach (var sinkKey in affectedSinkKeys)
                    {
                        if (DeviceManager.GetDeviceForKey(sinkKey) is IRoutingSinkWithSwitchingWithInputPort sink)
                        {
                            UpdateDestination(sink, sink.CurrentInputPort);
                        }
                    }
                }
                else
                {
                    Debug.LogMessage(
                        Serilog.Events.LogEventLevel.Debug,
                        "Midpoint {midpoint} changed but has no downstream sinks in map",
                        this,
                        midpoint.Key
                    );
                }
            }
            catch (Exception ex)
            {
                Debug.LogMessage(
                    ex,
                    "Error handling midpoint update from {midpointKey}:{Exception}",
                    this,
                    midpoint.Key,
                    ex
                );
            }
        }

        /// <summary>
        /// Removes a sink from every midpoint set in the map and re-adds it based on its
        /// current input port. Call this whenever a sink's selected input changes so that
        /// HandleMidpointUpdate always sees an up-to-date downstream set.
        /// </summary>
        private void RebuildMapForSink(IRoutingSinkWithSwitchingWithInputPort sink)
        {
            if (midpointToSinksMap == null)
                return;

            // Remove this sink from all existing midpoint sets
            foreach (var set in midpointToSinksMap.Values)
                set.Remove(sink.Key);

            // Drop any midpoint entries that are now empty
            var emptyKeys = midpointToSinksMap
                .Where(kvp => kvp.Value.Count == 0)
                .Select(kvp => kvp.Key)
                .ToList();
            foreach (var k in emptyKeys)
                midpointToSinksMap.Remove(k);

            // Re-add the sink under every midpoint that is upstream of its new input
            if (sink.CurrentInputPort == null)
                return;

            var upstreamMidpoints = GetUpstreamMidpoints(sink);
            foreach (var midpointKey in upstreamMidpoints)
            {
                if (!midpointToSinksMap.ContainsKey(midpointKey))
                    midpointToSinksMap[midpointKey] = new HashSet<string>();

                midpointToSinksMap[midpointKey].Add(sink.Key);
            }
        }

        /// <summary>
        /// Handles the InputChanged event from a sink device.
        /// Updates the midpoint-to-sink map for the new input path, then triggers
        /// a source-info update for the sink.
        /// </summary>
        /// <param name="sender">The sink device that reported an input change.</param>
        /// <param name="currentInputPort">The new input port selected on the sink device.</param>
        private void HandleSinkUpdate(
            IRoutingSinkWithSwitching sender,
            RoutingInputPort currentInputPort
        )
        {
            try
            {
                // Keep the map current so HandleMidpointUpdate can find this sink
                if (sender is IRoutingSinkWithSwitchingWithInputPort sinkWithInputPort)
                    RebuildMapForSink(sinkWithInputPort);

                UpdateDestination(sender, currentInputPort);
            }
            catch (Exception ex)
            {
                Debug.LogMessage(
                    ex,
                    "Error handling Sink update from {senderKey}:{Exception}",
                    this,
                    sender.Key,
                    ex
                );
            }
        }

        /// <summary>
        /// Updates the CurrentSourceInfo and CurrentSourceInfoKey properties on a destination (sink) device
        /// based on its currently selected input port by tracing the route back through tie lines.
        /// Uses debouncing to prevent rapid successive updates.
        /// </summary>
        /// <param name="destination">The destination sink device to update.</param>
        /// <param name="inputPort">The currently selected input port on the destination device.</param>
        private void UpdateDestination(
            IRoutingSinkWithSwitching destination,
            RoutingInputPort inputPort
        )
        {
            if (destination == null)
                return;

            var key = destination.Key;

            // Cancel and replace any existing timer under the lock so no callback
            // can race with us while we swap the entry.
            Timer timerToDispose = null;
            Timer newTimer = null;

            newTimer = new Timer(DEBOUNCE_MS) { AutoReset = false };
            newTimer.Elapsed += (s, e) =>
            {
                try
                {
                    UpdateDestinationImmediate(destination, inputPort);
                }
                catch (Exception ex)
                {
                    Debug.LogMessage(
                        ex,
                        "Error in debounced update for destination {destinationKey}: {message}",
                        this,
                        destination.Key,
                        ex.Message
                    );
                }
                finally
                {
                    // Remove the entry first so a concurrent UpdateDestination call
                    // cannot re-dispose whatever timer we're about to dispose.
                    Timer selfTimer = null;
                    lock (_timerLock)
                    {
                        if (updateTimers.TryGetValue(key, out var current) && ReferenceEquals(current, newTimer))
                        {
                            selfTimer = current;
                            updateTimers.Remove(key);
                        }
                    }
                    selfTimer?.Dispose();
                }
            };

            lock (_timerLock)
            {
                if (updateTimers.TryGetValue(key, out var existingTimer))
                    timerToDispose = existingTimer;

                updateTimers[key] = newTimer;
            }

            // Dispose the old timer outside the lock to avoid holding the lock during disposal.
            // Dispose implicitly stops the timer, preventing its Elapsed event from firing.
            timerToDispose?.Dispose();

            // Start after the lock is released so the Elapsed callback cannot deadlock
            // trying to acquire _timerLock while we still hold it.
            newTimer.Start();
        }

        /// <summary>
        /// Immediately updates the CurrentSourceInfo for a destination device.
        /// Called after debounce delay.
        /// </summary>
        private void UpdateDestinationImmediate(
            IRoutingSinkWithSwitching destination,
            RoutingInputPort inputPort
        )
        {
            Debug.LogMessage(
                Serilog.Events.LogEventLevel.Debug,
                "Updating destination {destination} with inputPort {inputPort}",
                this,
                destination?.Key,
                inputPort?.Key
            );

            if (inputPort == null)
            {
                Debug.LogMessage(
                    Serilog.Events.LogEventLevel.Debug,
                    "Destination {destination} has not reported an input port yet",
                    this,
                    destination.Key
                );
                return;
            }

            TieLine firstTieLine;
            try
            {
                var tieLines = TieLineCollection.Default;

                firstTieLine = tieLines.FirstOrDefault(tl =>
                    tl.DestinationPort.Key == inputPort.Key
                    && tl.DestinationPort.ParentDevice.Key == inputPort.ParentDevice.Key
                );

                if (firstTieLine == null)
                {
                    Debug.LogMessage(
                        Serilog.Events.LogEventLevel.Debug,
                        "No tieline found for inputPort {inputPort}. Clearing current source",
                        this,
                        inputPort
                    );

                    var tempSourceListItem = new SourceListItem
                    {
                        SourceKey = "$transient",
                        Name = inputPort.Key,
                    };

                    destination.CurrentSourceInfo = tempSourceListItem;
                    ;
                    destination.CurrentSourceInfoKey = "$transient";
                    return;
                }
            }
            catch (Exception ex)
            {
                Debug.LogMessage(ex, "Error getting first tieline: {Exception}", this, ex);
                return;
            }

            // Debug.LogMessage(Serilog.Events.LogEventLevel.Verbose, "Getting source for first TieLine {tieLine}", this, firstTieLine);

            TieLine sourceTieLine;
            try
            {
                sourceTieLine = GetRootTieLine(firstTieLine);

                if (sourceTieLine == null)
                {
                    Debug.LogMessage(
                        Serilog.Events.LogEventLevel.Debug,
                        "No route found to source for inputPort {inputPort}. Clearing current source",
                        this,
                        inputPort
                    );

                    var tempSourceListItem = new SourceListItem
                    {
                        SourceKey = "$transient",
                        Name = "None",
                    };

                    destination.CurrentSourceInfo = tempSourceListItem;
                    destination.CurrentSourceInfoKey = string.Empty;
                    return;
                }
            }
            catch (Exception ex)
            {
                Debug.LogError(this, "Error getting sourceTieLine: {message}", ex.Message);
                Debug.LogDebug(ex, "StackTrace: ");
                return;
            }

            // Debug.LogMessage(Serilog.Events.LogEventLevel.Verbose, "Found root TieLine {tieLine}", this, sourceTieLine);

            // Does not handle combinable scenarios or other scenarios where a display might be part of multiple rooms yet.
            var room = DeviceManager
                .AllDevices.OfType<IEssentialsRoom>()
                .FirstOrDefault(
                    (r) =>
                    {
                        if (r is IHasMultipleDisplays roomMultipleDisplays)
                        {
                            return roomMultipleDisplays.Displays.Any(d =>
                                d.Value.Key == destination.Key
                            );
                        }

                        if (r is IHasDefaultDisplay roomDefaultDisplay)
                        {
                            return roomDefaultDisplay.DefaultDisplay.Key == destination.Key;
                        }

                        if (ConfigReader.ConfigObject.GetDestinationListForKey(r.DestinationListKey)?.FirstOrDefault(d => d.Value.SinkKey == destination.Key) != null)
                        {
                            return true;
                        }

                        return false;
                    }
                );

            if (room == null)
            {
                Debug.LogMessage(
                    Serilog.Events.LogEventLevel.Debug,
                    "No room found for display {destination}",
                    this,
                    destination.Key
                );
                return;
            }

            // Debug.LogMessage(Serilog.Events.LogEventLevel.Verbose, "Found room {room} for destination {destination}", this, room.Key, destination.Key);

            var sourceList = ConfigReader.ConfigObject.GetSourceListForKey(room.SourceListKey);

            if (sourceList == null)
            {
                Debug.LogDebug(this,
                    "No source list found for source list key {key}. Unable to find source for tieLine {sourceTieLine}",
                    room.SourceListKey,
                    sourceTieLine
                );
                return;
            }

            // Debug.LogMessage(Serilog.Events.LogEventLevel.Verbose, "Found sourceList for room {room}", this, room.Key);

            var sourceListItem = sourceList.FirstOrDefault(sli =>
            {
                //// Debug.LogMessage(Serilog.Events.LogEventLevel.Verbose,
                //   "SourceListItem {sourceListItem}:{sourceKey} tieLine sourceport device key {sourcePortDeviceKey}",
                //   this,
                //   sli.Key,
                //   sli.Value.SourceKey,
                //   sourceTieLine.SourcePort.ParentDevice.Key);

                return sli.Value.SourceKey.Equals(
                    sourceTieLine.SourcePort.ParentDevice.Key,
                    StringComparison.InvariantCultureIgnoreCase
                );
            });

            var source = sourceListItem.Value;
            var sourceKey = sourceListItem.Key;

            if (source == null)
            {
                Debug.LogDebug(this,
                    "No source found for device {key}. Creating transient source for {destination}",
                    sourceTieLine.SourcePort.ParentDevice.Key,
                    destination
                );

                var tempSourceListItem = new SourceListItem
                {
                    SourceKey = "$transient",
                    Name = sourceTieLine.SourcePort.Key,
                };

                destination.CurrentSourceInfoKey = "$transient";
                destination.CurrentSourceInfo = tempSourceListItem;
                return;
            }

            //Debug.LogMessage(Serilog.Events.LogEventLevel.Verbose, "Got Source {@source} with key {sourceKey}", this, source, sourceKey);

            destination.CurrentSourceInfoKey = sourceKey;
            destination.CurrentSourceInfo = source;
        }

        /// <summary>
        /// Traces a route back from a given tie line to find the root source tie line.
        /// Leverages the existing Extensions.GetRouteToSource method with loop protection.
        /// </summary>
        /// <param name="tieLine">The starting tie line (typically connected to a sink or midpoint).</param>
        /// <returns>The <see cref="TieLine"/> connected to the original source device, or null if the source cannot be determined.</returns>
        private TieLine GetRootTieLine(TieLine tieLine)
        {
            try
            {
                if (!(tieLine.DestinationPort.ParentDevice is IRoutingInputs sink))
                {
                    Debug.LogDebug(this,
                        "TieLine destination {device} is not IRoutingInputs",
                        tieLine.DestinationPort.ParentDevice.Key
                    );
                    return null;
                }

                // Get all potential sources (devices that only have outputs, not inputs+outputs)
                var sources = DeviceManager.AllDevices
                            .OfType<IRoutingOutputs>()
                            .Where(s => !(s is IRoutingInputsOutputs));

                // Try each signal type that this TieLine supports
                var signalTypes = new[]
                {
                    eRoutingSignalType.Audio,
                    eRoutingSignalType.Video,
                    eRoutingSignalType.AudioVideo,
                    eRoutingSignalType.SecondaryAudio,
                    eRoutingSignalType.UsbInput,
                    eRoutingSignalType.UsbOutput
                };

                foreach (var signalType in signalTypes)
                {
                    if (!tieLine.Type.HasFlag(signalType))
                        continue;

                    foreach (var source in sources)
                    {
                        // Use the optimized route discovery with loop protection
                        var (route, _) = sink.GetRouteToSource(
                            source,
                            signalType,
                            tieLine.DestinationPort,
                            null
                        );

                        if (route != null && route.Routes != null && route.Routes.Count > 0)
                        {
                            // Routes[0] is the hop nearest the source: its InputPort is the
                            // port on the first switching device that receives the signal from
                            // the source side. The TieLine whose DestinationPort matches that
                            // port is the exact tie that was traversed, giving us the precise
                            // source output port via SourcePort — regardless of how many output
                            // ports the source device has.
                            var firstHop = route.Routes[0];
                            var sourceTieLine = TieLineCollection.Default.FirstOrDefault(tl =>
                                tl.DestinationPort.Key == firstHop.InputPort.Key &&
                                tl.DestinationPort.ParentDevice.Key == firstHop.InputPort.ParentDevice.Key);

                            if (sourceTieLine != null)
                            {
                                Debug.LogDebug(this,
                                "Found route from {source} to {sink} with {count} hops",
                                    source.Key,
                                    sink.Key,
                                    route.Routes.Count
                                );
                                return sourceTieLine;
                            }
                        }
                    }
                }

                Debug.LogDebug(this, "No route found to any source from {sink}", sink.Key);
                return null;
            }
            catch (Exception ex)
            {
                Debug.LogError(this, "Error getting root tieLine: {message}", ex.Message);
                Debug.LogDebug(ex, "StackTrace: ");

                return null;
            }
        }
    }
}
