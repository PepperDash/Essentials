using System;
using System.Collections.Concurrent;
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
        /// Maps midpoint device keys to the set of downstream sink input ports, derived from the
        /// static tie-line topology. Because it is built from topology rather than a sink's currently
        /// reported input, sinks that never report an input (i.e. <see cref="IRoutingSinkWithFeedback.CurrentInputPort"/>
        /// stays null, such as codecs fed from an external matrix) are still mapped and updated.
        /// </summary>
        private Dictionary<string, HashSet<RoutingInputPort>> midpointToSinkInputsMap;

        /// <summary>
        /// Debounce timers for each sink device to prevent rapid successive updates. Must be a
        /// ConcurrentDictionary - it's mutated both from whatever thread calls UpdateDestination and
        /// from each Timer's Elapsed callback (which fires on a threadpool timer thread), so a plain
        /// Dictionary here would have its internal state corrupted by the concurrent Insert/Remove
        /// calls, surfacing as an IndexOutOfRangeException from deep inside Dictionary internals.
        /// </summary>
        private readonly ConcurrentDictionary<string, Timer> updateTimers = new ConcurrentDictionary<string, Timer>();

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
            // The midpoint-to-sink map is NOT built here: devices activate before tie lines are
            // loaded (ControlSystem.Load), so at pre-activation TieLineCollection is still empty and
            // the map would be empty too - silently dropping every midpoint RouteChanged.
            // ControlSystem.LoadTieLines calls BuildMidpointSinkMap once tie lines exist.
            AddPreActivationAction(SubscribeForMidpointFeedback);
            AddPreActivationAction(SubscribeForSinkFeedback);
        }

        /// <summary>
        /// Builds a map of which sink input ports are downstream of each midpoint device
        /// for performance optimization in HandleMidpointUpdate.
        /// The map is derived from the static tie-line topology (every sink input port is traced
        /// upstream), so it does not depend on a sink having already reported its current input.
        /// Must be called after tie lines are loaded, and again if they change.
        /// </summary>
        public void BuildMidpointSinkMap()
        {
            midpointToSinkInputsMap = new Dictionary<string, HashSet<RoutingInputPort>>();

            var sinks = DeviceManager.AllDevices.OfType<IRoutingSinkWithFeedback>();

            foreach (var sink in sinks)
            {
                // Trace from every input port on the sink (static topology) rather than only the
                // currently-selected input. Sinks that never report an input still get mapped.
                var inputPorts = (sink as IRoutingInputs)?.InputPorts;
                if (inputPorts == null)
                    continue;

                foreach (var inputPort in inputPorts)
                {
                    var upstreamMidpoints = GetUpstreamMidpointsForInput(inputPort);

                    foreach (var midpointKey in upstreamMidpoints)
                    {
                        if (!midpointToSinkInputsMap.TryGetValue(midpointKey, out var inputs))
                        {
                            inputs = new HashSet<RoutingInputPort>();
                            midpointToSinkInputsMap[midpointKey] = inputs;
                        }

                        inputs.Add(inputPort);
                    }
                }
            }

            Debug.LogMessage(
                Serilog.Events.LogEventLevel.Information,
                "Built midpoint-to-sink map with {count} midpoints",
                this,
                midpointToSinkInputsMap.Count
            );
        }

        /// <summary>
        /// Gets all upstream midpoint device keys reachable from a specific sink input port
        /// by walking the static tie-line topology.
        /// </summary>
        private HashSet<string> GetUpstreamMidpointsForInput(RoutingInputPort inputPort)
        {
            var result = new HashSet<string>();
            var visited = new HashSet<string>();

            if (inputPort == null)
                return result;

            var tieLine = TieLineCollection.Default.FirstOrDefault(tl =>
                tl.DestinationPort.Key == inputPort.Key &&
                tl.DestinationPort.ParentDevice.Key == inputPort.ParentDevice.Key);

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

            if (tieLine.SourcePort.ParentDevice is IRoutingMidpointWithFeedback midpoint)
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
        /// Subscribes to the RouteChanged event on all devices implementing <see cref="IRoutingMidpointWithFeedback"/>.
        /// </summary>
        private void SubscribeForMidpointFeedback()
        {
            var midpointDevices = DeviceManager.AllDevices.OfType<IRoutingMidpointWithFeedback>();

            foreach (var device in midpointDevices)
            {
                device.RouteChanged += HandleMidpointUpdate;
            }
        }

        /// <summary>
        /// Subscribes to the InputChanged event on all devices implementing <see cref="IRoutingSinkWithFeedback"/>.
        /// </summary>
        private void SubscribeForSinkFeedback()
        {
            var sinkDevices =
                DeviceManager.AllDevices.OfType<IRoutingSinkWithFeedback>();

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
            IRoutingMidpointWithFeedback midpoint,
            RouteSwitchDescriptor newRoute
        )
        {
            try
            {
                // Only update affected sinks (performance optimization)
                if (midpointToSinkInputsMap != null && midpointToSinkInputsMap.TryGetValue(midpoint.Key, out var affectedInputPorts))
                {
                    Debug.LogMessage(
                        Serilog.Events.LogEventLevel.Debug,
                        "Midpoint {midpoint} changed, updating {count} downstream sink inputs",
                        this,
                        midpoint.Key,
                        affectedInputPorts.Count
                    );

                    // Avoid redundant updates when a feedback-reporting sink has several mapped inputs.
                    var updatedSinkKeys = new HashSet<string>();

                    foreach (var inputPort in affectedInputPorts)
                    {
                        if (!(inputPort.ParentDevice is IRoutingSinkWithFeedback sink))
                            continue;

                        // Sinks that report their input drive updates off the currently-selected input.
                        // Sinks that never report an input (CurrentInputPort == null, e.g. a codec fed
                        // from an external matrix) fall back to the static topology input port so that
                        // matrix route changes still propagate.
                        var portToUse = sink.CurrentInputPort ?? inputPort;

                        if (sink.CurrentInputPort != null && !updatedSinkKeys.Add(sink.Key))
                            continue;

                        UpdateDestination(sink, portToUse);
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
        /// Handles the InputChanged event from a sink device.
        /// Triggers an update for the specific sink device.
        /// </summary>
        /// <param name="sender">The sink device that reported an input change.</param>
        /// <param name="currentInputPort">The new input port selected on the sink device.</param>
        private void HandleSinkUpdate(
            IRoutingSinkWithFeedback sender,
            RoutingInputPort currentInputPort
        )
        {
            try
            {
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
            IRoutingSinkWithFeedback destination,
            RoutingInputPort inputPort
        )
        {
            if (destination == null)
                return;

            // Keyed by destination AND input port, not just destination: a single sink can have
            // several independently-routed input ports (e.g. a codec with multiple simultaneous
            // USB camera inputs, all downstream of the same midpoint). Keying by destination alone
            // meant updates for one port would cancel/replace the still-pending debounce timer for
            // a sibling port on the same sink, so only the last-processed port's update ever
            // actually ran - the others silently never got refreshed.
            var key = destination.Key + ":" + (inputPort?.Key ?? string.Empty);

            // Cancel existing timer for this specific sink/port combination
            if (updateTimers.TryGetValue(key, out var existingTimer))
            {
                existingTimer.Stop();
                existingTimer.Dispose();
            }

            // Start new debounced timer
            var timer = new Timer(DEBOUNCE_MS) { AutoReset = false };
            timer.Elapsed += (sender, e) =>
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
                    if (updateTimers.TryRemove(key, out var timerToDispose))
                    {
                        timerToDispose?.Dispose();
                    }
                }
            };
            timer.Start();
            updateTimers[key] = timer;
        }

        /// <summary>
        /// Immediately updates the CurrentSourceInfo for a destination device.
        /// Called after debounce delay.
        /// </summary>
        private void UpdateDestinationImmediate(
            IRoutingSinkWithFeedback destination,
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


                    return;
                }
            }
            catch (Exception ex)
            {
                Debug.LogMessage(ex, "Error getting first tieline: {Exception}", this, ex);
                return;
            }

            // Trace each signal type independently through the midpoints' *current* crosspoints.
            // Audio and video can come from different sources (e.g. breakaway on a matrix), and a
            // purely topological search (GetRouteToSource) only proves a source *could* be routed
            // here, not that it currently is - so it can't be used to report feedback.
            try
            {
                while (RouteDescriptorCollection.DefaultCollection.RemoveRouteDescriptor(destination, inputPort.Key) != null) { }

                foreach (var signalType in new[] { eRoutingSignalType.Audio, eRoutingSignalType.Video })
                {
                    if (!firstTieLine.Type.HasFlag(signalType))
                        continue;

                    if (!TryGetActiveSourcePort(firstTieLine, signalType, new HashSet<string>(), out var sourcePort))
                    {
                        Debug.LogMessage(
                            Serilog.Events.LogEventLevel.Debug,
                            "Route to {destination}:{inputPort} ({signalType}) passes through a midpoint without route feedback. Leaving current source unchanged",
                            this,
                            destination.Key,
                            inputPort.Key,
                            signalType
                        );
                        continue;
                    }

                    var source = sourcePort?.ParentDevice as IRoutingOutputs;

                    Debug.LogMessage(
                        Serilog.Events.LogEventLevel.Debug,
                        "Setting {destination} current {signalType} source to {source}",
                        this,
                        destination.Key,
                        signalType,
                        source?.Key ?? "none"
                    );

                    destination.SetCurrentSource(signalType, source as IRoutingSource);

                    if (source == null)
                        continue;

                    var (route, _) = destination.GetRouteToSource(source, signalType, inputPort, sourcePort);

                    RouteDescriptorCollection.DefaultCollection.AddRouteDescriptor(route);
                }
            }
            catch (Exception ex)
            {
                Debug.LogMessage(ex, "Error updating current sources for {destination}: {Exception}", this, destination.Key, ex);
            }
        }

        /// <summary>
        /// Walks upstream from a tie line, following each midpoint's reported <see cref="IRoutingMidpointWithFeedback.CurrentRoutes"/>
        /// for the given signal type, to find the source output port currently feeding it.
        /// </summary>
        /// <param name="tieLine">The tie line to start from (typically connected to a sink).</param>
        /// <param name="signalType">A single signal type flag (Audio or Video).</param>
        /// <param name="visited">Midpoint keys already walked, for loop protection.</param>
        /// <param name="sourcePort">The source's output port, or null if nothing is currently routed.</param>
        /// <returns>False if the path passes through a midpoint that can't report its routes, so the source is unknown.</returns>
        private bool TryGetActiveSourcePort(
            TieLine tieLine,
            eRoutingSignalType signalType,
            HashSet<string> visited,
            out RoutingOutputPort sourcePort
        )
        {
            sourcePort = null;

            var upstreamDevice = tieLine.SourcePort.ParentDevice;

            if (!(upstreamDevice is IRoutingMidpoint))
            {
                sourcePort = tieLine.SourcePort;
                return true;
            }

            if (!(upstreamDevice is IRoutingMidpointWithFeedback midpoint))
                return false;

            if (!visited.Add(midpoint.Key))
                return true;

            var currentRoute = midpoint.CurrentRoutes?.ToList().FirstOrDefault(r =>
                r.OutputPort != null
                && r.InputPort != null
                && r.OutputPort.Key == tieLine.SourcePort.Key
                && (r.SignalType ?? r.InputPort.Type).HasFlag(signalType));

            if (currentRoute == null)
                return true;

            var upstreamTieLine = TieLineCollection.Default.FirstOrDefault(tl =>
                tl.DestinationPort.Key == currentRoute.InputPort.Key
                && tl.DestinationPort.ParentDevice.Key == midpoint.Key
                && tl.Type.HasFlag(signalType));

            if (upstreamTieLine == null)
                return true;

            return TryGetActiveSourcePort(upstreamTieLine, signalType, visited, out sourcePort);
        }
    }
}
