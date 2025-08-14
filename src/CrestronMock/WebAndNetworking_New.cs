using System;
using System.Collections.Generic;
using Crestron.SimplSharp.WebScripting;

namespace Crestron.SimplSharp.Net.Http
{
  /// <summary>HTTP request types</summary>
  public enum RequestType
  {
    /// <summary>GET request</summary>
    Get = 0,
    /// <summary>POST request</summary>
    Post = 1,
    /// <summary>PUT request</summary>
    Put = 2,
    /// <summary>DELETE request</summary>
    Delete = 3,
    /// <summary>HEAD request</summary>
    Head = 4,
    /// <summary>OPTIONS request</summary>
    Options = 5,
    /// <summary>PATCH request</summary>
    Patch = 6
  }

  /// <summary>Mock HTTP client</summary>
  public static class HttpClient
  {
    /// <summary>Dispatch HTTP request</summary>
    /// <param name="request">HTTP request</param>
    /// <param name="callback">Callback for response</param>
    public static void Dispatch(HttpClientRequest request, Action<HttpClientResponse> callback)
    {
      // Mock implementation - invoke callback with empty response
      var response = new HttpClientResponse();
      callback?.Invoke(response);
    }
  }

  /// <summary>Mock HTTP client request</summary>
  public class HttpClientRequest
  {
    /// <summary>Gets or sets the URL</summary>
    public string Url { get; set; } = string.Empty;

    /// <summary>Gets or sets the HTTP method</summary>
    public string RequestType { get; set; } = "GET";

    /// <summary>Gets or sets the content data</summary>
    public string ContentString { get; set; } = string.Empty;

    /// <summary>Gets the headers collection</summary>
    public Dictionary<string, string> Header { get; } = new Dictionary<string, string>();
  }

  /// <summary>Mock HTTP client response</summary>
  public class HttpClientResponse
  {
    /// <summary>Gets the response code</summary>
    public int Code { get; set; } = 200;

    /// <summary>Gets the response content</summary>
    public string ContentString { get; set; } = string.Empty;

    /// <summary>Gets the response data as bytes</summary>
    public byte[] ContentBytes { get; set; } = Array.Empty<byte>();

    /// <summary>Gets the headers collection</summary>
    public Dictionary<string, string> Header { get; } = new Dictionary<string, string>();
  }
}

namespace Crestron.SimplSharp.Net.Https
{
  /// <summary>HTTPS request types</summary>
  public enum RequestType
  {
    /// <summary>GET request</summary>
    Get = 0,
    /// <summary>POST request</summary>
    Post = 1,
    /// <summary>PUT request</summary>
    Put = 2,
    /// <summary>DELETE request</summary>
    Delete = 3,
    /// <summary>HEAD request</summary>
    Head = 4,
    /// <summary>OPTIONS request</summary>
    Options = 5,
    /// <summary>PATCH request</summary>
    Patch = 6
  }

  /// <summary>Mock HTTPS client</summary>
  public static class HttpsClient
  {
    /// <summary>Dispatch HTTPS request</summary>
    /// <param name="request">HTTPS request</param>
    /// <param name="callback">Callback for response</param>
    public static void Dispatch(HttpsClientRequest request, Action<HttpsClientResponse> callback)
    {
      // Mock implementation - invoke callback with empty response
      var response = new HttpsClientResponse();
      callback?.Invoke(response);
    }
  }

  /// <summary>Mock HTTPS client request</summary>
  public class HttpsClientRequest
  {
    /// <summary>Gets or sets the URL</summary>
    public string Url { get; set; } = string.Empty;

    /// <summary>Gets or sets the HTTP method</summary>
    public string RequestType { get; set; } = "GET";

    /// <summary>Gets or sets the content data</summary>
    public string ContentString { get; set; } = string.Empty;

    /// <summary>Gets the headers collection</summary>
    public Dictionary<string, string> Header { get; } = new Dictionary<string, string>();
  }

  /// <summary>Mock HTTPS client response</summary>
  public class HttpsClientResponse
  {
    /// <summary>Gets the response code</summary>
    public int Code { get; set; } = 200;

    /// <summary>Gets the response content</summary>
    public string ContentString { get; set; } = string.Empty;

    /// <summary>Gets the response data as bytes</summary>
    public byte[] ContentBytes { get; set; } = Array.Empty<byte>();

    /// <summary>Gets the headers collection</summary>
    public Dictionary<string, string> Header { get; } = new Dictionary<string, string>();
  }
}

namespace Crestron.SimplSharp.CrestronLogger
{
  /// <summary>Mock Crestron logger</summary>
  public static class CrestronLogger
  {
    /// <summary>Mock log levels</summary>
    public enum LogLevel
    {
      /// <summary>Debug level</summary>
      Debug = 0,
      /// <summary>Info level</summary>
      Info = 1,
      /// <summary>Warning level</summary>
      Warning = 2,
      /// <summary>Error level</summary>
      Error = 3
    }

    /// <summary>Mock logger interface</summary>
    public interface ILogger
    {
      /// <summary>Logs a message</summary>
      /// <param name="level">Log level</param>
      /// <param name="message">Message to log</param>
      void Log(LogLevel level, string message);
    }

    /// <summary>Gets a logger by name</summary>
    /// <param name="name">Logger name</param>
    /// <returns>Mock logger instance</returns>
    public static ILogger GetLogger(string name)
    {
      return new MockLogger();
    }

