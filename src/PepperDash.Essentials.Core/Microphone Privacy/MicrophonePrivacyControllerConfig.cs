using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Crestron.SimplSharp;

using PepperDash.Essentials.Core.CrestronIO;

namespace PepperDash.Essentials.Core.Privacy
{
    /// <summary>
    /// Represents a MicrophonePrivacyControllerConfig
    /// </summary>
    public class MicrophonePrivacyControllerConfig
    {
        /// <summary>
        /// Gets or sets the Inputs
        /// </summary>
        public List<KeyedDevice> Inputs { get; set; }
        /// <summary>
        /// Gets or sets the GreenLedRelay
        /// </summary>
        public KeyedDevice GreenLedRelay { get; set; }
        /// <summary>
        /// Gets or sets the RedLedRelay
        /// </summary>
        public KeyedDevice RedLedRelay { get; set; }
    }

    /// <summary>
    /// Represents a KeyedDevice
    /// </summary>
    public class KeyedDevice
    {
        /// <summary>
        /// Gets or sets the DeviceKey
        /// </summary>
        public string DeviceKey { get; set; }
    }
}