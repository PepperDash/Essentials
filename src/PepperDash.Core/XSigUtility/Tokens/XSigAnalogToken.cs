using System;

namespace PepperDash.Core.Intersystem.Tokens
{
    /// <summary>
    /// Represents an XSigAnalogToken
    /// </summary>
    public sealed class XSigAnalogToken : XSigToken, IFormattable
    {
        private readonly ushort _value;

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="index"></param>
        /// <param name="value"></param>
        public XSigAnalogToken(int index, ushort value)
            : base(index)
        {
            // 10-bits available for analog encoded data
            if (index >= 1024 || index < 0)
                throw new ArgumentOutOfRangeException("index");

            _value = value;
        }

        /// <summary>
        /// 
        /// </summary>
        public ushort Value
        {
            get { return _value; }
        }

        /// <summary>
        /// 
        /// </summary>
        public override XSigTokenType TokenType
        {
            get { return XSigTokenType.Analog; }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public override byte[] GetBytes()
        {
            return new[] {
                (byte)(0xC0 | ((Value & 0xC000) >> 10) | (Index - 1 >> 7)),
                (byte)((Index - 1) & 0x7F),
                (byte)((Value & 0x3F80) >> 7),
                (byte)(Value & 0x7F)
            };
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="offset"></param>
        /// <returns></returns>
        public override XSigToken GetTokenWithOffset(int offset)
        {
            if (offset == 0) return this;
            return new XSigAnalogToken(Index + offset, Value);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            return Index + " = 0x" + Value.ToString("X4");
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="format"></param>
        /// <param name="formatProvider"></param>
        /// <returns></returns>
        public string ToString(string format, IFormatProvider formatProvider)
        {
            return Value.ToString(format, formatProvider);
        }
    }
}