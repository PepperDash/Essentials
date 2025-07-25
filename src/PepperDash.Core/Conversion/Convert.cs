using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Crestron.SimplSharp;

namespace PepperDash.Core
{
    /// <summary>
    /// Represents a EncodingHelper
    /// </summary>
    public class EncodingHelper
    {
        /// <summary>
        /// ConvertUtf8ToAscii method
        /// </summary>
        public static string ConvertUtf8ToAscii(string utf8String)
        {
            return Encoding.ASCII.GetString(Encoding.UTF8.GetBytes(utf8String), 0, utf8String.Length);
        }

        /// <summary>
        /// ConvertUtf8ToUtf16 method
        /// </summary>
        public static string ConvertUtf8ToUtf16(string utf8String)
        {
            return Encoding.Unicode.GetString(Encoding.UTF8.GetBytes(utf8String), 0, utf8String.Length);
        }

    }
}