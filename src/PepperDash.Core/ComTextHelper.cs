using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace PepperDash.Core
{
  /// <summary>
  /// Helper class for formatting communication text and byte data for debugging purposes.
  /// </summary>
  public class ComTextHelper
  {
    /// <summary>
    /// Gets escaped text for a byte array
    /// </summary>
    /// <param name="bytes"></param>
    /// <returns>string with all bytes escaped</returns>
    public static string GetEscapedText(byte[] bytes)
    {
      return string.Concat(bytes.Select(b => string.Format(@"[{0:X2}]", (int)b)).ToArray());
    }

    /// <summary>
    /// Gets escaped text for a string
    /// </summary>
    /// <param name="text"></param>
    /// <returns>string with all bytes escaped</returns>
    public static string GetEscapedText(string text)
    {
      var bytes = Encoding.GetEncoding(28591).GetBytes(text);
      return string.Concat(bytes.Select(b => string.Format(@"[{0:X2}]", (int)b)).ToArray());
    }

    /// <summary>
    /// Gets debug text for a string
    /// </summary>
    /// <param name="text"></param>
    /// <returns>string with all non-printable characters escaped</returns>
    public static string GetDebugText(string text)
    {
      return Regex.Replace(text, @"[^\u0020-\u007E]", a => GetEscapedText(a.Value));
    }
  }
}