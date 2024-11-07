﻿using PepperDash.Core;
using PepperDash.Essentials.Core.Devices;
using Serilog.Events;

//using PepperDash.Essentials.Devices.Common.Cameras;

namespace PepperDash.Essentials.Core.Bridges
{
    /// <summary>
    /// Helper methods for bridges
    /// </summary>
    public static class BridgeHelper
    {
        public static void PrintJoinMap(string command)
        {
            var targets = command.Split(' ');

            var bridgeKey = targets[0].Trim();

            if (!(DeviceManager.GetDeviceForKey(bridgeKey) is EiscApiAdvanced bridge))
            {
                Debug.LogMessage(LogEventLevel.Information, "Unable to find advanced bridge with key: '{0}'", bridgeKey);
                return;
            }

            if (targets.Length > 1)
            {
                var deviceKey = targets[1].Trim();

                if (string.IsNullOrEmpty(deviceKey)) return;
                bridge.PrintJoinMapForDevice(deviceKey);
            }
            else
            {
                bridge.PrintJoinMaps();
            }
        }
        public static void JoinmapMarkdown(string command)
        {
            var targets = command.Split(' ');

            var bridgeKey = targets[0].Trim();

            var bridge = DeviceManager.GetDeviceForKey(bridgeKey) as EiscApiAdvanced;

            if (bridge == null)
            {
                Debug.LogMessage(LogEventLevel.Information, "Unable to find advanced bridge with key: '{0}'", bridgeKey);
                return;
            }

            if (targets.Length > 1)
            {
                var deviceKey = targets[1].Trim();

                if (string.IsNullOrEmpty(deviceKey)) return;
                bridge.MarkdownJoinMapForDevice(deviceKey, bridgeKey);
            }
            else
            {
                bridge.MarkdownForBridge(bridgeKey);

            }
        }
    }

}