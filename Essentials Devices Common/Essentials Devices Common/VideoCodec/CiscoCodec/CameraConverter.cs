//using System;
//using System.Collections.Generic;
//using System.Linq;
//using System.Text;
//using Crestron.SimplSharp;

//namespace PepperDash.Essentials.Devices.Common.VideoCodec.CiscoCodec
//{
//    /// <summary>
//    /// This helps convert the camera section differences between Spark and Spark+
//    /// One of them returns an object. One returns an array.
//    /// </summary>
//    public class CameraConverter : JsonConverter
//    {

//        public override bool CanConvert(System.Type objectType)
//        {
//            return objectType == typeof(Camera) || objectType == typeof(List<Camera>);
//        }

//        public override object ReadJson(JsonReader reader, System.Type objectType, object existingValue, JsonSerializer serializer)
//        {
//            if (reader.TokenType == JsonToken.StartArray)
//            {
//                var l = new List<Camera>();
//                reader.Read();
//                while (reader.TokenType != JsonToken.EndArray)
//                {
//                    l.Add(reader.Value as Camera);
//                    reader.Read();
//                }
//                return l;
//            }
//            else
//            {
//                return new List<Camera> { reader.Value as Camera };
//            }
//        }

//        public override bool CanWrite
//        {
//            get
//            {
//                return false;
//            }
//        }

//        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
//        {
//            throw new NotImplementedException("SOD OFF HOSER");
//        }
//    }
//}