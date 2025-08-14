using System;
using System.Collections.Generic;

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
  public class HttpClient
  {
    /// <summary>Gets or sets the keep-alive setting</summary>
    public bool KeepAlive { get; set; } = false;

    /// <summary>Gets or sets the port number</summary>
    public int Port { get; set; } = 80;

    /// <summary>Dispatch HTTP request</summary>
    /// <param name="request">HTTP request</param>
    /// <param name="callback">Callback for response</param>
    public void Dispatch(HttpClientRequest request, Action<HttpClientResponse> callback)
    {
      // Mock implementation - invoke callback with empty response
      var response = new HttpClientResponse();
      callback?.Invoke(response);
    }

    /// <summary>Dispatches HTTP request synchronously</summary>
    /// <param name="request">HTTP request</param>
    /// <returns>HTTP response</returns>
    public HttpClientResponse Dispatch(HttpClientRequest request)
    {
      // Mock implementation - return empty response
      return new HttpClientResponse();
    }
  }

  /// <summary>Mock HTTP client request</summary>
  public class HttpClientRequest
  {
    /// <summary>Gets or sets the URL parser</summary>
    public Crestron.SimplSharp.Net.Http.UrlParserResult Url { get; set; } = new Crestron.SimplSharp.Net.Http.UrlParserResult();

    /// <summary>Gets or sets the HTTP method</summary>
    public RequestType RequestType { get; set; } = RequestType.Get;

    /// <summary>Gets or sets the content data</summary>
    public string ContentString { get; set; } = string.Empty;

    /// <summary>Gets the headers collection</summary>
    public HttpHeaderCollection Header { get; } = new HttpHeaderCollection();
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
    public HttpHeaderCollection Header { get; } = new HttpHeaderCollection();
  }

  /// <summary>Mock HTTP header collection</summary>
  public class HttpHeaderCollection
  {
    private readonly Dictionary<string, string> _headers = new Dictionary<string, string>();

    /// <summary>Gets or sets the content type</summary>
    public string ContentType
    {
      get => _headers.TryGetValue("Content-Type", out var value) ? value : string.Empty;
      set => _headers["Content-Type"] = value;
    }

    /// <summary>Sets a header value</summary>
    /// <param name="name">Header name</param>
    /// <param name="value">Header value</param>
    public void SetHeaderValue(string name, string value)
    {
      _headers[name] = value;
    }

    /// <summary>Gets a header value</summary>
    /// <param name="name">Header name</param>
    /// <returns>Header value or empty string if not found</returns>
    public string GetHeaderValue(string name)
    {
      return _headers.TryGetValue(name, out var value) ? value : string.Empty;
    }
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
  public class HttpsClient
  {
    /// <summary>Gets or sets the keep-alive setting</summary>
    public bool KeepAlive { get; set; } = false;

    /// <summary>Gets or sets the host verification setting</summary>
    public bool HostVerification { get; set; } = false;

    /// <summary>Gets or sets the peer verification setting</summary>
    public bool PeerVerification { get; set; } = false;

    /// <summary>Dispatch HTTPS request</summary>
    /// <param name="request">HTTPS request</param>
    /// <param name="callback">Callback for response</param>
    public void Dispatch(HttpsClientRequest request, Action<HttpsClientResponse> callback)
    {
      // Mock implementation - invoke callback with empty response
      var response = new HttpsClientResponse();
      callback?.Invoke(response);
    }

    /// <summary>Dispatches HTTPS request synchronously</summary>
    /// <param name="request">HTTPS request</param>
    /// <returns>HTTPS response</returns>
    public HttpsClientResponse Dispatch(HttpsClientRequest request)
    {
      // Mock implementation - return empty response
      return new HttpsClientResponse();
    }
  }

  /// <summary>Mock HTTPS client request</summary>
  public class HttpsClientRequest
  {
    /// <summary>Gets or sets the URL parser</summary>
    public Crestron.SimplSharp.Net.Https.UrlParserResult Url { get; set; } = new Crestron.SimplSharp.Net.Https.UrlParserResult();

    /// <summary>Gets or sets the HTTP method</summary>
    public RequestType RequestType { get; set; } = RequestType.Get;

    /// <summary>Gets or sets the content data</summary>
    public string ContentString { get; set; } = string.Empty;

    /// <summary>Gets the headers collection</summary>
    public HttpsHeaderCollection Header { get; } = new HttpsHeaderCollection();
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
    public HttpsHeaderCollection Header { get; } = new HttpsHeaderCollection();
  }

  /// <summary>Mock HTTPS header collection</summary>
  public class HttpsHeaderCollection
  {
    private readonly Dictionary<string, string> _headers = new Dictionary<string, string>();

    /// <summary>Gets or sets the content type</summary>
    public string ContentType
    {
      get => _headers.TryGetValue("Content-Type", out var value) ? value : string.Empty;
      set => _headers["Content-Type"] = value;
    }

    /// <summary>Sets a header value</summary>
    /// <param name="name">Header name</param>
    /// <param name="value">Header value</param>
    public void SetHeaderValue(string name, string value)
    {
      _headers[name] = value;
    }

    /// <summary>Adds a header</summary>
    /// <param name="header">Header to add</param>
    public void AddHeader(HttpsHeader header)
    {
      _headers[header.Name] = header.Value;
    }

    /// <summary>Gets a header value</summary>
    /// <param name="name">Header name</param>
    /// <returns>Header value or empty string if not found</returns>
    public string GetHeaderValue(string name)
    {
      return _headers.TryGetValue(name, out var value) ? value : string.Empty;
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
      Name = name ?? string.Empty;
      Value = value ?? string.Empty;
    }
  }

  /// <summary>Mock HTTP exception</summary>
  public class HttpException : Exception
  {
    /// <summary>Gets the HTTP response</summary>
    public HttpsClientResponse Response { get; }

    /// <summary>Initializes a new instance of HttpException</summary>
    public HttpException() : base()
    {
      Response = new HttpsClientResponse();
    }

    /// <summary>Initializes a new instance of HttpException</summary>
    /// <param name="message">Exception message</param>
    public HttpException(string message) : base(message)
    {
      Response = new HttpsClientResponse();
    }

    /// <summary>Initializes a new instance of HttpException</summary>
    /// <param name="message">Exception message</param>
    /// <param name="innerException">Inner exception</param>
    public HttpException(string message, Exception innerException) : base(message, innerException)
    {
      Response = new HttpsClientResponse();
    }

    /// <summary>Initializes a new instance of HttpException</summary>
    /// <param name="message">Exception message</param>
    /// <param name="response">HTTP response</param>
    public HttpException(string message, HttpsClientResponse response) : base(message)
    {
      Response = response ?? new HttpsClientResponse();
    }
  }


}
