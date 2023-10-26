extern alias Full;
using System;
using Full::Newtonsoft.Json;
using Full::Newtonsoft.Json.Linq;

namespace PepperDash.Essentials.Core.Config
{
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