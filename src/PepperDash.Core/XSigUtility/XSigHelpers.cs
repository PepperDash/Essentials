using System;
using System.Linq;
using Crestron.SimplSharp.CrestronIO;
using PepperDash.Core.Intersystem.Serialization;
using PepperDash.Core.Intersystem.Tokens;

/*
    Digital (2 bytes)
        10C##### 0####### (mask = 11000000_10000000b -> 0xC080)

    Analog (4 bytes)
        11aa0### 0####### (mask = 11001000_10000000b -> 0xC880)
        0aaaaaaa 0aaaaaaa

    Serial (Variable length)
        11001### 0####### (mask = 11111000_10000000b -> 0xF880)
        dddddddd ........ <- up to 252 bytes of serial data (255 - 3)
        11111111 <- denotes end of data
*/

namespace PepperDash.Core.Intersystem
{
    /// <summary>
    /// Helper methods for creating XSig byte sequences compatible with the Intersystem Communications (ISC) symbol.
    /// </summary>
    /// <remarks>
    /// Indexing is not from the start of each signal type but rather from the beginning of the first defined signal
    /// the Intersystem Communications (ISC) symbol.
    /// </remarks>
    public static class XSigHelpers
    {
        /// <summary>
        /// Forces all outputs to 0.
        /// </summary>
        /// <returns>Bytes in XSig format for clear outputs trigger.</returns>
        public static byte[] ClearOutputs()
        {
            return new byte[] { 0xFC };
        }

        /// <summary>
        /// Evaluate all inputs and re-transmit any digital, analog, and permanent serail signals not set to 0.
        /// </summary>
        /// <returns>Bytes in XSig format for send status trigger.</returns>
        public static byte[] SendStatus()
        {
            return new byte[] { 0xFD };
        }

        /// <summary>
        /// Get bytes for an IXSigStateResolver object.
        /// </summary>
        /// <param name="xSigSerialization">XSig state resolver.</param>
        /// <returns>Bytes in XSig format for each token within the state representation.</returns>
        public static byte[] GetBytes(IXSigSerialization xSigSerialization)
        {
            return GetBytes(xSigSerialization, 0);
        }

        /// <summary>
        /// Get bytes for an IXSigStateResolver object, with a specified offset.
        /// </summary>
        /// <param name="xSigSerialization">XSig state resolver.</param>
        /// <param name="offset">Offset to which the data will be aligned.</param>
        /// <returns>Bytes in XSig format for each token within the state representation.</returns>
        public static byte[] GetBytes(IXSigSerialization xSigSerialization, int offset)
        {
            var tokens = xSigSerialization.Serialize();
            if (tokens == null) return new byte[0];
            using (var memoryStream = new MemoryStream())
            {
                using (var tokenWriter = new XSigTokenStreamWriter(memoryStream))
                    tokenWriter.WriteXSigData(xSigSerialization, offset);

                return memoryStream.ToArray();
            }
        }

        /// <summary>
        /// Get bytes for a single digital signal.
        /// </summary>
        /// <param name="index">1-based digital index</param>
        /// <param name="value">Digital data to be encoded</param>
        /// <returns>Bytes in XSig format for digtial information.</returns>
        public static byte[] GetBytes(int index, bool value)
        {
            return GetBytes(index, 0, value);
        }

        /// <summary>
        /// Get bytes for a single digital signal.
        /// </summary>
        /// <param name="index">1-based digital index</param>
        /// <param name="offset">Index offset.</param>
        /// <param name="value">Digital data to be encoded</param>
        /// <returns>Bytes in XSig format for digtial information.</returns>
        public static byte[] GetBytes(int index, int offset, bool value)
        {
            return new XSigDigitalToken(index + offset, value).GetBytes();
        }

        /// <summary>
        /// Get byte sequence for multiple digital signals.
        /// </summary>
        /// <param name="startIndex">Starting index of the sequence.</param>
        /// <param name="values">Digital signal value array.</param>
        /// <returns>Byte sequence in XSig format for digital signal information.</returns>
        public static byte[] GetBytes(int startIndex, bool[] values)
        {
            return GetBytes(startIndex, 0, values);
        }

