

using System;
using System.Collections.Generic;
using Crestron.SimplSharp;
using Crestron.SimplSharp.CrestronIO;
using Crestron.SimplSharpPro;

using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
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

    /// <summary>
    /// 
    /// </summary>
    public class DevicePropertiesConverter : JsonConverter
    {

        public override bool CanConvert(Type objectType)
        {
            return objectType == typeof(JToken);
        }

        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
        {
            return JToken.ReadFrom(reader);
        }

        public override bool CanWrite
        {
            get
            {
                return false;
            }
        }

        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            throw new NotImplementedException("SOD OFF HOSER");
        }
    }
}