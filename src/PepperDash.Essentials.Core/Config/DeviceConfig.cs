

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
    /// <summary>
    /// Represents a DeviceConfig
    /// </summary>
    public class DeviceConfig
    {
        /// <summary>
        /// Gets or sets the Key
        /// </summary>
        [JsonProperty("key")]
        public string Key { get; set; }

        /// <summary>
        /// Gets or sets the Uid
        /// </summary>
        [JsonProperty("uid")]
        public int Uid { get; set; }

        /// <summary>
        /// Gets or sets the Name
        /// </summary>
        [JsonProperty("name")]
        public string Name { get; set; }

        /// <summary>
        /// Gets or sets the Group
        /// </summary>
        [JsonProperty("group")]
        public string Group { get; set; }

        /// <summary>
        /// Gets or sets the Type
        /// </summary>
        [JsonProperty("type")]
        public string Type { get; set; }

        /// <summary>
        /// Gets or sets the Properties
        /// </summary>
        [JsonProperty("properties")]
        [JsonConverter(typeof(DevicePropertiesConverter))]
        public JToken Properties { get; set; }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="dc">device config</param>
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

        /// <summary>
        /// Default Constructor
        /// </summary>
        public DeviceConfig() { }
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

        /// <summary>
        /// ReadJson method
        /// </summary>
        /// <param name="reader">reader to use</param>
        /// <param name="objectType">type of object being read</param>
        /// <param name="existingValue">existing value for the object</param>
        /// <param name="serializer">serializer to use</param>
        /// <returns></returns>
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