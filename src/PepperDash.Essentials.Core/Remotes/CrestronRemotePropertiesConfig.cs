extern alias Full;

using System;
using System.Collections.Generic;
using Full.Newtonsoft.Json;
using Full.Newtonsoft.Json.Converters;
using PepperDash.Core;

namespace PepperDash.Essentials.Core
{
    public class CrestronRemotePropertiesConfig
    {
        [JsonProperty("control")]
        public EssentialsControlPropertiesConfig Control { get; set; }

        [JsonProperty("gatewayDeviceKey")]
        public string GatewayDeviceKey { get; set; }
    }
}