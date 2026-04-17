using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using Crestron.SimplSharp;
using Crestron.SimplSharp.WebScripting;
using PepperDash.Core.Web.RequestHandlers;
using PepperDash.Core.Logging;

namespace PepperDash.Core.Web;

/// <summary>
/// Web API server
/// </summary>
public class WebApiServer : IKeyName
{
	private const string SplusKey = "Uninitialized Web API Server";
	private const string DefaultName = "Web API Server";
	private const string DefaultBasePath = "/api";

	private readonly object _serverLock = new();
	private HttpCwsServer _server;

	/// <summary>
	/// Gets or sets the Key
	/// </summary>
	public string Key { get; private set; }

	/// <summary>
	/// Gets or sets the Name
	/// </summary>
	public string Name { get; private set; }

	/// <summary>
	/// Gets or sets the BasePath
	/// </summary>
	public string BasePath { get; private set; }

	/// <summary>
	/// Gets or sets the IsRegistered
	/// </summary>
	public bool IsRegistered { get; private set; }

	/// <summary>
	/// Constructor for S+.  Make sure to set necessary properties using init method
	/// </summary>
	public WebApiServer()
		: this(SplusKey, DefaultName, null)
	{
	}

	/// <summary>
	/// Constructor
	/// </summary>
	/// <param name="key"></param>
	/// <param name="basePath"></param>
	public WebApiServer(string key, string basePath)
		: this(key, DefaultName, basePath)
	{
	}

	/// <summary>
	/// Constructor
	/// </summary>
	/// <param name="key"></param>
	/// <param name="name"></param>
	/// <param name="basePath"></param>
	public WebApiServer(string key, string name, string basePath)
	{
		Key = key;
		Name = string.IsNullOrEmpty(name) ? DefaultName : name;
		BasePath = string.IsNullOrEmpty(basePath) ? DefaultBasePath : basePath;

		this.LogInformation("Creating Web API Server with Key: {Key}, Name: {Name}, BasePath: {BasePath}", Key, Name, BasePath);

		if (_server == null) _server = new HttpCwsServer(BasePath);

		 _server.AuthenticateAllRoutes = false;

		_server.setProcessName(Key);
		_server.HttpRequestHandler = new DefaultRequestHandler();
		_server.ReceivedRequestEvent += ReceivedRequestEventHandler;

		CrestronEnvironment.ProgramStatusEventHandler += CrestronEnvironment_ProgramStatusEventHandler;
		CrestronEnvironment.EthernetEventHandler += CrestronEnvironment_EthernetEventHandler;
	}

	/// <summary>
	/// Program status event handler
	/// </summary>
	/// <param name="programEventType"></param>
	void CrestronEnvironment_ProgramStatusEventHandler(eProgramStatusEventType programEventType)
	{
		if (programEventType != eProgramStatusEventType.Stopping) return;

		this.LogInformation("Program stopping. stopping server");

		Stop();
	}

	/// <summary>
	/// Ethernet event handler
	/// </summary>
	/// <param name="ethernetEventArgs"></param>
	void CrestronEnvironment_EthernetEventHandler(EthernetEventArgs ethernetEventArgs)
	{
		if (ethernetEventArgs.EthernetEventType != eEthernetEventType.LinkUp)
		{
			return;
		}

		if (IsRegistered)
		{
			this.LogInformation("Ethernet link up. Server is already registered.");
			return;
		}

		this.LogInformation("Ethernet link up. Starting server");

		Start();
	}

	// /// <summary>
	// /// Initialize method
	// /// </summary>
	// public void Initialize(string key, string basePath)
	// {
	// 	Key = key;
	// 	BasePath = string.IsNullOrEmpty(basePath) ? DefaultBasePath : basePath;
	// }

	/// <summary>
	/// Adds a route to CWS
	/// </summary>
	public void AddRoute(HttpCwsRoute route)
	{
		if (route == null)
		{
			this.LogWarning("Failed to add route, route parameter is null");
			return;
		}

		_server.Routes.Add(route);

	}

	/// <summary>
	/// Removes a route from CWS
	/// </summary>
	/// <param name="route"></param>
	/// <summary>
	/// RemoveRoute method
	/// </summary>
	public void RemoveRoute(HttpCwsRoute route)
	{
		if (route == null)
		{
			this.LogWarning("Failed to remove route, route parameter is null");
			return;
		}

		_server.Routes.Remove(route);
	}

	/// <summary>
	/// Sets the fallback request handler that is invoked when no registered route
	/// matches an incoming request.  Must be called before <see cref="Start"/>.
	/// </summary>
	/// <param name="handler">The handler to use as the server-level fallback.</param>
	public void SetFallbackHandler(IHttpCwsHandler handler)
	{
		if (handler == null)
		{
			this.LogWarning("SetFallbackHandler: handler parameter is null, ignoring");
			return;
		}

		_server.HttpRequestHandler = handler;
	}

	/// <summary>
	/// GetRouteCollection method
	/// </summary>
	public HttpCwsRouteCollection GetRouteCollection()
	{
		return _server.Routes;
	}

	/// <summary>
	/// Starts CWS instance
	/// </summary>
	public void Start()
	{
		lock (_serverLock)
		{
			try
			{
				if (_server == null)
				{
					this.LogDebug("Server is null, unable to start");
					return;
				}

				if (IsRegistered)
				{
					this.LogDebug("Server has already been started");
					return;
				}

				IsRegistered = _server.Register();

				this.LogDebug("Starting server, registration {0}", IsRegistered ? "was successful" : "failed");
			}
			catch (Exception ex)
			{
				this.LogException(ex, "Start Exception Message: {0}", ex.Message);
				this.LogVerbose("Start Exception StackTrace: {0}", ex.StackTrace);
			}
		} // end lock
	}

	/// <summary>
	/// Stop method
	/// </summary>
	public void Stop()
	{
		lock (_serverLock)
		{
			try
			{
				if (_server == null)
				{
					this.LogDebug("Server is null or has already been stopped");
					return;
				}

				var unregistered = _server.Unregister();
				IsRegistered = !unregistered;

				this.LogDebug("Stopping server, unregistration {0}", unregistered ? "was successful" : "failed");
			}
			catch (Exception ex)
			{
				this.LogException(ex, "Server Stop Exception Message: {0}", ex.Message);
			}
		} // end lock
	}

	/// <summary>
	/// Received request handler
	/// </summary>
	/// <remarks>
	/// This is here for development and testing
	/// </remarks>
	/// <param name="sender"></param>
	/// <param name="args"></param>
	public void ReceivedRequestEventHandler(object sender, HttpCwsRequestEventArgs args)
	{
		try
		{
			var req = args.Context?.Request;
			this.LogVerbose("ReceivedRequestEventHandler: {Method} {Path}", req?.HttpMethod, req?.Path);
		}
		catch (Exception ex)
		{
			this.LogException(ex, "ReceivedRequestEventHandler Exception Message: {0}", ex.Message);
		}
	}
}