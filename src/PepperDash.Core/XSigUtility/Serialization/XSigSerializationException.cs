using System;

namespace PepperDash.Core.Intersystem.Serialization
{
    /// <summary>
    /// Class to handle this specific exception type
    /// </summary>
    public class XSigSerializationException : Exception
    {
        /// <summary>
        /// default constructor
        /// </summary>
        public XSigSerializationException() { }

        /// <summary>
        /// constructor with message
        /// </summary>
        /// <param name="message"></param>
        public XSigSerializationException(string message) : base(message) { }

        /// <summary>
        /// constructor with message and innner exception
        /// </summary>
        /// <param name="message"></param>
        /// <param name="inner"></param>
        public XSigSerializationException(string message, Exception inner) : base(message, inner) { }
    }
}