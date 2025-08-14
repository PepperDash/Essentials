using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace Crestron.SimplSharp.CrestronWebSocketServer
{
  /// <summary>Mock HttpCwsServer class for HTTP web server functionality</summary>
  public class HttpCwsServer : IDisposable
  {
    private HttpListener? _httpListener;
    private bool _listening;
    private readonly Dictionary<string, IHttpCwsHandler> _routes = new Dictionary<string, IHttpCwsHandler>();

    /// <summary>Gets or sets the port number</summary>
    public int Port { get; set; }

    /// <summary>Gets whether the server is listening</summary>
    public bool Listening => _listening;

    /// <summary>Initializes a new instance of HttpCwsServer</summary>
    public HttpCwsServer()
    {
      Port = 80;
    }

    /// <summary>Initializes a new instance of HttpCwsServer</summary>
    /// <param name="port">Port number to listen on</param>
    public HttpCwsServer(int port)
    {
      Port = port;
    }

    /// <summary>Starts the HTTP server</summary>
    /// <returns>True if started successfully</returns>
    public bool Start()
    {
      if (_listening)
        return true;

      try
      {
        _httpListener = new HttpListener();
        _httpListener.Prefixes.Add($"http://+:{Port}/");
        _httpListener.Start();
        _listening = true;

        _ = Task.Run(ProcessRequestsAsync);

        return true;
      }
      catch (Exception)
      {
        return false;
      }
    }

    /// <summary>Stops the HTTP server</summary>
    /// <returns>True if stopped successfully</returns>
    public bool Stop()
    {
      if (!_listening)
        return true;

      try
      {
        _listening = false;
        _httpListener?.Stop();
        _httpListener?.Close();
        return true;
      }
      catch (Exception)
      {
        return false;
      }
    }

    /// <summary>Adds a route handler</summary>
    /// <param name="route">Route path</param>
    /// <param name="handler">Handler for the route</param>
    public void AddRoute(string route, IHttpCwsHandler handler)
    {
      _routes[route.ToLowerInvariant()] = handler;
    }

    /// <summary>Removes a route handler</summary>
    /// <param name="route">Route path to remove</param>
    public void RemoveRoute(string route)
    {
      _routes.Remove(route.ToLowerInvariant());
    }

    private async Task ProcessRequestsAsync()
    {
      while (_listening && _httpListener != null)
      {
        try
        {
          var context = await _httpListener.GetContextAsync();
          _ = Task.Run(() => HandleRequest(context));
        }
        catch (HttpListenerException)
        {
          // Listener was stopped
          break;
        }
        catch (ObjectDisposedException)
        {
          // Listener was disposed
          break;
        }
        catch (Exception)
        {
          // Handle other exceptions
          continue;
        }
      }
    }

    private void HandleRequest(HttpListenerContext context)
    {
      try
      {
        var request = context.Request;
        var response = context.Response;

        var path = request.Url?.AbsolutePath?.ToLowerInvariant() ?? "/";

        var cwsContext = new HttpCwsContext(context);

        if (_routes.TryGetValue(path, out var handler))
        {
          handler.ProcessRequest(cwsContext);
        }
        else
        {
          // Default 404 response
          response.StatusCode = 404;
          var buffer = Encoding.UTF8.GetBytes("Not Found");
          response.ContentLength64 = buffer.Length;
          response.OutputStream.Write(buffer, 0, buffer.Length);
        }

        response.Close();
      }
      catch (Exception)
      {
        // Handle request processing errors
        try
        {
          context.Response.StatusCode = 500;
          context.Response.Close();
        }
        catch
        {
          // Ignore errors when closing response
        }
      }
    }

    /// <summary>Disposes the HttpCwsServer</summary>
    public void Dispose()
    {
      Stop();
      _httpListener?.Close();
    }
  }

  /// <summary>Mock HttpCwsContext class representing an HTTP request/response context</summary>
  public class HttpCwsContext
  {
    private readonly HttpListenerContext _context;

    /// <summary>Gets the HTTP request</summary>
    public HttpCwsRequest Request { get; }

    /// <summary>Gets the HTTP response</summary>
    public HttpCwsResponse Response { get; }

    /// <summary>Initializes a new instance of HttpCwsContext</summary>
    /// <param name="context">Underlying HttpListenerContext</param>
    public HttpCwsContext(HttpListenerContext context)
    {
      _context = context;
      Request = new HttpCwsRequest(context.Request);
      Response = new HttpCwsResponse(context.Response);
    }
  }

  /// <summary>Mock HttpCwsRequest class representing an HTTP request</summary>
  public class HttpCwsRequest
  {
    private readonly HttpListenerRequest _request;

    /// <summary>Gets the HTTP method</summary>
    public string HttpMethod => _request.HttpMethod;

    /// <summary>Gets the request URL</summary>
    public Uri? Url => _request.Url;

    /// <summary>Gets the request headers</summary>
    public System.Collections.Specialized.NameValueCollection Headers => _request.Headers;

    /// <summary>Gets the query string</summary>
    public System.Collections.Specialized.NameValueCollection QueryString => _request.QueryString;

    /// <summary>Gets the content type</summary>
    public string? ContentType => _request.ContentType;

    /// <summary>Gets the content length</summary>
    public long ContentLength => _request.ContentLength64;

    /// <summary>Gets the input stream</summary>
    public Stream InputStream => _request.InputStream;

    /// <summary>Initializes a new instance of HttpCwsRequest</summary>
    /// <param name="request">Underlying HttpListenerRequest</param>
    public HttpCwsRequest(HttpListenerRequest request)
    {
      _request = request;
    }

    /// <summary>Gets the request body as a string</summary>
    /// <returns>Request body content</returns>
    public string GetRequestBodyAsString()
    {
      using var reader = new StreamReader(InputStream, Encoding.UTF8);
      return reader.ReadToEnd();
    }
  }

  /// <summary>Mock HttpCwsResponse class representing an HTTP response</summary>
  public class HttpCwsResponse
  {
    private readonly HttpListenerResponse _response;

    /// <summary>Gets or sets the status code</summary>
    public int StatusCode
    {
      get => _response.StatusCode;
      set => _response.StatusCode = value;
    }

    /// <summary>Gets or sets the content type</summary>
    public string? ContentType
    {
      get => _response.ContentType;
      set => _response.ContentType = value;
    }

    /// <summary>Gets or sets the content length</summary>
    public long ContentLength
    {
      get => _response.ContentLength64;
      set => _response.ContentLength64 = value;
    }

    /// <summary>Gets the response headers</summary>
    public WebHeaderCollection Headers => _response.Headers;

    /// <summary>Gets the output stream</summary>
    public Stream OutputStream => _response.OutputStream;

    /// <summary>Initializes a new instance of HttpCwsResponse</summary>
    /// <param name="response">Underlying HttpListenerResponse</param>
    public HttpCwsResponse(HttpListenerResponse response)
    {
      _response = response;
    }

    /// <summary>Writes a string to the response</summary>
    /// <param name="content">Content to write</param>
    public void Write(string content)
    {
      var buffer = Encoding.UTF8.GetBytes(content);
      ContentLength = buffer.Length;
      OutputStream.Write(buffer, 0, buffer.Length);
    }

    /// <summary>Writes bytes to the response</summary>
    /// <param name="buffer">Buffer to write</param>
    /// <param name="offset">Offset in buffer</param>
    /// <param name="count">Number of bytes to write</param>
    public void Write(byte[] buffer, int offset, int count)
    {
      OutputStream.Write(buffer, offset, count);
    }
  }

  /// <summary>Interface for HTTP request handlers</summary>
  public interface IHttpCwsHandler
  {
    /// <summary>Processes an HTTP request</summary>
    /// <param name="context">HTTP context</param>
    void ProcessRequest(HttpCwsContext context);
  }

  /// <summary>Mock HttpCwsRoute class for route management</summary>
  public class HttpCwsRoute
  {
    /// <summary>Gets or sets the route path</summary>
    public string Path { get; set; } = string.Empty;

    /// <summary>Gets or sets the HTTP method</summary>
    public string Method { get; set; } = "GET";

    /// <summary>Gets or sets the route handler</summary>
    public IHttpCwsHandler? Handler { get; set; }

    /// <summary>Initializes a new instance of HttpCwsRoute</summary>
    public HttpCwsRoute()
    {
    }

    /// <summary>Initializes a new instance of HttpCwsRoute</summary>
    /// <param name="path">Route path</param>
    /// <param name="handler">Route handler</param>
    public HttpCwsRoute(string path, IHttpCwsHandler handler)
    {
      Path = path;
      Handler = handler;
    }

    /// <summary>Initializes a new instance of HttpCwsRoute</summary>
    /// <param name="path">Route path</param>
    /// <param name="method">HTTP method</param>
    /// <param name="handler">Route handler</param>
    public HttpCwsRoute(string path, string method, IHttpCwsHandler handler)
    {
      Path = path;
      Method = method;
      Handler = handler;
    }
  }

  /// <summary>Mock HTTP CWS route collection</summary>
  public class HttpCwsRouteCollection
  {
    private readonly List<HttpCwsRoute> _routes = new List<HttpCwsRoute>();

    /// <summary>Adds a route</summary>
    /// <param name="route">Route to add</param>
    public void Add(HttpCwsRoute route)
    {
      _routes.Add(route);
    }

    /// <summary>Removes a route</summary>
    /// <param name="route">Route to remove</param>
    public void Remove(HttpCwsRoute route)
    {
      _routes.Remove(route);
    }

    /// <summary>Clears all routes</summary>
    public void Clear()
    {
      _routes.Clear();
    }

    /// <summary>Gets route count</summary>
    public int Count => _routes.Count;
  }

  /// <summary>Mock HTTP CWS request event args</summary>
  public class HttpCwsRequestEventArgs : EventArgs
  {
    /// <summary>Gets the HTTP context</summary>
    public HttpCwsContext Context { get; private set; }

    /// <summary>Initializes new instance</summary>
    /// <param name="context">HTTP context</param>
    public HttpCwsRequestEventArgs(HttpCwsContext context)
    {
      Context = context;
    }
  }
}
