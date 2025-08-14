using System;

namespace Crestron.SimplSharp.Net.Http
{
  /// <summary>Mock UrlParser for HTTP</summary>
  public static class UrlParser
  {
    /// <summary>Parse a URL string</summary>
    /// <param name="url">URL to parse</param>
    /// <returns>Parsed URL components</returns>
    public static UrlParserResult Parse(string url)
    {
      return new UrlParserResult { Url = url };
    }
  }

  /// <summary>URL parser result</summary>
  public class UrlParserResult
  {
    /// <summary>Original URL</summary>
    public string Url { get; set; } = string.Empty;
  }
}

namespace Crestron.SimplSharp.Net.Https
{
  /// <summary>Mock UrlParser for HTTPS - different from HTTP version</summary>
  public static class UrlParser
  {
    /// <summary>Parse a URL string</summary>
    /// <param name="url">URL to parse</param>
    /// <returns>Parsed URL components</returns>
    public static UrlParserResult Parse(string url)
    {
      return new UrlParserResult { Url = url };
    }
  }

  /// <summary>HTTPS URL parser result</summary>
  public class UrlParserResult
  {
    /// <summary>Original URL</summary>
    public string Url { get; set; } = string.Empty;
  }
}
