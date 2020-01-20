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
    public class DeviceConfig : PropertiesConfigBase
    {
        [JsonProperty("key", Required = Required.Always)]
        public string Key { get; set; }

        [JsonProperty("uid")]
        public int Uid { get; set; }

        [JsonProperty("name")]
        public string Name { get; set; }

        [JsonProperty("group")]
        public string Group { get; set; }

        [JsonProperty("type", Required = Required.Always)]
        public string Type { get; set; }

        [JsonProperty("properties", Required = Required.Always)]
        [JsonConverter(typeof(DevicePropertiesConverter))]
        public JToken Properties { get; set; }

        public DeviceConfig()
        {
            SchemaJson = @"
{
  'definitions': {},
  '$schema': 'http://json-schema.org/draft-07/schema#',
  '$id': 'http://example.com/root.json',
  'type': 'object',
  'title': 'The Root Schema',
  'properties': {
    'name': {
      '$id': '#/properties/name',
      'type': 'string',
      'title': 'The Name Schema',
      'default': '',
      'examples': [
        'App Server'
      ],
      'pattern': '^(.*)$'
    },
    'group': {
      '$id': '#/properties/group',
      'type': 'string',
      'title': 'The Group Schema',
      'default': '',
      'examples': [
        'appServer'
      ],
      'pattern': '^(.*)$'
    },
    'properties': {
      '$id': '#/properties/properties',
      'type': 'object',
      'title': 'The Properties Schema'
    },
    'uid': {
      '$id': '#/properties/uid',
      'type': 'integer',
      'title': 'The Uid Schema',
      'default': 0,
      'examples': [
        4
      ]
    },
    'key': {
      '$id': '#/properties/key',
      'type': 'string',
      'title': 'The Key Schema',
      'default': '',
      'examples': [
        'display-1'
      ],
      'pattern': '^(.*)$'
    },
    'type': {
      '$id': '#/properties/type',
      'type': 'string',
      'title': 'The Type Schema',
      'default': '',
      'examples': [
        'appServer'
      ],
      'pattern': '^(.*)$'
    }
  },
  'required': [
    'group',
    'properties',
    'key',
    'type'
  ]
}
";
        }
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
            throw new NotImplementedException("Not Supported");
        }
    }
}