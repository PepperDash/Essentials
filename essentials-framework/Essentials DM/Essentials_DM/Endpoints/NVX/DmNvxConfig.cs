using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Crestron.SimplSharp;
using Crestron.SimplSharpPro.DM.Streaming;

using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using PepperDash.Core;

namespace PepperDash.Essentials.DM.Endpoints.NVX
{
    /// <summary>
    /// Represents the "properties" property of a DM NVX device config
    /// </summary>
    public class DmNvxConfig
    {
        [JsonProperty("control")]
        public ControlPropertiesConfig Control { get; set; }

        [JsonProperty("parrentDeviceKey")]
        public string ParentDeviceKey { get; set; }

        [JsonProperty("deviceMode")]
        public eDeviceMode DeviceMode { get; set; }


    }
}