

using System;
using System.Linq;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using PepperDash.Core;
using PepperDash.Essentials.Core.Devices;
using Serilog.Events;

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
        /// <param name="room">The room associated with this Fusion instance</param>
        /// <param name="roomInfo">The room information from Fusion</param>
        /// <param name="useFusionRoomName"></param>
        public void EvaluateRoomInfo(IEssentialsRoom room, RoomInformation roomInfo, bool useFusionRoomName)
        {
            try
            {
                var reconfigurableDevices = DeviceManager.AllDevices.OfType<ReconfigurableDevice>();

                foreach (var device in reconfigurableDevices)
                {
                    // Get the current device config so new values can be overwritten over existing
                    var deviceConfig = device.Config;

                    if (device is IEssentialsRoom)
                    {
                        // Skipping room name as this will affect ALL room instances in the configuration and cause unintended consequences when multiple rooms are present and multiple Fusion instances are used
                        continue;
                    }

                    if (device is RoomOnToDefaultSourceWhenOccupied)
                    {
                        Debug.LogMessage(LogEventLevel.Debug, "Mapping Room on via Occupancy values from Fusion");

                        var devProps = JsonConvert.DeserializeObject<RoomOnToDefaultSourceWhenOccupiedConfig>(deviceConfig.Properties.ToString());

                        var enableFeature = roomInfo.FusionCustomProperties.FirstOrDefault(p => p.ID.Equals("EnRoomOnWhenOccupied"));
                        if (enableFeature != null)
                            devProps.EnableRoomOnWhenOccupied = bool.Parse(enableFeature.CustomFieldValue);

                        var enableTime = roomInfo.FusionCustomProperties.FirstOrDefault(p => p.ID.Equals("RoomOnWhenOccupiedStartTime"));
                        if (enableTime != null)
                            devProps.OccupancyStartTime = enableTime.CustomFieldValue;

                        var disableTime = roomInfo.FusionCustomProperties.FirstOrDefault(p => p.ID.Equals("RoomOnWhenOccupiedEndTime"));
                        if (disableTime != null)
                            devProps.OccupancyEndTime = disableTime.CustomFieldValue;

                        var enableSunday = roomInfo.FusionCustomProperties.FirstOrDefault(p => p.ID.Equals("EnRoomOnWhenOccupiedSun"));
                        if (enableSunday != null)
                            devProps.EnableSunday = bool.Parse(enableSunday.CustomFieldValue);

                        var enableMonday = roomInfo.FusionCustomProperties.FirstOrDefault(p => p.ID.Equals("EnRoomOnWhenOccupiedMon"));
                        if (enableMonday != null)
                            devProps.EnableMonday = bool.Parse(enableMonday.CustomFieldValue);

                        var enableTuesday = roomInfo.FusionCustomProperties.FirstOrDefault(p => p.ID.Equals("EnRoomOnWhenOccupiedTue"));
                        if (enableTuesday != null)
                            devProps.EnableTuesday = bool.Parse(enableTuesday.CustomFieldValue);

                        var enableWednesday = roomInfo.FusionCustomProperties.FirstOrDefault(p => p.ID.Equals("EnRoomOnWhenOccupiedWed"));
                        if (enableWednesday != null)
                            devProps.EnableWednesday = bool.Parse(enableWednesday.CustomFieldValue);

                        var enableThursday = roomInfo.FusionCustomProperties.FirstOrDefault(p => p.ID.Equals("EnRoomOnWhenOccupiedThu"));
                        if (enableThursday != null)
                            devProps.EnableThursday = bool.Parse(enableThursday.CustomFieldValue);

                        var enableFriday = roomInfo.FusionCustomProperties.FirstOrDefault(p => p.ID.Equals("EnRoomOnWhenOccupiedFri"));
                        if (enableFriday != null)
                            devProps.EnableFriday = bool.Parse(enableFriday.CustomFieldValue);

                        var enableSaturday = roomInfo.FusionCustomProperties.FirstOrDefault(p => p.ID.Equals("EnRoomOnWhenOccupiedSat"));
                        if (enableSaturday != null)
                            devProps.EnableSaturday = bool.Parse(enableSaturday.CustomFieldValue);

                        deviceConfig.Properties = JToken.FromObject(devProps);
                    }

                    // Set the config on the device
                    device.SetConfig(deviceConfig);
                }

                if (!(room is ReconfigurableDevice reconfigurable))
                {
                    Debug.LogWarning("FusionCustomPropertiesBridge: Room is not a ReconfigurableDevice. Cannot map custom properties.");
                    return;
                }

                var roomConfig = reconfigurable.Config;

                var updateConfig = false;

                // Set the room name
                if (!string.IsNullOrEmpty(roomInfo.Name) && useFusionRoomName)
                {
                    Debug.LogDebug("Current Room Name: {currentName}. New Room Name: {fusionName}", roomConfig.Name, roomInfo.Name);
                    // Set the name in config
                    roomConfig.Name = roomInfo.Name;
                    updateConfig = true;

                    Debug.LogDebug("Room Name Successfully Changed.");
                }

                // Set the help message
                var helpMessage = roomInfo.FusionCustomProperties.FirstOrDefault(p => p.ID.Equals("RoomHelpMessage"));
                if (helpMessage != null)
                {
                    roomConfig.Properties["helpMessage"] = helpMessage.CustomFieldValue;
                    updateConfig = true;
                }

                if (updateConfig)
                {
                    reconfigurable.SetConfig(roomConfig);
                }
            }
            catch (Exception e)
            {
                Debug.LogError("FusionCustomPropetiesBridge: Exception mapping properties for {roomKey}: {message}", room.Key, e.Message);
                Debug.LogDebug(e, "Stack Trace: ");
            }
        }
    }
}