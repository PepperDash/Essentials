extern alias Full;
using System;
using Crestron.SimplSharpPro;
using Full::Newtonsoft.Json;

namespace PepperDash.Essentials.Core
{
    /// <summary>
    /// The gist of this converter: The comspec JSON comes in with normal values that need to be converted
    /// into enum names.  This converter takes the value and applies the appropriate enum's name prefix to the value
    /// and then returns the enum value using Enum.Parse. NOTE: Does not write
    /// </summary>
    public class ComSpecPropsJsonConverter : JsonConverter
    {
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

        public override bool CanRead { get { return true; } }

        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
        {
            //Debug.Console(2, "ReadJson type: " + objectType.Name);
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

        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            throw new NotImplementedException();
        }
    }
}