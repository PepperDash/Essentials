using System.Collections.Generic;
using Crestron.SimplSharp;
using Crestron.SimplSharp.WebScripting;
using PepperDash.Core;
using PepperDash.Core.Web;
using PepperDash.Essentials.Core.Web.RequestHandlers;

namespace PepperDash.Essentials.Core.Web
{
	public class EssemtialsWebApi : EssentialsDevice
	{
		private readonly WebApiServer _server;

		private const string DefaultBasePath = "/api";

		private const int DebugTrace = 0;
		private const int DebugInfo = 1;
		private const int DebugVerbose = 2;

		/// <summary>
		/// CWS base path
		/// </summary>
		public string BasePath { get; private set; }

		/// <summary>
		/// Tracks if CWS is registered
		/// </summary>
		public bool IsRegistered
		{
			get { return _server.IsRegistered; }
		}

		/// <summary>
		/// Constructor
		/// </summary>
		/// <param name="key"></param>
		/// <param name="name"></param>
		/// <param name="config"></param>
		public EssemtialsWebApi(string key, string name, EssentialsWebApiPropertiesConfig config)
			: base(key, name)
		{
			Key = key;

			BasePath = string.IsNullOrEmpty(config.BasePath) ? DefaultBasePath : config.BasePath;

			_server = new WebApiServer(Key, Name, BasePath);
		}

		/// <summary>
		/// Custom activate, add routes
		/// </summary>
		/// <returns></returns>
		public override bool CustomActivate()
		{
			var routes = new List<HttpCwsRoute>
			{
				new HttpCwsRoute("reportversions")
				{
					Name = "ReportVersions",
					RouteHandler = new ReportVersionsRequestHandler()
				},
				new HttpCwsRoute("appdebug")
				{
					Name = "AppDebug",
					RouteHandler = new AppDebugRequestHandler()
				},
				new HttpCwsRoute("devlist")
				{
					Name = "DevList",
					RouteHandler = new DevListRequestHandler()
				},
				new HttpCwsRoute("devprops/{key}")
				{
					Name = "DevProps",
					RouteHandler = new DevPropsRequestHandler()
				},
				new HttpCwsRoute("devjson")
				{
					Name = "DevJson",
					RouteHandler = new DevJsonRequestHandler()
				},
				new HttpCwsRoute("setdevicestreamdebug/{deviceKey}/{state}")
				{
					Name = "SetDeviceStreamDebug",
					RouteHandler = new SetDeviceStreamDebugRequestHandler()
				},
				new HttpCwsRoute("disableallstreamdebug")
				{
					Name = "DisableAllStreamDebug",
					RouteHandler = new DisableAllStreamDebugRequestHandler()
				},
				new HttpCwsRoute("showconfig")
				{
					Name = "ShowConfig",
					RouteHandler = new ShowConfigRequestHandler()
				},
				new HttpCwsRoute("gettypes/all")
				{
					Name = "GetTypesAll",
					RouteHandler = new GetTypesRequestHandler()
				},
				new HttpCwsRoute("gettypes/{filter}")
				{
					Name = "GetTypesByFilter",
					RouteHandler = new GetTypesRequestHandler()
				},
				new HttpCwsRoute("getjoinmap/{bridgeKey}/all")
				{
					Name = "GetJoinMapsByBridgeKey",
					RouteHandler = new GetJoinMapRequestHandler()
				},
				new HttpCwsRoute("getjoinmap/{bridgeKey}/{deviceKey}")
				{
					Name = "GetJoinMapsForBridgeKeyFilteredByDeviceKey",
					RouteHandler = new GetJoinMapRequestHandler()
				}
			};

			foreach (var route in routes)
			{
				_server.AddRoute(route);
			}

			return base.CustomActivate();
		}

		/// <summary>
		/// Initializes the CWS class
		/// </summary>
		public override void Initialize()
		{
			// If running on an appliance
			if (CrestronEnvironment.DevicePlatform == eDevicePlatform.Appliance)
			{
				/*
					RMC4>
					WEBSERVER [ON | OFF | TIMEOUT <VALUE IN SECONDS> | MAXSESSIONSPERUSER <Number of sessions>]
					WEBSERVER [TIMEOUT] will display current session timeout value
					WEBSERVER MAXSESSIONSPERUSER will display current max web sessions per user
					WEBSERVER ALLOWSHAREDSESSION will display whether 'samesite = none' would be set on cookies
							No parameter - displays current setting
				*/
				var response = string.Empty;
				CrestronConsole.SendControlSystemCommand("webserver", ref response);
				if (response.Contains("OFF")) return;

				var is4Series = eCrestronSeries.Series4 == (Global.ProcessorSeries & eCrestronSeries.Series4);
				Debug.Console(DebugTrace, Debug.ErrorLogLevel.Notice, "Starting Essentials CWS on {0} Appliance", is4Series ? "4-series" : "3-series");

				_server.Start();

				return;
			}

			// Automatically start CWS when running on a server (Linux OS, Virtual Control)
			Debug.Console(DebugTrace, Debug.ErrorLogLevel.Notice, "Starting Essentials CWS on Virtual Control Server");

			_server.Start();
		}
	}
}