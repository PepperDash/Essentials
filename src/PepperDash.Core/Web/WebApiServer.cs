using System;
using System.Collections.Generic;
using System.Linq;
using Crestron.SimplSharp;
using Crestron.SimplSharp.WebScripting;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using PepperDash.Core.Web.RequestHandlers;

namespace PepperDash.Core.Web
{
	/// <summary>
	/// Web API server
	/// </summary>
	public class WebApiServer : IKeyName
	{
		private const string SplusKey = "Uninitialized Web API Server";
		private const string DefaultName = "Web API Server";
		private const string DefaultBasePath = "/api";

		private const uint DebugTrace = 0;
		private const uint DebugInfo = 1;
		private const uint DebugVerbose = 2;

		private readonly CCriticalSection _serverLock = new CCriticalSection();
		private HttpCwsServer _server;

		/// <summary>
		/// Web API server key
		/// </summary>
		public string Key { get; private set; }

		/// <summary>
		/// Web API server name
		/// </summary>
		public string Name { get; private set; }

		/// <summary>
		/// CWS base path, will default to "/api" if not set via initialize method
		/// </summary>
		public string BasePath { get; private set; }

		/// <summary>
		/// Indicates CWS is registered with base path
		/// </summary>
		public bool IsRegistered { get; private set; }

		/// <summary>
		/// Http request handler
		/// </summary>
		//public IHttpCwsHandler HttpRequestHandler
		//{
		//    get { return _server.HttpRequestHandler; }
		//    set
		//    {
		//        if (_server == null) return;
		//        _server.HttpRequestHandler = value;
		//    }
		//}

		/// <summary>
		/// Received request event handler
		/// </summary>
		//public event EventHandler<HttpCwsRequestEventArgs> ReceivedRequestEvent
		//{
		//    add { _server.ReceivedRequestEvent += new HttpCwsRequestEventHandler(value); }
		//    remove { _server.ReceivedRequestEvent -= new HttpCwsRequestEventHandler(value); }
		//}

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

			if (_server == null) _server = new HttpCwsServer(BasePath);

			_server.setProcessName(Key);
			_server.HttpRequestHandler = new DefaultRequestHandler();

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

			Debug.Console(DebugInfo, this, "Program stopping. stopping server");

			Stop();
		}

		/// <summary>
		/// Ethernet event handler
		/// </summary>
		/// <param name="ethernetEventArgs"></param>
		void CrestronEnvironment_EthernetEventHandler(EthernetEventArgs ethernetEventArgs)
		{
			// Re-enable the server if the link comes back up and the status should be connected
			if (ethernetEventArgs.EthernetEventType == eEthernetEventType.LinkUp && IsRegistered)
			{
				Debug.Console(DebugInfo, this, "Ethernet link up. Server is alreedy registered.");
				return;
			}

			Debug.Console(DebugInfo, this, "Ethernet link up. Starting server");

			Start();
		}

		/// <summary>
		/// Initializes CWS class
		/// </summary>
		public void Initialize(string key, string basePath)
		{
			Key = key;
			BasePath = string.IsNullOrEmpty(basePath) ? DefaultBasePath : basePath;
		}

		/// <summary>
		/// Adds a route to CWS
		/// </summary>
		public void AddRoute(HttpCwsRoute route)
		{
			if (route == null)
			{
				Debug.Console(DebugInfo, this, "Failed to add route, route parameter is null");
				return;
			}

			_server.Routes.Add(route);

		}

		/// <summary>
		/// Removes a route from CWS
		/// </summary>
		/// <param name="route"></param>
		public void RemoveRoute(HttpCwsRoute route)
		{
			if (route == null)
			{
				Debug.Console(DebugInfo, this, "Failed to remote route, orute parameter is null");
				return;
			}

			_server.Routes.Remove(route);
		}

		/// <summary>
		/// Returns a list of the current routes
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
			try
			{
				_serverLock.Enter();

				if (_server == null)
				{
					Debug.Console(DebugInfo, this, "Server is null, unable to start");
					return;
				}

				if (IsRegistered)
				{
					Debug.Console(DebugInfo, this, "Server has already been started");
					return;
				}

				IsRegistered = _server.Register();

				Debug.Console(DebugInfo, this, "Starting server, registration {0}", IsRegistered ? "was successful" : "failed");
			}
			catch (Exception ex)
			{
				Debug.Console(DebugInfo, this, "Start Exception Message: {0}", ex.Message);
				Debug.Console(DebugVerbose, this, "Start Exception StackTrace: {0}", ex.StackTrace);
				if (ex.InnerException != null)
					Debug.Console(DebugVerbose, this, "Start Exception InnerException: {0}", ex.InnerException);
			}
			finally
			{
				_serverLock.Leave();
			}
		}

		/// <summary>
		/// Stop CWS instance
		/// </summary>
		public void Stop()
		{
			try
			{
				_serverLock.Enter();

				if (_server == null)
				{
					Debug.Console(DebugInfo, this, "Server is null or has already been stopped");
					return;
				}

				IsRegistered = _server.Unregister() == false;

				Debug.Console(DebugInfo, this, "Stopping server, unregistration {0}", IsRegistered ? "failed" : "was successful");

				_server.Dispose();
				_server = null;
			}
			catch (Exception ex)
			{
				Debug.Console(DebugInfo, this, "Server Stop Exception Message: {0}", ex.Message);
				Debug.Console(DebugVerbose, this, "Server Stop Exception StackTrace: {0}", ex.StackTrace);
				if (ex.InnerException != null)
					Debug.Console(DebugVerbose, this, "Server Stop Exception InnerException: {0}", ex.InnerException);
			}
			finally
			{
				_serverLock.Leave();
			}
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
				var j = JsonConvert.SerializeObject(args.Context, Formatting.Indented);
				Debug.Console(DebugVerbose, this, "RecieveRequestEventHandler Context:\x0d\x0a{0}", j);
			}
			catch (Exception ex)
			{
				Debug.Console(DebugInfo, this, "ReceivedRequestEventHandler Exception Message: {0}", ex.Message);
				Debug.Console(DebugVerbose, this, "ReceivedRequestEventHandler Exception StackTrace: {0}", ex.StackTrace);
				if (ex.InnerException != null)
					Debug.Console(DebugVerbose, this, "ReceivedRequestEventHandler Exception InnerException: {0}", ex.InnerException);
			}
		}
	}
}