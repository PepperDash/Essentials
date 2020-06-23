using System;
using System.Collections.Generic;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
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