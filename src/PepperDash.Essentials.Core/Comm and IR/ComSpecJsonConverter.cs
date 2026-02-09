

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Crestron.SimplSharp;
using Crestron.SimplSharpPro;

using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

using PepperDash.Core;


namespace PepperDash.Essentials.Core
{
    /// <summary>
    /// This converter creates a proper ComPort.ComPortSpec struct from more-friendly JSON values.  It uses
    /// ComSpecPropsJsonConverter to finish the individual properties.
    /// </summary>
    public class ComSpecJsonConverter : JsonConverter
    {
        /// <summary>
        /// ReadJson method
        /// </summary>
        /// <param name="reader">reader to use</param>
        /// <param name="objectType">type of the object being read</param>
        /// <param name="existingValue">existing value of the object being read</param>
        /// <param name="serializer">serializer to use</param>
        /// <returns>deserialized object</returns>
        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
        {
            if (objectType == typeof(ComPort.ComPortSpec?))
            {
                var newSer = new JsonSerializer();
                newSer.Converters.Add(new ComSpecPropsJsonConverter());
                newSer.ObjectCreationHandling = ObjectCreationHandling.Replace;
                return newSer.Deserialize<ComPort.ComPortSpec?>(reader);
            }
            return null;
        }

        /// <summary>
        /// CanConvert method
        /// </summary>
        /// <inheritdoc />
        public override bool CanConvert(Type objectType)
        {
            return objectType == typeof(ComPort.ComPortSpec?);
        }

        /// <summary>
        /// Gets or sets the CanRead
        /// </summary>
        public override bool CanRead { get { return true; } }

        /// <summary>
        /// Gets or sets the CanWrite
        /// </summary>
        /// <inheritdoc />
        public override bool CanWrite { get { return false; } }

        /// <summary>
        /// WriteJson method
        /// </summary>
        /// <inheritdoc />
        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            throw new NotImplementedException();
        }
    }

    /// <summary>
    /// Represents a ComSpecPropsJsonConverter
    /// </summary>
    public class ComSpecPropsJsonConverter : JsonConverter
    {
        /// <inheritdoc />
        public override bool CanConvert(Type objectType)
        {
            return objectType == typeof(ComPort.eComBaudRates)
                || objectType == typeof(ComPort.eComDataBits)
                || objectType == typeof(ComPort.eComParityType)
                || objectType == typeof(ComPort.eComHardwareHandshakeType)
                || objectType == typeof(ComPort.eComSoftwareHandshakeType)
                || objectType == typeof(ComPort.eComProtocolType)
                || objectType == typeof(ComPort.eComStopBits);
        }

        /// <summary>
        /// Gets or sets the CanRead
        /// </summary>
        /// <inheritdoc />
        public override bool CanRead { get { return true; } }

        /// <summary>
        /// ReadJson method
        /// </summary>
        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
        {
            //Debug.LogMessage(LogEventLevel.Verbose, "ReadJson type: " + objectType.Name);
            if (objectType == typeof(ComPort.eComBaudRates))
                return Enum.Parse(typeof(ComPort.eComBaudRates), "ComspecBaudRate" + reader.Value, false);
            else if (objectType == typeof(ComPort.eComDataBits))
                return Enum.Parse(typeof(ComPort.eComDataBits), "ComspecDataBits" + reader.Value, true);
            else if (objectType == typeof(ComPort.eComHardwareHandshakeType))
                return Enum.Parse(typeof(ComPort.eComHardwareHandshakeType), "ComspecHardwareHandshake" + reader.Value, true);
            else if (objectType == typeof(ComPort.eComParityType))
                return Enum.Parse(typeof(ComPort.eComParityType), "ComspecParity" + reader.Value, true);
            else if (objectType == typeof(ComPort.eComProtocolType))
                return Enum.Parse(typeof(ComPort.eComProtocolType), "ComspecProtocol" + reader.Value, true);
            else if (objectType == typeof(ComPort.eComSoftwareHandshakeType))
                return Enum.Parse(typeof(ComPort.eComSoftwareHandshakeType), "ComspecSoftwareHandshake" + reader.Value, true);
            else if (objectType == typeof(ComPort.eComStopBits))
                return Enum.Parse(typeof(ComPort.eComStopBits), "ComspecStopBits" + reader.Value, true);
            return null;
        }

        /// <summary>
        /// WriteJson method
        /// </summary>
        /// <inheritdoc />
        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            throw new NotImplementedException();
        }
    }


}