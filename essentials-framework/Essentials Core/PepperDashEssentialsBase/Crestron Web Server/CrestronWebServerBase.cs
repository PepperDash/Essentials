using System;
using Crestron.SimplSharp;
using Crestron.SimplSharp.WebScripting;
using PepperDash.Core;

namespace PepperDash.Essentials.Core
{
	public class CrestronWebServerBase : EssentialsDevice, IDisposable
	{
		private HttpCwsServer _server;
		private readonly CCriticalSection _serverLock = new CCriticalSection();

		/// <summary>
		/// CWS base path
		/// </summary>
		public string BasePath { get; private set; }

		/// <summary>
		/// Constructor
		/// </summary>
		/// <param name="key"></param>
		/// <param name="name"></param>
		/// <param name="basePath"></param>
		public CrestronWebServerBase(string key, string name, string basePath)
			: base(key, name)
		{
			Key = key;

			BasePath = string.IsNullOrEmpty(basePath) ? "/api" : basePath;

			CrestronEnvironment.ProgramStatusEventHandler += programEvent =>
			{
				if (programEvent != eProgramStatusEventType.Stopping)
					return;

				Dispose(true);
			};
		}

		/// <summary>
		/// Initializes the CWS class
		/// </summary>
		public override void Initialize()
		{
			ServerStart();
			base.Initialize();
		}

		/// <summary>
		/// Starts the CWS server
		/// </summary>
		public void ServerStart()
		{
			try
			{
				_serverLock.Enter();

				if (_server != null)
				{
					Debug.Console(1, this, "Server is already running");
					return;
				}

				Debug.Console(1, this, "Starting server");

				_server = new HttpCwsServer(BasePath)
				{
					HttpRequestHandler = new RequestHandlerUnknown()
				};

				// TODO [ ] Add server paths 
			}
			catch (Exception ex)
			{
				Debug.Console(1, this, "ServerStart Exception Message: {0}", ex.Message);
				Debug.Console(2, this, "ServerStart Exception StackTrace: {0}", ex.StackTrace);
				if (ex.InnerException != null)
					Debug.Console(2, this, "ServerStart Exception InnerException: {0}", ex.InnerException);
			}
			finally
			{
				_serverLock.Leave();
			}
		}

		/// <summary>
		/// Stops the CWS server
		/// </summary>
		public void ServerStop()
		{
			try
			{
				_serverLock.Enter();
				if (_server == null)
				{
					Debug.Console(1, this, "Server is already stopped");
					return;
				}

				_server.Unregister();
				_server.Dispose();
				_server = null;
			}
			catch (Exception ex)
			{
				Debug.Console(1, this, "ServerStop Exception Message: {0}", ex.Message);
				Debug.Console(2, this, "ServerStop Exception StackTrace: {0}", ex.StackTrace);
				if (ex.InnerException != null)
					Debug.Console(2, this, "ServerStop Exception InnerException: {0}", ex.InnerException);
			}
			finally
			{
				_serverLock.Leave();
			}
		}

		/// <summary>
		/// Received request handler
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="args"></param>
		public void ReceivedRequestEventHandler(object sender, HttpCwsRequestEventArgs args)
		{
			try
			{
				// TODO [ ] Add logic for received requests
				Debug.Console(1, this, @"RecieveRequestEventHandler 
Method: {0}
Path: {1}
PathInfo: {2}
PhysicalPath: {3}
ContentType: {4}
RawUrl: {5}
Url: {6}
UserAgent: {7}
UserHostAddress: {8}
UserHostName: {9}",
	args.Context.Request.HttpMethod,
	args.Context.Request.Path,
	args.Context.Request.PathInfo,
	args.Context.Request.PhysicalPath,
	args.Context.Request.ContentType,
	args.Context.Request.RawUrl,
	args.Context.Request.Url,
	args.Context.Request.UserAgent,
	args.Context.Request.UserHostAddress,
	args.Context.Request.UserHostName);

			}
			catch (Exception ex)
			{
				Debug.Console(1, this, "ReceivedRequestEventHandler Exception Message: {0}", ex.Message);
				Debug.Console(2, this, "ReceivedRequestEventHandler Exception StackTrace: {0}", ex.StackTrace);
				if (ex.InnerException != null)
					Debug.Console(2, this, "ReceivedRequestEventHandler Exception InnerException: {0}", ex.InnerException);
			}
		}

		#region Dispose

		/// <summary>
		/// Tracks if the CWS is disposed
		/// </summary>
		public bool Disposed { get; private set; }

		/// <summary>
		/// Disposes of the CWS
		/// </summary>
		public void Dispose()
		{
			Dispose(true);
			CrestronEnvironment.GC.SuppressFinalize(this);
		}

		protected void Dispose(bool disposing)
		{
			if (Disposed)
			{
				Debug.Console(1, this, "Server has already been disposed");
				return;
			}

			if (!disposing) return;

			if (_server != null) ServerStop();

			Disposed = _server == null;
		}

		~CrestronWebServerBase()
		{
			Dispose(true);
		}

		#endregion
	}
}