        /// <summary>
        /// Get byte sequence for multiple digital signals.
        /// </summary>
        /// <param name="startIndex">Starting index of the sequence.</param>
        /// <param name="offset">Index offset.</param>
        /// <param name="values">Digital signal value array.</param>
        /// <returns>Byte sequence in XSig format for digital signal information.</returns>
        public static byte[] GetBytes(int startIndex, int offset, bool[] values)
        {
            // Digital XSig data is 2 bytes per value
            const int fixedLength = 2;
            var bytes = new byte[values.Length * fixedLength];
            for (var i = 0; i < values.Length; i++)
                Buffer.BlockCopy(GetBytes(startIndex++, offset, values[i]), 0, bytes, i * fixedLength, fixedLength);

            return bytes;
        }

        /// <summary>
        /// Get bytes for a single analog signal.
        /// </summary>
        /// <param name="index">1-based analog index</param>
        /// <param name="value">Analog data to be encoded</param>
        /// <returns>Bytes in XSig format for analog signal information.</returns>
        public static byte[] GetBytes(int index, ushort value)
        {
            return GetBytes(index, 0, value);
        }

        /// <summary>
        /// Get bytes for a single analog signal.
        /// </summary>
        /// <param name="index">1-based analog index</param>
        /// <param name="offset">Index offset.</param>
        /// <param name="value">Analog data to be encoded</param>
        /// <returns>Bytes in XSig format for analog signal information.</returns>
        public static byte[] GetBytes(int index, int offset, ushort value)
        {
            return new XSigAnalogToken(index + offset, value).GetBytes();
        }

        /// <summary>
        /// Get byte sequence for multiple analog signals.
        /// </summary>
        /// <param name="startIndex">Starting index of the sequence.</param>
        /// <param name="values">Analog signal value array.</param>
        /// <returns>Byte sequence in XSig format for analog signal information.</returns>
        public static byte[] GetBytes(int startIndex, ushort[] values)
        {
            return GetBytes(startIndex, 0, values);
        }

        /// <summary>
        /// Get byte sequence for multiple analog signals.
        /// </summary>
        /// <param name="startIndex">Starting index of the sequence.</param>
        /// <param name="offset">Index offset.</param>
        /// <param name="values">Analog signal value array.</param>
        /// <returns>Byte sequence in XSig format for analog signal information.</returns>
        public static byte[] GetBytes(int startIndex, int offset, ushort[] values)
        {
            // Analog XSig data is 4 bytes per value
            const int fixedLength = 4;
            var bytes = new byte[values.Length * fixedLength];
            for (var i = 0; i < values.Length; i++)
                Buffer.BlockCopy(GetBytes(startIndex++, offset, values[i]), 0, bytes, i * fixedLength, fixedLength);

            return bytes;
        }

        /// <summary>
        /// Get bytes for a single serial signal.
        /// </summary>
        /// <param name="index">1-based serial index</param>
        /// <param name="value">Serial data to be encoded</param>
        /// <returns>Bytes in XSig format for serial signal information.</returns>
        public static byte[] GetBytes(int index, string value)
        {
            return GetBytes(index, 0, value);
        }

        /// <summary>
        /// Get bytes for a single serial signal.
        /// </summary>
        /// <param name="index">1-based serial index</param>
        /// <param name="offset">Index offset.</param>
        /// <param name="value">Serial data to be encoded</param>
        /// <returns>Bytes in XSig format for serial signal information.</returns>
        public static byte[] GetBytes(int index, int offset, string value)
        {
            return new XSigSerialToken(index + offset, value).GetBytes();
        }

        /// <summary>
        /// Get byte sequence for multiple serial signals.
        /// </summary>
        /// <param name="startIndex">Starting index of the sequence.</param>
        /// <param name="values">Serial signal value array.</param>
        /// <returns>Byte sequence in XSig format for serial signal information.</returns>
        public static byte[] GetBytes(int startIndex, string[] values)
        {
            return GetBytes(startIndex, 0, values);
        }

        /// <summary>
        /// Get byte sequence for multiple serial signals.
        /// </summary>
        /// <param name="startIndex">Starting index of the sequence.</param>
        /// <param name="offset">Index offset.</param>
        /// <param name="values">Serial signal value array.</param>
        /// <returns>Byte sequence in XSig format for serial signal information.</returns>
        public static byte[] GetBytes(int startIndex, int offset, string[] values)
        {
            // Serial XSig data is not fixed-length like the other formats
            var dstOffset = 0;
            var bytes = new byte[values.Sum(v => v.Length + 3)];
            for (var i = 0; i < values.Length; i++)
            {
                var data = GetBytes(startIndex++, offset, values[i]);
                Buffer.BlockCopy(data, 0, bytes, dstOffset, data.Length);
                dstOffset += data.Length;
            }

            return bytes;
        }
    }
}