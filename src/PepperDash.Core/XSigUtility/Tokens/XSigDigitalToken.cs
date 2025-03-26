using System;

namespace PepperDash.Core.Intersystem.Tokens
{
    /// <summary>
    /// Represents an XSigDigitalToken
    /// </summary>
    public sealed class XSigDigitalToken : XSigToken
    {
        private readonly bool _value;

        /// <summary>
        /// 
        /// </summary>
        /// <param name="index"></param>
        /// <param name="value"></param>
        public XSigDigitalToken(int index, bool value)
            : base(index)
        {
            // 12-bits available for digital encoded data
            if (index >= 4096 || index < 0)
                throw new ArgumentOutOfRangeException("index");

            _value = value;
        }

        /// <summary>
        /// 
        /// </summary>
        public bool Value
        {
            get { return _value; }
        }

        /// <summary>
        /// 
        /// </summary>
        public override XSigTokenType TokenType
        {
            get { return XSigTokenType.Digital; }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public override byte[] GetBytes()
        {
            return new[] {
                (byte)(0x80 | (Value ? 0 : 0x20) | ((Index - 1) >> 7)),
                (byte)((Index - 1) & 0x7F)
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
            return new XSigDigitalToken(Index + offset, Value);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            return Index + " = " + (Value ? "High" : "Low");
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="formatProvider"></param>
        /// <returns></returns>
        public string ToString(IFormatProvider formatProvider)
        {
            return Value.ToString(formatProvider);
        }
    }
}