using System.Collections.Generic;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using PepperDash.Essentials.Core.DeviceTypeInterfaces;

namespace PepperDash.Essentials.Devices.Common.Shades
{
    /// <summary>
    /// Represents a ScreenLiftControllerConfigProperties
    /// </summary>
    public class ScreenLiftControllerConfigProperties
    {
        /// <summary>
        /// Gets or sets the DisplayDeviceKey
        /// </summary>
        [JsonProperty("displayDeviceKey")]
        public string DisplayDeviceKey { get; set; }

        /// <summary>
        /// Gets or sets the Type
        /// </summary>
        [JsonProperty("type")]
        [JsonConverter(typeof(StringEnumConverter))]
        public eScreenLiftControlType Type { get; set; }

        /// <summary>
        /// Gets or sets the Mode
        /// </summary>
        [JsonProperty("mode")]
        [JsonConverter(typeof(StringEnumConverter))]
        public eScreenLiftControlMode Mode { get; set; }

        /// <summary>
        /// Gets or sets the Relays
        /// </summary>
        [JsonProperty("relays")]
        public Dictionary<string, ScreenLiftRelaysConfig> Relays { get; set; }

        /// <summary>
        /// Mutes the display when the screen is in the up position
        /// </summary>
        [JsonProperty("muteOnScreenUp")]
        public bool MuteOnScreenUp { get; set; }

        /// <summary>
        /// When true, this controller does NOT automatically lower when its assigned display powers on
        /// (warms up). Manual Raise/Lower still work, and the power-off auto-raise is unaffected. Intended
        /// for a projector screen that must not auto-drop in a public space for safety, while the projector
        /// lift (a separate controller) can still drop automatically.
        /// </summary>
        [JsonProperty("disableAutoLowerOnPowerOn")]
        public bool DisableAutoLowerOnPowerOn { get; set; }

        /// <summary>
        /// When true, this controller does NOT automatically raise when its assigned display powers off
        /// (cools down). Manual Raise/Lower still work, and the power-on auto-lower is unaffected. The
        /// companion to <see cref="DisableAutoLowerOnPowerOn"/>; together they make a controller fully
        /// manual while leaving other controllers (e.g. the lift) on their default automatic behavior.
        /// </summary>
        [JsonProperty("disableAutoRaiseOnPowerOff")]
        public bool DisableAutoRaiseOnPowerOff { get; set; }
    }
}
