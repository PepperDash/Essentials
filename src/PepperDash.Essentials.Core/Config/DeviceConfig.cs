extern alias Full;
using System.Collections.Generic;
using Crestron.SimplSharp;
using Crestron.SimplSharp.CrestronIO;
using Crestron.SimplSharpPro;

using Full.Newtonsoft.Json;
using Full.Newtonsoft.Json.Linq;
using PepperDash.Core;
using PepperDash.Essentials.Core;

namespace PepperDash.Essentials.Core.Config
{
    public class DeviceConfig
    {
        [JsonProperty("key")]
        public string Key { get; set; }

        [JsonProperty("uid")]
        public int Uid { get; set; }

        [JsonProperty("name")]
        public string Name { get; set; }

        [JsonProperty("group")]
        public string Group { get; set; }

        [JsonProperty("type")]
        public string Type { get; set; }

        [JsonProperty("properties")]
        [JsonConverter(typeof(DevicePropertiesConverter))]
        public JToken Properties { get; set; }

        public DeviceConfig(DeviceConfig dc)
        {
            Key = dc.Key;
            Uid = dc.Uid;
            Name = dc.Name;
            Group = dc.Group;
            Type = dc.Type;

            Properties = JToken.Parse(dc.Properties.ToString());

            //Properties = JToken.FromObject(dc.Properties);
        }

        public DeviceConfig() {}
    }
}