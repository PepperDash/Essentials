using System;
using System.Collections.Generic;
using System.Linq;
using Crestron.SimplSharp;
using Crestron.SimplSharp.WebScripting;
using PepperDash.Core;
using PepperDash.Core.Web;
using PepperDash.Essentials.Core.Web.RequestHandlers;

namespace PepperDash.Essentials.Core.Web
{
	public class EssentialsWebApi : EssentialsDevice
	{
		private readonly WebApiServer _server;

		///<example>
		/// http(s)://{ipaddress}/cws/{basePath}
		/// http(s)://{ipaddress}/VirtualControl/Rooms/{roomId}/cws/{basePath}
		/// </example>
		private readonly string _defaultBasePath = CrestronEnvironment.DevicePlatform == eDevicePlatform.Appliance 
			? string.Format("/app{0:00}/api", InitialParametersClass.ApplicationNumber) 
			: "/api";

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
		public EssentialsWebApi(string key, string name)
			: this(key, name, null)
		{
		}

		/// <summary>
		/// Constructor
		/// </summary>
		/// <param name="key"></param>
		/// <param name="name"></param>
		/// <param name="config"></param>
		public EssentialsWebApi(string key, string name, EssentialsWebApiPropertiesConfig config)
			: base(key, name)
		{
			Key = key;

			if (config == null)
				BasePath = _defaultBasePath;
			else
				BasePath = string.IsNullOrEmpty(config.BasePath) ? _defaultBasePath : config.BasePath;

			_server = new WebApiServer(Key, Name, BasePath);

			SetupRoutes();

			Initialize();
		}

		private void SetupRoutes()
		{
            var routes = new List<HttpCwsRoute>
            {
                new HttpCwsRoute("versions")
                {
                    Name = "ReportVersions",
                    RouteHandler = new ReportVersionsRequestHandler()
                },
                new HttpCwsRoute("appdebug")
                {
                    Name = "AppDebug",
                    RouteHandler = new AppDebugRequestHandler()
                },
                new HttpCwsRoute("devices")
                {
                    Name = "DevList",
                    RouteHandler = new DevListRequestHandler()
                },
                new HttpCwsRoute("deviceCommands")
                {
                    Name = "DevJson",
                    RouteHandler = new DevJsonRequestHandler()
                },
                new HttpCwsRoute("deviceProperties/{deviceKey}")
                {
                    Name = "DevProps",
                    RouteHandler = new DevPropsRequestHandler()
                },
                new HttpCwsRoute("deviceMethods/{deviceKey}")
                {
                    Name = "DevMethods",
                    RouteHandler = new DevMethodsRequestHandler()
                },
                new HttpCwsRoute("deviceFeedbacks/{deviceKey}")
                {
                    Name = "GetFeedbacksForDeviceKey",
                    RouteHandler = new GetFeedbacksForDeviceRequestHandler()
                },
                new HttpCwsRoute("deviceStreamDebug")
                {
                    Name = "SetDeviceStreamDebug",
                    RouteHandler = new SetDeviceStreamDebugRequestHandler()
                },
                new HttpCwsRoute("disableAllStreamDebug")
                {
                    Name = "DisableAllStreamDebug",
                    RouteHandler = new DisableAllStreamDebugRequestHandler()
                },
                new HttpCwsRoute("config")
                {
                    Name = "ShowConfig",
                    RouteHandler = new ShowConfigRequestHandler()
                },
                new HttpCwsRoute("types")
                {
                    Name = "GetTypes",
                    RouteHandler = new GetTypesRequestHandler()
                },
                new HttpCwsRoute("types/{filter}")
                {
                    Name = "GetTypesByFilter",
                    RouteHandler = new GetTypesByFilterRequestHandler()
                },
                new HttpCwsRoute("joinMap/{bridgeKey}")
                {
                    Name = "GetJoinMapsForBridgeKey",
                    RouteHandler = new GetJoinMapForBridgeKeyRequestHandler()
                },
                new HttpCwsRoute("joinMap/{bridgeKey}/{deviceKey}")
                {
                    Name = "GetJoinMapsForDeviceKey",
                    RouteHandler = new GetJoinMapForDeviceKeyRequestHandler()
                },
				new HttpCwsRoute("debugSession")
				{
					Name = "DebugSession",
					RouteHandler = new DebugSessionRequestHandler()
				}

            };

            foreach (var route in routes.Where(route => route != null))
            {
                var r = route;
                _server.AddRoute(r);
            }
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
					WEBSERVER [ON | OFF | TIMEOUT <VALUE IN SECONDS> | MAXSESSIONSPERUSER <Number of sessions>]
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

			GetPaths();
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
				? string.Format("http(s)://{0}/VirtualControl/Rooms/{1}/cws{2}", hostname, InitialParametersClass.RoomId, BasePath)
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