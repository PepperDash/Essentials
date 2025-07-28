using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace PepperDash.Essentials.Core.Communications
{
    /// <summary>
    /// 
    /// </summary>
    public class ComTextHelper
	{
        /// <summary>
        /// Gets escaped text for a byte array
        /// </summary>
        /// <param name="bytes"></param>
        /// <returns></returns>
		public static string GetEscapedText(byte[] bytes)
		{
			return string.Concat(bytes.Select(b => string.Format(@"[{0:X2}]", (int)b)).ToArray());
		}

        /// <summary>
        /// Gets escaped text for a string
        /// </summary>
        /// <param name="text"></param>
        /// <returns></returns>
  /// <summary>
  /// GetEscapedText method
  /// </summary>
		public static string GetEscapedText(string text)
		{
			var bytes = Encoding.GetEncoding(28591).GetBytes(text);
			return string.Concat(bytes.Select(b => string.Format(@"[{0:X2}]", (int)b)).ToArray());
		}

        /// <summary>
        /// Gets debug text for a string
        /// </summary>
        /// <param name="text"></param>
        /// <returns></returns>
        /// <summary>
        /// GetDebugText method
        /// </summary>
        public static string GetDebugText(string text)
        {
            return Regex.Replace(text, @"[^\u0020-\u007E]", a => GetEscapedText(a.Value));
        }
	}
}