using System;
using System.Linq;
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
        /// Initializes a new instance of the <see cref="RoutingFeedbackManager"/> class.
        /// </summary>
        /// <param name="key">The unique key for this manager device.</param>
        /// <param name="name">The name of this manager device.</param>
        public RoutingFeedbackManager(string key, string name)
            : base(key, name)
        {
            AddPreActivationAction(SubscribeForMidpointFeedback);
            AddPreActivationAction(SubscribeForSinkFeedback);
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
        /// Triggers an update for all sink devices.
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
                var devices =
                    DeviceManager.AllDevices.OfType<IRoutingSinkWithSwitchingWithInputPort>();

                foreach (var device in devices)
                {
                    UpdateDestination(device, device.CurrentInputPort);
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
            IRoutingSinkWithSwitching sender,
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
        /// </summary>
        /// <param name="destination">The destination sink device to update.</param>
        /// <param name="inputPort">The currently selected input port on the destination device.</param>
        private void UpdateDestination(
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
                Debug.LogMessage(ex, "Error getting sourceTieLine: {Exception}", this, ex);
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
                Debug.LogMessage(
                    Serilog.Events.LogEventLevel.Debug,
                    "No source list found for source list key {key}. Unable to find source for tieLine {sourceTieLine}",
                    this,
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
                Debug.LogMessage(
                    Serilog.Events.LogEventLevel.Debug,
                    "No source found for device {key}. Creating transient source for {destination}",
                    this,
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
        /// Recursively traces a route back from a given tie line to find the root source tie line.
        /// It navigates through midpoint devices (<see cref="IRoutingWithFeedback"/>) by checking their current routes.
        /// </summary>
        /// <param name="tieLine">The starting tie line (typically connected to a sink or midpoint).</param>
        /// <returns>The <see cref="TieLine"/> connected to the original source device, or null if the source cannot be determined.</returns>
        private TieLine GetRootTieLine(TieLine tieLine)
        {
            TieLine nextTieLine = null;
            try
            {
                //Debug.LogMessage(Serilog.Events.LogEventLevel.Verbose, "**Following tieLine {tieLine}**", this, tieLine);

                if (tieLine.SourcePort.ParentDevice is IRoutingWithFeedback midpoint)
                {
                    // Debug.LogMessage(Serilog.Events.LogEventLevel.Verbose, "TieLine Source device {sourceDevice} is midpoint", this, midpoint);

                    if (midpoint.CurrentRoutes == null || midpoint.CurrentRoutes.Count == 0)
                    {
                        Debug.LogMessage(
                            Serilog.Events.LogEventLevel.Debug,
                            "Midpoint {midpointKey} has no routes",
                            this,
                            midpoint.Key
                        );
                        return null;
                    }

                    var currentRoute = midpoint.CurrentRoutes.FirstOrDefault(route =>
                    {
                        //Debug.LogMessage(Serilog.Events.LogEventLevel.Verbose, "Checking {route} against {tieLine}", this, route, tieLine);

                        return route.OutputPort != null
                            && route.InputPort != null
                            && route.OutputPort?.Key == tieLine.SourcePort.Key
                            && route.OutputPort?.ParentDevice.Key
                                == tieLine.SourcePort.ParentDevice.Key;
                    });

                    if (currentRoute == null)
                    {
                        Debug.LogMessage(
                            Serilog.Events.LogEventLevel.Debug,
                            "No route through midpoint {midpoint} for outputPort {outputPort}",
                            this,
                            midpoint.Key,
                            tieLine.SourcePort
                        );
                        return null;
                    }

                    //Debug.LogMessage(Serilog.Events.LogEventLevel.Verbose, "Found currentRoute {currentRoute} through {midpoint}", this, currentRoute, midpoint);

                    nextTieLine = TieLineCollection.Default.FirstOrDefault(tl =>
                    {
                        //Debug.LogMessage(Serilog.Events.LogEventLevel.Verbose, "Checking {route} against {tieLine}", tl.DestinationPort.Key, currentRoute.InputPort.Key);
                        return tl.DestinationPort.Key == currentRoute.InputPort.Key
                            && tl.DestinationPort.ParentDevice.Key
                                == currentRoute.InputPort.ParentDevice.Key;
                    });

                    if (nextTieLine != null)
                    {
                        //Debug.LogMessage(Serilog.Events.LogEventLevel.Verbose, "Found next tieLine {tieLine}. Walking the chain", this, nextTieLine);
                        return GetRootTieLine(nextTieLine);
                    }

                    //Debug.LogMessage(Serilog.Events.LogEventLevel.Verbose, "Found root tieLine {tieLine}", this,nextTieLine);
                    return nextTieLine;
                }

                //Debug.LogMessage(Serilog.Events.LogEventLevel.Verbose, "TieLIne Source Device {sourceDeviceKey} is IRoutingSource: {isIRoutingSource}", this, tieLine.SourcePort.ParentDevice.Key, tieLine.SourcePort.ParentDevice is IRoutingSource);
                //Debug.LogMessage(Serilog.Events.LogEventLevel.Verbose, "TieLine Source Device interfaces: {typeFullName}:{interfaces}", this, tieLine.SourcePort.ParentDevice.GetType().FullName, tieLine.SourcePort.ParentDevice.GetType().GetInterfaces().Select(i => i.Name));

                if (
                    tieLine.SourcePort.ParentDevice is IRoutingSource
                    || tieLine.SourcePort.ParentDevice is IRoutingOutputs
                ) //end of the chain
                {
                    // Debug.LogMessage(Serilog.Events.LogEventLevel.Verbose, "Found root: {tieLine}", this, tieLine);
                    return tieLine;
                }

                nextTieLine = TieLineCollection.Default.FirstOrDefault(tl =>
                    tl.DestinationPort.Key == tieLine.SourcePort.Key
                    && tl.DestinationPort.ParentDevice.Key == tieLine.SourcePort.ParentDevice.Key
                );

                if (nextTieLine != null)
                {
                    return GetRootTieLine(nextTieLine);
                }
            }
            catch (Exception ex)
            {
                Debug.LogMessage(ex, "Error walking tieLines: {Exception}", this, ex);
                return null;
            }

            return null;
        }
    }
}
