

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Crestron.SimplSharp;
using Newtonsoft.Json;

namespace PepperDash.Essentials.Core
{
    /// <summary>
    /// Provide a way to easily deserialize into a secret object from config
    /// </summary>
    public class SecretsPropertiesConfig
    {
        [JsonProperty("provider")]
        public string Provider { get; set; }
        [JsonProperty("key")]
        public string Key { get; set; }
    }
}