    private class MockLogger : ILogger
    {
      public void Log(LogLevel level, string message)
      {
        // Mock implementation - do nothing in test environment
      }
    }
  }
}

namespace Crestron.SimplSharp.CrestronDataStore
{
  /// <summary>Mock Crestron data store</summary>
  public static class CrestronDataStore
  {
    /// <summary>Mock data store interface</summary>
    public interface IDataStore
    {
      /// <summary>Sets a value</summary>
      /// <param name="key">Key</param>
      /// <param name="value">Value</param>
      void SetValue(string key, string value);

      /// <summary>Gets a value</summary>
      /// <param name="key">Key</param>
      /// <returns>Value or null if not found</returns>
      string? GetValue(string key);
    }

    /// <summary>Gets the global data store</summary>
    /// <returns>Mock data store instance</returns>
    public static IDataStore GetGlobalDataStore()
    {
      return new MockDataStore();
    }

    private class MockDataStore : IDataStore
    {
      private readonly Dictionary<string, string> _data = new Dictionary<string, string>();

      public void SetValue(string key, string value)
      {
        _data[key] = value;
      }

      public string? GetValue(string key)
      {
        return _data.TryGetValue(key, out var value) ? value : null;
      }
    }
  }

  /// <summary>Mock HTTPS client request for data store namespace</summary>
  public class HttpsClientRequest
  {
    /// <summary>Gets or sets the request URL</summary>
    public string Url { get; set; } = string.Empty;

    /// <summary>Gets or sets the HTTP method</summary>
    public string Method { get; set; } = "GET";

    /// <summary>Gets or sets the request headers</summary>
    public HttpsHeaderCollection Headers { get; set; }

    /// <summary>Gets or sets the request content</summary>
    public string Content { get; set; } = string.Empty;

    /// <summary>Initializes a new instance of HttpsClientRequest</summary>
    public HttpsClientRequest()
    {
      Headers = new HttpsHeaderCollection();
    }
  }

  /// <summary>Mock HTTPS client response for data store namespace</summary>
  public class HttpsClientResponse
  {
    /// <summary>Gets or sets the response status code</summary>
    public int StatusCode { get; set; } = 200;

    /// <summary>Gets or sets the response content</summary>
    public string Content { get; set; } = string.Empty;

    /// <summary>Gets or sets the response headers</summary>
    public HttpsHeaderCollection Headers { get; set; }

    /// <summary>Initializes a new instance of HttpsClientResponse</summary>
    public HttpsClientResponse()
    {
      Headers = new HttpsHeaderCollection();
    }
  }

  /// <summary>Mock HTTPS header</summary>
  public class HttpsHeader
  {
    /// <summary>Gets the header name</summary>
    public string Name { get; private set; }

    /// <summary>Gets the header value</summary>
    public string Value { get; private set; }

    /// <summary>Initializes a new instance of HttpsHeader</summary>
    /// <param name="name">Header name</param>
    /// <param name="value">Header value</param>
    public HttpsHeader(string name, string value)
    {
      Name = name;
      Value = value;
    }
  }

  /// <summary>Mock HTTPS header collection</summary>
  public class HttpsHeaderCollection
  {
    private readonly List<HttpsHeader> _headers = new List<HttpsHeader>();

    /// <summary>Adds a header to the collection</summary>
    /// <param name="header">Header to add</param>
    public void AddHeader(HttpsHeader header)
    {
      _headers.Add(header);
    }

    /// <summary>Gets all headers</summary>
    /// <returns>Array of headers</returns>
    public HttpsHeader[] GetHeaders()
    {
      return _headers.ToArray();
    }
  }

  /// <summary>Mock HTTPS client for data store namespace</summary>
  public class HttpsClient
  {
    /// <summary>Dispatch HTTPS request</summary>
    /// <param name="request">HTTPS request</param>
    /// <param name="callback">Callback for response</param>
    public void Dispatch(HttpsClientRequest request, Action<HttpsClientResponse> callback)
    {
      // Mock implementation - invoke callback with empty response
      var response = new HttpsClientResponse();
      callback?.Invoke(response);
    }
  }

  /// <summary>Mock URL parser</summary>
  public class UrlParser
  {
    /// <summary>Gets the parsed URL</summary>
    public string Url { get; private set; }

    /// <summary>Initializes a new instance of UrlParser</summary>
    /// <param name="url">URL to parse</param>
    public UrlParser(string url)
    {
      Url = url;
    }

    /// <summary>Implicit conversion to string</summary>
    /// <param name="parser">URL parser</param>
    public static implicit operator string(UrlParser parser)
    {
      return parser.Url;
    }
  }

  /// <summary>Mock HTTP exception</summary>
  public class HttpException : Exception
  {
    /// <summary>Initializes a new instance of HttpException</summary>
    public HttpException() : base()
    {
    }

    /// <summary>Initializes a new instance of HttpException</summary>
    /// <param name="message">Exception message</param>
    public HttpException(string message) : base(message)
    {
    }

    /// <summary>Initializes a new instance of HttpException</summary>
    /// <param name="message">Exception message</param>
    /// <param name="innerException">Inner exception</param>
    public HttpException(string message, Exception innerException) : base(message, innerException)
    {
    }
  }
}
