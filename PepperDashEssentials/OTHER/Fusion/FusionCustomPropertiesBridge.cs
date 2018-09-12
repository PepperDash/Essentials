using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Crestron.SimplSharp;

using PepperDash.Core;
using PepperDash.Essentials.Core;
using PepperDash.Essentials.Room.Behaviours;

namespace PepperDash.Essentials.Fusion
{
    /// <summary>
    /// Handles mapping Fusion Custom Property values to system properties
    /// </summary>
    public class FusionCustomPropertiesBridge
    {

        /// <summary>
        /// Evaluates the room info and custom properties from Fusion and updates the system properties aa needed
        /// </summary>
        /// <param name="roomInfo"></param>
        public void EvaluateRoomInfo(RoomInformation roomInfo)
        {
            var runtimeConfigurableDevices = DeviceManager.AllDevices.Where(d => d is IRuntimeConfigurableDevice);

            try
            {
                foreach (var device in runtimeConfigurableDevices)
                {
                    var deviceConfig = (device as IRuntimeConfigurableDevice).GetDeviceConfig();

                    if (device is RoomOnToDefaultSourceWhenOccupied)
                    {
                        var devConfig = (deviceConfig as RoomOnToDefaultSourceWhenOccupiedConfig);

                        var enableFeature = roomInfo.FusionCustomProperties.FirstOrDefault(p => p.ID.Equals("EnRoomOnWhenOccupied"));
                        if (enableFeature != null)
                            devConfig.EnableRoomOnWhenOccupied = bool.Parse(enableFeature.CustomFieldValue);

                        var enableTime = roomInfo.FusionCustomProperties.FirstOrDefault(p => p.ID.Equals("RoomOnWhenOccupiedStartTime"));
                        if (enableTime != null)
                            devConfig.OccupancyStartTime = enableTime.CustomFieldValue;

                        var disableTime = roomInfo.FusionCustomProperties.FirstOrDefault(p => p.ID.Equals("RoomOnWhenOccupiedEndTime"));
                        if (disableTime != null)
                            devConfig.OccupancyEndTime = disableTime.CustomFieldValue;

                        var enableSunday = roomInfo.FusionCustomProperties.FirstOrDefault(p => p.ID.Equals("EnRoomOnWhenOccupiedSun"));
                        if (enableSunday != null)
                            devConfig.EnableSunday = bool.Parse(enableSunday.CustomFieldValue);

                        var enableMonday = roomInfo.FusionCustomProperties.FirstOrDefault(p => p.ID.Equals("EnRoomOnWhenOccupiedMon"));
                        if (enableMonday != null)
                            devConfig.EnableMonday = bool.Parse(enableMonday.CustomFieldValue);

                        var enableTuesday = roomInfo.FusionCustomProperties.FirstOrDefault(p => p.ID.Equals("EnRoomOnWhenOccupiedTue"));
                        if (enableTuesday != null)
                            devConfig.EnableTuesday = bool.Parse(enableTuesday.CustomFieldValue);

                        var enableWednesday = roomInfo.FusionCustomProperties.FirstOrDefault(p => p.ID.Equals("EnRoomOnWhenOccupiedWed"));
                        if (enableWednesday != null)
                            devConfig.EnableWednesday = bool.Parse(enableWednesday.CustomFieldValue);

                        var enableThursday = roomInfo.FusionCustomProperties.FirstOrDefault(p => p.ID.Equals("EnRoomOnWhenOccupiedThu"));
                        if (enableThursday != null)
                            devConfig.EnableThursday = bool.Parse(enableThursday.CustomFieldValue);

                        var enableFriday = roomInfo.FusionCustomProperties.FirstOrDefault(p => p.ID.Equals("EnRoomOnWhenOccupiedFri"));
                        if (enableFriday != null)
                            devConfig.EnableFriday = bool.Parse(enableFriday.CustomFieldValue);

                        var enableSaturday = roomInfo.FusionCustomProperties.FirstOrDefault(p => p.ID.Equals("EnRoomOnWhenOccupiedSat"));
                        if (enableSaturday != null)
                            devConfig.EnableSaturday = bool.Parse(enableSaturday.CustomFieldValue);

                        deviceConfig = devConfig;
                    }

                    (device as IRuntimeConfigurableDevice).SetDeviceConfig(deviceConfig);
                }
            }
            catch (Exception e)
            {
                Debug.Console(1, "FusionCustomPropetiesBridge: Error mapping properties: {0}", e);
            }
        }
    }
}