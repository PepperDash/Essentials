extern alias Full;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Crestron.SimplSharp;
using Crestron.SimplSharpPro;

using Full.Newtonsoft.Json;
using Full.Newtonsoft.Json.Linq;

using PepperDash.Core;


namespace PepperDash.Essentials.Core
{
    /// <summary>
    /// This converter creates a proper ComPort.ComPortSpec struct from more-friendly JSON values.  It uses
    /// ComSpecPropsJsonConverter to finish the individual properties.
    /// </summary>
    public class ComSpecJsonConverter : JsonConverter
    {
        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
        {
            if (objectType == typeof(ComPort.ComPortSpec))
            {
                var newSer = new JsonSerializer();
                newSer.Converters.Add(new ComSpecPropsJsonConverter());
                newSer.ObjectCreationHandling = ObjectCreationHandling.Replace;
                return newSer.Deserialize<ComPort.ComPortSpec>(reader);
            }
            return null;
        }

        /// <summary>
        /// 
        /// </summary>
        public override bool CanConvert(Type objectType)
        {
            return objectType == typeof(ComPort.ComPortSpec);
        }

        public override bool CanRead { get { return true; } }

        /// <summary>
        /// This converter will not be used for writing
        /// </summary>
        public override bool CanWrite { get { return false; } }

        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            throw new NotImplementedException();
        }
    }
}