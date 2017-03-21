//using System;
//using System.Collections.Generic;
//using System.Linq;
//using System.Text;
//using Crestron.SimplSharp;
//using Crestron.SimplSharpPro;

//using Newtonsoft.Json;
//using Newtonsoft.Json.Linq;

//namespace PepperDash.Essentials.Core
//{

//    public class DeviceConfig
//    {
//        public string Key { get; set; }
//        public string Name { get; set; }
//        public string Type { get; set; }
//        [JsonConverter(typeof(DevicePropertiesJsonConverter))]
//        public JToken Properties { get; set; }

		
//    }

//    /// <summary>
//    /// The gist of this converter: The comspec JSON comes in with normal values that need to be converted
//    /// into enum names.  This converter takes the value and applies the appropriate enum's name prefix to the value
//    /// and then returns the enum value using Enum.Parse
//    /// </summary>
//    public class DevicePropertiesJsonConverter : JsonConverter
//    {
//        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
//        {
//            return JObject.Load(reader);
//        }

//        /// <summary>
//        /// This will be hit with every value in the ComPortConfig class.  We only need to
//        /// do custom conversion on the comspec items.
//        /// </summary>
//        public override bool CanConvert(Type objectType) 
//        {
//            return true;
//        }

//        public override bool CanRead { get { return true; } }
//        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
//        {
//            throw new NotImplementedException();
//        }
//    }		
//}