using Org.BouncyCastle.Crypto.Prng;
using PepperDash.Core;
using PepperDash.Essentials.Core.Config;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace PepperDash.Essentials.Core.Routing
{
    public class RoutingFeedbackManager:EssentialsDevice
    {
        public RoutingFeedbackManager(string key, string name): base(key, name)
        {
            AddPostActivationAction(SubscribeForMidpointFeedback);
            AddPostActivationAction(SubscribeForSinkFeedback);
        }

        private void SubscribeForMidpointFeedback()
        {
            var midpointDevices = DeviceManager.AllDevices.OfType<IRoutingWithFeedback>();

            foreach (var device in midpointDevices)
            {
                device.RouteChanged += HandleMidpointUpdate;
            }
        }

        private void SubscribeForSinkFeedback()
        {
                var sinkDevices = DeviceManager.AllDevices.OfType<IRoutingSinkWithSwitching>();

                foreach (var device in sinkDevices)
                {
                    device.InputChanged += HandleSinkUpdate;
                }   
        }

        private void HandleMidpointUpdate(IRoutingWithFeedback midpoint, RouteSwitchDescriptor newRoute)
        {
            var devices = DeviceManager.AllDevices.OfType<IRoutingSinkWithSwitching>();

            foreach(var device in devices)
            {
                UpdateDestination(device, device.CurrentInputPort);
            }
        }

        private void HandleSinkUpdate(IRoutingSinkWithSwitching sender, RoutingInputPort currentInputPort)
        {
            UpdateDestination(sender, currentInputPort);
        }

        private void UpdateDestination(IRoutingSinkWithSwitching destination, RoutingInputPort inputPort)
        {
            var tieLines = TieLineCollection.Default;

            var firstTieLine = tieLines.FirstOrDefault(tl => tl.DestinationPort.Key == inputPort.Key);

            if (firstTieLine == null)
            {
                Debug.LogMessage(Serilog.Events.LogEventLevel.Information, "No tieline found for inputPort {inputPort}. Clearing current source", this, inputPort);

                destination.CurrentSourceInfo = null;
                destination.CurrentSourceInfoKey = string.Empty;
                return;
            }

            var sourceTieLine = GetRootTieLine(firstTieLine);

            if (sourceTieLine == null)
            {
                Debug.LogMessage(Serilog.Events.LogEventLevel.Information, "No route found to source for inputPort {inputPort}. Clearing current source", this, inputPort);

                destination.CurrentSourceInfo = null;
                destination.CurrentSourceInfoKey = string.Empty;
            }


            // Does not handle combinable scenarios or other scenarios where a display might be part of multiple rooms yet.
            var room = DeviceManager.AllDevices.OfType<IEssentialsRoom>().FirstOrDefault((r) => {
                if(r is IHasMultipleDisplays roomMultipleDisplays)
                {
                    return roomMultipleDisplays.Displays.Any(d => d.Value.Key == destination.Key);
                }

                if(r is IHasDefaultDisplay roomDefaultDisplay)
                {
                    return roomDefaultDisplay.DefaultDisplay.Key == destination.Key;
                }

                return false;
            }) ;
            
            if(room == null)
            {
                Debug.LogMessage(Serilog.Events.LogEventLevel.Warning, "No room found for display {destination}", this, destination.Key);
                return;
            }

            var sourceList = ConfigReader.ConfigObject.GetSourceListForKey(room.SourceListKey);

            if (sourceList == null)
            {
                Debug.LogMessage(Serilog.Events.LogEventLevel.Warning, "No source list found for source list key {key}. Unable to find source for tieLine {sourceTieLine}", this, room.SourceListKey, sourceTieLine);
                return;
            }

            var sourceListItem = sourceList.FirstOrDefault(sli => sli.Value.SourceKey == sourceTieLine.SourcePort.ParentDevice.Key);

            var source = sourceListItem.Value;

            if (source == null)
            {
                Debug.LogMessage(Serilog.Events.LogEventLevel.Information, "No source found for device {key}. Clearing current source on {destination}", this, sourceTieLine.SourcePort.ParentDevice.Key, destination);

                destination.CurrentSourceInfo = null;
                destination.CurrentSourceInfoKey = string.Empty;
                return;
            }

            destination.CurrentSourceInfo = source;
            destination.CurrentSourceInfoKey = source.SourceKey;
        }

        private TieLine GetRootTieLine(TieLine tieLine)
        {
            TieLine nextTieLine = null;

            if(tieLine.SourcePort.ParentDevice is IRoutingWithFeedback midpoint)
            {
                var currentRoute = midpoint.CurrentRoutes.FirstOrDefault(route => route.OutputPort.Key == tieLine.SourcePort.Key);

                if(currentRoute == null)
                {
                    Debug.LogMessage(Serilog.Events.LogEventLevel.Information, "No route through midpoint {midpoint} for outputPort {outputPort}", this, midpoint.Key, tieLine.SourcePort);
                    return null;
                }

                nextTieLine = TieLineCollection.Default.FirstOrDefault(tl => tl.DestinationPort.Key == currentRoute.InputPort.Key);

                if(tieLine != null)
                {
                    return GetRootTieLine(nextTieLine);
                }

                return tieLine;
            }

            if(tieLine.SourcePort.ParentDevice is IRoutingSource) //end of the chain
            {
                return tieLine;
            }

            nextTieLine = TieLineCollection.Default.FirstOrDefault(tl => tl.SourcePort.Key == tieLine.SourcePort.Key);

            if(nextTieLine != null)
            {
                return GetRootTieLine(nextTieLine);
            }

            return nextTieLine;
        }
    }
}
