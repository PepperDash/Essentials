using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Crestron.SimplSharp;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using PepperDash.Core;
using PepperDash.Essentials.Core.Config;


namespace PepperDash.Essentials.Core
{
    public class EssentialsRfGatewayConfig
    {
        [JsonProperty("control")]
        public EssentialsControlPropertiesConfig Control { get; set; }

        [JsonProperty("gatewayType")]
        public string GatewayType { get; set; }

     }
}