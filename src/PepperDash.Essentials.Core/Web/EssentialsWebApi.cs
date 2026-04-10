using System;
using System.Collections.Generic;
using System.Linq;
using Crestron.SimplSharp;
using Crestron.SimplSharp.WebScripting;
using PepperDash.Core;
using PepperDash.Core.Web;
using PepperDash.Essentials.Core.Web.RequestHandlers;
using Serilog.Events;

namespace PepperDash.Essentials.Core.Web
{
    /// <summary>
    /// Represents a EssentialsWebApi
    /// </summary>
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
        /// Gets or sets the BasePath
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
                new HttpCwsRoute("deviceCommands/{deviceKey}")
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
                },
                new HttpCwsRoute("doNotLoadConfigOnNextBoot")
                {
                    Name = "DoNotLoadConfigOnNextBoot",
                    RouteHandler = new DoNotLoadConfigOnNextBootRequestHandler()
                },
                new HttpCwsRoute("restartProgram")
                {
                    Name = "Restart Program",
                    RouteHandler = new RestartProgramRequestHandler()
                },
                new HttpCwsRoute("loadConfig")
                {
                    Name = "Load Config",
                    RouteHandler = new LoadConfigRequestHandler()
                },
                new HttpCwsRoute("tielines")
                {
                    Name = "Get TieLines",
                    RouteHandler = new GetTieLinesRequestHandler()
                },
                new HttpCwsRoute("device/{deviceKey}/routingPorts")
                {
                    Name = "Get Routing Ports for a device",
                    RouteHandler = new GetRoutingPortsHandler()
                },
                new HttpCwsRoute("routingDevicesAndTieLines")
                {
                    Name = "Get Routing Devices and TieLines",
                    RouteHandler = new GetRoutingDevicesAndTieLinesHandler()
                },
            };

            AddRoute(routes);
        }

        /// <summary>
        /// Add a single route to the API. MUST be done during the activation phase
        /// </summary>
        /// <param name="route"></param>
        /// <summary>
        /// AddRoute method
        /// </summary>
        public void AddRoute(HttpCwsRoute route)
        {
            _server.AddRoute(route);
        }

        /// <summary>
        /// Add a collection of routes to the API. MUST be done during the activation phase
        /// </summary>
        /// <param name="routes"></param>
        public void AddRoute(List<HttpCwsRoute> routes)
        {
            foreach (var route in routes)
            {
                AddRoute(route);
            }
        }

        /// <summary>
        /// Initialize method
        /// </summary>
        /// <inheritdoc />
        public override void Initialize()
        {
            AddRoute(new HttpCwsRoute("apiPaths")
            {
                Name = "GetPaths",
                RouteHandler = new GetRoutesHandler(_server.GetRouteCollection(), BasePath)
            });

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
                Debug.LogMessage(LogEventLevel.Verbose, "Starting Essentials Web API on {0} Appliance", is4Series ? "4-series" : "3-series");

                _server.Start();

                GetPaths();

                return;
            }

            // Automatically start CWS when running on a server (Linux OS, Virtual Control)
            Debug.LogMessage(LogEventLevel.Verbose, "Starting Essentials Web API on Virtual Control Server");

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
        /// <summary>
        /// GetPaths method
        /// </summary>
        public void GetPaths()
        {
            Debug.LogMessage(LogEventLevel.Information, this, new string('-', 50));

            var currentIp = CrestronEthernetHelper.GetEthernetParameter(
                CrestronEthernetHelper.ETHERNET_PARAMETER_TO_GET.GET_CURRENT_IP_ADDRESS, 0);

            var hostname = CrestronEthernetHelper.GetEthernetParameter(
                    CrestronEthernetHelper.ETHERNET_PARAMETER_TO_GET.GET_HOSTNAME, 0);

            var path = CrestronEnvironment.DevicePlatform == eDevicePlatform.Server
                ? $"https://{hostname}/VirtualControl/Rooms/{InitialParametersClass.RoomId}/cws{BasePath}"
                : $"https://{currentIp}/cws{BasePath}";

            Debug.LogMessage(LogEventLevel.Information, this, "Server:{path:l}", path);

            var routeCollection = _server.GetRouteCollection();
            if (routeCollection == null)
            {
                Debug.LogMessage(LogEventLevel.Information, this, "Server route collection is null");
                return;
            }
            Debug.LogMessage(LogEventLevel.Information, this, "Configured Routes:");
            foreach (var route in routeCollection)
            {
                Debug.LogMessage(LogEventLevel.Information, this, "{routeName:l}: {routePath:l}/{routeUrl:l}", route.Name, path, route.Url);
            }
            Debug.LogMessage(LogEventLevel.Information, this, new string('-', 50));
        }
    }
}