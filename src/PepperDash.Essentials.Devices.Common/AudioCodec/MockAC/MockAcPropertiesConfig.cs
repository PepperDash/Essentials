

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Crestron.SimplSharp;

using Newtonsoft.Json;

namespace PepperDash.Essentials.Devices.Common.AudioCodec
{
    /// <summary>
    /// Represents a MockAcPropertiesConfig
    /// </summary>
    public class MockAcPropertiesConfig
    {
        [JsonProperty("phoneNumber")]
        /// <summary>
        /// Gets or sets the PhoneNumber
        /// </summary>
        public string PhoneNumber { get; set; }
    }
}