using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Crestron.SimplSharp;

namespace PepperDash.Core
{
    public class EncodingHelper
    {
        public static string ConvertUtf8ToAscii(string utf8String)
        {
            return Encoding.ASCII.GetString(Encoding.UTF8.GetBytes(utf8String), 0, utf8String.Length);
        }

        public static string ConvertUtf8ToUtf16(string utf8String)
        {
            return Encoding.Unicode.GetString(Encoding.UTF8.GetBytes(utf8String), 0, utf8String.Length);
        }

    }
}