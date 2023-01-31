using System;
using System.Collections.Generic;
using System.Linq;
using Crestron.SimplSharp;
using Crestron.SimplSharp.WebScripting;
using Crestron.SimplSharpPro.Diagnostics;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using PepperDash.Core;
using PepperDash.Core.Web;
using PepperDash.Essentials.Core.Web.RequestHandlers;

namespace PepperDash.Essentials.Core.Web
{
	public class EssemtialsWebApi : EssentialsDevice
	{
		private readonly WebApiServer _server;

		///<example>
		/// http(s)://{ipaddress}/cws/{basePath}
		/// http(s)://{ipaddress}/VirtualControl/Rooms/{roomId}/cws/{basePath}
		/// </example>
		private readonly string _defaultBasePath =
			CrestronEnvironment.DevicePlatform == eDevicePlatform.Appliance ? string.Format("/app{0:00}/api", InitialParametersClass.ApplicationNumber) : "/api";

		// TODO [ ] Reset debug levels to proper value Trace = 0, Info = 1, Verbose = 2
		private const int DebugTrace = 0;
		private const int DebugInfo = 0;
		private const int DebugVerbose = 0;

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
		public EssemtialsWebApi(string key, string name)
			: this(key, name, null)
		{
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

			if (config == null)
				BasePath = _defaultBasePath;
			else
				BasePath = string.IsNullOrEmpty(config.BasePath) ? _defaultBasePath : config.BasePath;

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
				new HttpCwsRoute("devprops")
				{
					Name = "DevProps",
					RouteHandler = new DevPropsRequestHandler()
				},
				//new HttpCwsRoute("devprops/{key}")
				//{
				//    Name = "DevProps",
				//    RouteHandler = new DevPropsRequestHandler()
				//},
				new HttpCwsRoute("devjson")
				{
					Name = "DevJson",
					RouteHandler = new DevJsonRequestHandler()
				},
				new HttpCwsRoute("setdevicestreamdebug")
				{
				    Name = "SetDeviceStreamDebug",
				    RouteHandler = new SetDeviceStreamDebugRequestHandler()
				},
				//new HttpCwsRoute("setdevicestreamdebug/{deviceKey}/{state}")
				//{
				//    Name = "SetDeviceStreamDebug",
				//    RouteHandler = new SetDeviceStreamDebugRequestHandler()
				//},
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

			foreach (var route in routes.Where(route => route != null))
			{
				var r = route;
				_server.AddRoute(r);
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
				Debug.Console(DebugTrace, Debug.ErrorLogLevel.Notice, "Starting Essentials Web API on {0} Appliance", is4Series ? "4-series" : "3-series");

				_server.Start();

				GetPaths();

				return;
			}

			// Automatically start CWS when running on a server (Linux OS, Virtual Control)
			Debug.Console(DebugTrace, Debug.ErrorLogLevel.Notice, "Starting Essentials Web API on Virtual Control Server");

			_server.Start();
		}

		/// <summary>
		/// Print the available pahts
		/// </summary>
		/// <example>
		/// http(s)://{ipaddress}/cws/{basePath}
		/// http(s)://{ipaddress}/VirtualControl/Rooms/{roomId}/cws/{basePath}
		/// </example>
		public void GetPaths()
		{
			Debug.Console(DebugTrace, this, "{0}", new String('-', 50));

			var currentIp = CrestronEthernetHelper.GetEthernetParameter(
				CrestronEthernetHelper.ETHERNET_PARAMETER_TO_GET.GET_CURRENT_IP_ADDRESS, 0);
			
			var hostname = CrestronEthernetHelper.GetEthernetParameter(
                    CrestronEthernetHelper.ETHERNET_PARAMETER_TO_GET.GET_HOSTNAME, 0);
			
			var path = CrestronEnvironment.DevicePlatform == eDevicePlatform.Server 
				? string.Format("http(s)://{0}/virtualcontrol/rooms/{1}/cws{2}", hostname, InitialParametersClass.RoomId, BasePath)
				: string.Format("http(s)://{0}/cws{1}", currentIp, BasePath);
			
			Debug.Console(DebugTrace, this, "Server:{0}", path);

			var routeCollection = _server.GetRouteCollection();
			if (routeCollection == null)
			{
				Debug.Console(DebugTrace, this, "Server route collection is null");
				return;
			}
			Debug.Console(DebugTrace, this, "Configured Routes:");
			foreach (var route in routeCollection)
			{
				Debug.Console(DebugTrace, this, "{0}: {1}/{2}", route.Name, path, route.Url);
			}
			Debug.Console(DebugTrace, this, "{0}", new String('-', 50));
		}
	}
}