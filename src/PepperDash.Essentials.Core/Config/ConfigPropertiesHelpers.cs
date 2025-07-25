

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Crestron.SimplSharp;
using PepperDash.Essentials.Core;
using PepperDash.Core;
using Newtonsoft.Json.Linq;

namespace PepperDash.Essentials.Core.Config
{
    /// <summary>
    /// Represents a ConfigPropertiesHelpers
    /// </summary>
    public class ConfigPropertiesHelpers
    {
        /// <summary>
        /// GetHasAudio method
        /// </summary>
        public static bool GetHasAudio(DeviceConfig deviceConfig)
        {
            return deviceConfig.Properties.Value<bool>("hasAudio");
        }

        /// <summary>
        /// Returns the value of properties.hasControls, or false if not defined
        /// </summary>
        public static bool GetHasControls(DeviceConfig deviceConfig)
        {
            return deviceConfig.Properties.Value<bool>("hasControls");
        }
    }
}