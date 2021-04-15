using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Crestron.SimplSharp;
using Newtonsoft.Json;

namespace PepperDash.Essentials.Core
{
    public class SecretsPropertiesConfig
    {
        [JsonProperty("provider")]
        public string Provider { get; set; }
        [JsonProperty("key")]
        public string Key { get; set; }
    }
}