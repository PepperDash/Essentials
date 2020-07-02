using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Crestron.SimplSharp;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

using PepperDash.Core;
using PepperDash.Essentials.Core;
using PepperDash.Essentials.Core.Config;
using PepperDash.Essentials.Core.Devices;
using PepperDash.Essentials.Core.Rooms;

namespace PepperDash.Essentials.Core.Fusion
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
        public void EvaluateRoomInfo(string roomKey, RoomInformation roomInfo)
        {
            try
            {
                var reconfigurableDevices = DeviceManager.AllDevices.Where(d => d is ReconfigurableDevice);

                foreach (var device in reconfigurableDevices)
                {
                    // Get the current device config so new values can be overwritten over existing
                    var deviceConfig = (device as ReconfigurableDevice).Config;

                    if (device is RoomOnToDefaultSourceWhenOccupied)
                    {
                        Debug.Console(1, "Mapping Room on via Occupancy values from Fusion");

                        var devProps = JsonConvert.DeserializeObject<RoomOnToDefaultSourceWhenOccupiedConfig>(deviceConfig.Properties.ToString());

                        var enableFeature = roomInfo.FusionCustomProperties.FirstOrDefault(p => p.Id.Equals("EnRoomOnWhenOccupied"));
                        if (enableFeature != null)
                            devProps.EnableRoomOnWhenOccupied = bool.Parse(enableFeature.CustomFieldValue);

                        var enableTime = roomInfo.FusionCustomProperties.FirstOrDefault(p => p.Id.Equals("RoomOnWhenOccupiedStartTime"));
                        if (enableTime != null)
                            devProps.OccupancyStartTime = enableTime.CustomFieldValue;

                        var disableTime = roomInfo.FusionCustomProperties.FirstOrDefault(p => p.Id.Equals("RoomOnWhenOccupiedEndTime"));
                        if (disableTime != null)
                            devProps.OccupancyEndTime = disableTime.CustomFieldValue;

                        var enableSunday = roomInfo.FusionCustomProperties.FirstOrDefault(p => p.Id.Equals("EnRoomOnWhenOccupiedSun"));
                        if (enableSunday != null)
                            devProps.EnableSunday = bool.Parse(enableSunday.CustomFieldValue);

                        var enableMonday = roomInfo.FusionCustomProperties.FirstOrDefault(p => p.Id.Equals("EnRoomOnWhenOccupiedMon"));
                        if (enableMonday != null)
                            devProps.EnableMonday = bool.Parse(enableMonday.CustomFieldValue);

                        var enableTuesday = roomInfo.FusionCustomProperties.FirstOrDefault(p => p.Id.Equals("EnRoomOnWhenOccupiedTue"));
                        if (enableTuesday != null)
                            devProps.EnableTuesday = bool.Parse(enableTuesday.CustomFieldValue);

                        var enableWednesday = roomInfo.FusionCustomProperties.FirstOrDefault(p => p.Id.Equals("EnRoomOnWhenOccupiedWed"));
                        if (enableWednesday != null)
                            devProps.EnableWednesday = bool.Parse(enableWednesday.CustomFieldValue);

                        var enableThursday = roomInfo.FusionCustomProperties.FirstOrDefault(p => p.Id.Equals("EnRoomOnWhenOccupiedThu"));
                        if (enableThursday != null)
                            devProps.EnableThursday = bool.Parse(enableThursday.CustomFieldValue);

                        var enableFriday = roomInfo.FusionCustomProperties.FirstOrDefault(p => p.Id.Equals("EnRoomOnWhenOccupiedFri"));
                        if (enableFriday != null)
                            devProps.EnableFriday = bool.Parse(enableFriday.CustomFieldValue);

                        var enableSaturday = roomInfo.FusionCustomProperties.FirstOrDefault(p => p.Id.Equals("EnRoomOnWhenOccupiedSat"));
                        if (enableSaturday != null)
                            devProps.EnableSaturday = bool.Parse(enableSaturday.CustomFieldValue);

                        deviceConfig.Properties = JToken.FromObject(devProps);
                    }
                    else if (device is EssentialsRoomBase)
                    {
                        // Set the room name
                        if (!string.IsNullOrEmpty(roomInfo.Name))
                        {
                            Debug.Console(1, "Current Room Name: {0}. New Room Name: {1}", deviceConfig.Name, roomInfo.Name);
                            // Set the name in config
                            deviceConfig.Name = roomInfo.Name;

                            Debug.Console(1, "Room Name Successfully Changed.");
                        }

                        // Set the help message
                        var helpMessage = roomInfo.FusionCustomProperties.FirstOrDefault(p => p.Id.Equals("RoomHelpMessage"));
                        if (helpMessage != null)
                        {
                            //Debug.Console(1, "Current Help Message: {0}. New Help Message: {1}", deviceConfig.Properties["help"]["message"].Value<string>(ToString()), helpMessage.CustomFieldValue);
                            deviceConfig.Properties["helpMessage"] = (string)helpMessage.CustomFieldValue;
                        }
                    }

                    // Set the config on the device
                    (device as ReconfigurableDevice).SetConfig(deviceConfig);
                }


            }
            catch (Exception e)
            {
                Debug.Console(1, "FusionCustomPropetiesBridge: Error mapping properties: {0}", e);
            }
        }
    }
}