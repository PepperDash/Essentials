

using System;

using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace PepperDash.Essentials.Core.Config
{
    /// <summary>
    /// Represents a DeviceConfig
    /// </summary>
    public class DeviceConfig
    {
        [JsonProperty("key")]
        /// <summary>
        /// Gets or sets the Key
        /// </summary>
        public string Key { get; set; }

        [JsonProperty("uid")]
        /// <summary>
        /// Gets or sets the Uid
        /// </summary>
        public int Uid { get; set; }

        [JsonProperty("name")]
        /// <summary>
        /// Gets or sets the Name
        /// </summary>
        public string Name { get; set; }

        [JsonProperty("group")]
        /// <summary>
        /// Gets or sets the Group
        /// </summary>
        public string Group { get; set; }

        [JsonProperty("type")]
        /// <summary>
        /// Gets or sets the Type
        /// </summary>
        public string Type { get; set; }

        [JsonProperty("properties")]
        [JsonConverter(typeof(DevicePropertiesConverter))]
        /// <summary>
        /// Gets or sets the Properties
        /// </summary>
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
    /// Represents a DevicePropertiesConverter
    /// </summary>
    public class DevicePropertiesConverter : JsonConverter
    {

        /// <summary>
        /// CanConvert method
        /// </summary>
        public override bool CanConvert(Type objectType)
        {
            return objectType == typeof(JToken);
        }

        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
        {
            return JToken.ReadFrom(reader);
        }

        /// <inheritdoc />
        public override bool CanWrite
        {
            get
            {
                return false;
            }
        }

        /// <summary>
        /// WriteJson method
        /// </summary>
        /// <inheritdoc />
        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            throw new NotImplementedException("SOD OFF HOSER");
        }
    }
}