using System;
using System.Collections.Generic;
using Crestron.SimplSharp.CrestronIO;
using PepperDash.Core.Intersystem.Serialization;
using PepperDash.Core.Intersystem.Tokens;

namespace PepperDash.Core.Intersystem
{
    /// <summary>
    /// XSigToken stream reader.
    /// </summary>
    public sealed class XSigTokenStreamReader : IDisposable
    {
        private readonly Stream _stream;
        private readonly bool _leaveOpen;

        /// <inheritdoc />
        /// <summary>
        /// XSigToken stream reader constructor.
        /// </summary>
        /// <param name="stream">Input stream to read from.</param>
        /// <exception cref="T:System.ArgumentNullException">Stream is null.</exception>
        /// <exception cref="T:System.ArgumentException">Stream cannot be read from.</exception>
        public XSigTokenStreamReader(Stream stream)
            : this(stream, false) { }

        /// <summary>
        /// XSigToken stream reader constructor.
        /// </summary>
        /// <param name="stream">Input stream to read from.</param>
        /// <param name="leaveOpen">Determines whether to leave the stream open or not.</param>
        /// <exception cref="ArgumentNullException">Stream is null.</exception>
        /// <exception cref="ArgumentException">Stream cannot be read from.</exception>
        public XSigTokenStreamReader(Stream stream, bool leaveOpen)
        {
            if (stream == null)
                throw new ArgumentNullException("stream");
            if (!stream.CanRead)
                throw new ArgumentException("The specified stream cannot be read from.");

            _stream = stream;
            _leaveOpen = leaveOpen;
        }

        /// <summary>
        /// Reads a 16-bit unsigned integer from the specified stream using Big Endian byte order.
        /// </summary>
        /// <param name="stream">Input stream</param>
        /// <param name="value">Result</param>
        /// <returns>True if successful, otherwise false.</returns>
        public static bool TryReadUInt16BE(Stream stream, out ushort value)
        {
            value = 0;
            if (stream.Length < 2)
                return false;

            var buffer = new byte[2];
            stream.Read(buffer, 0, 2);
            value = (ushort)((buffer[0] << 8) | buffer[1]);
            return true;
        }

        /// <summary>
        /// Read XSig token from the stream.
        /// </summary>
        /// <returns>XSigToken</returns>
        /// <exception cref="ArgumentOutOfRangeException">Offset is less than 0.</exception>
        public XSigToken ReadXSigToken()
        {
            ushort prefix;
            if (!TryReadUInt16BE(_stream, out prefix))
                return null;

            if ((prefix & 0xF880) == 0xC800) // Serial data
            {
                var index = ((prefix & 0x0700) >> 1) | (prefix & 0x7F);
                var n = 0;
                const int maxSerialDataLength = 252;
                var chars = new char[maxSerialDataLength];
                int ch;
                while ((ch = _stream.ReadByte()) != 0xFF)
                {
                    if (ch == -1) // Reached end of stream without end of data marker
                        return null;
                    
                    chars[n++] = (char)ch;
                }

                return new XSigSerialToken((ushort)(index + 1), new string(chars, 0, n));
            }

            if ((prefix & 0xC880) == 0xC000) // Analog data
            {
                ushort data;
                if (!TryReadUInt16BE(_stream, out data))
                    return null;

                var index = ((prefix & 0x0700) >> 1) | (prefix & 0x7F);
                var value = ((prefix & 0x3000) << 2) | ((data & 0x7F00) >> 1) | (data & 0x7F);
                return new XSigAnalogToken((ushort)(index + 1), (ushort)value);
            }

            if ((prefix & 0xC080) == 0x8000) // Digital data
            {
                var index = ((prefix & 0x1F00) >> 1) | (prefix & 0x7F);
                var value = (prefix & 0x2000) == 0;
                return new XSigDigitalToken((ushort)(index + 1), value);
            }

            return null;
        }

        /// <summary>
        /// Reads all available XSig tokens from the stream.
        /// </summary>
        /// <returns>XSigToken collection.</returns>
        public IEnumerable<XSigToken> ReadAllXSigTokens()
        {
            var tokens = new List<XSigToken>();
            XSigToken token;
            while ((token = ReadXSigToken()) != null)
                tokens.Add(token);

            return tokens;
        }

        /// <summary>
        /// Attempts to deserialize all XSig data within the stream from the current position.
        /// </summary>
        /// <typeparam name="T">Type to deserialize the information to.</typeparam>
        /// <returns>Deserialized object.</returns>
        public T DeserializeStream<T>()
            where T : class, IXSigSerialization, new()
        {
            return new T().Deserialize<T>(ReadAllXSigTokens());
        }

        /// <summary>
        /// Disposes of the internal stream if specified to not leave open.
        /// </summary>
        public void Dispose()
        {
            if (!_leaveOpen)
                _stream.Dispose();
        }
    }
}