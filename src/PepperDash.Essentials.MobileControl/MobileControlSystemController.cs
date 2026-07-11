using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Timers;
using Crestron.SimplSharp;
using Crestron.SimplSharp.CrestronIO;
using Crestron.SimplSharp.Net.Http;
using Crestron.SimplSharp.WebScripting;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using PepperDash.Core;
using PepperDash.Core.Logging;
using PepperDash.Essentials.AppServer;
using PepperDash.Essentials.AppServer.Messengers;
using PepperDash.Essentials.Core;
using PepperDash.Essentials.Core.Config;
using PepperDash.Essentials.Core.DeviceTypeInterfaces;
using PepperDash.Essentials.Core.Monitoring;
using PepperDash.Essentials.Core.Queues;
using PepperDash.Essentials.Core.Web;
using PepperDash.Essentials.RoomBridges;
using PepperDash.Essentials.Services;
using PepperDash.Essentials.WebApiHandlers;
using PepperDash.Essentials.WebSocketServer;
using WebSocketSharp;

namespace PepperDash.Essentials
{
    /// <summary>
    /// Represents a MobileControlSystemController
    /// </summary>
    public class MobileControlSystemController : EssentialsDevice, IMobileControl
    {
        private bool _initialized = false;
        private const long ServerReconnectInterval = 5000;
        private const long PingInterval = 25000;

        private readonly Dictionary<string, List<IMobileControlAction>> _actionDictionary =
            new Dictionary<string, List<IMobileControlAction>>(
                StringComparer.InvariantCultureIgnoreCase
            );

        /// <summary>
        /// Actions
        /// </summary>
        public ReadOnlyDictionary<string, List<IMobileControlAction>> ActionDictionary => new ReadOnlyDictionary<string, List<IMobileControlAction>>(_actionDictionary);

        private readonly GenericQueue _receiveQueue;
        private readonly List<MobileControlBridgeBase> _roomBridges =
            new List<MobileControlBridgeBase>();

        private readonly Dictionary<string, IMobileControlMessenger> _messengers =
            new Dictionary<string, IMobileControlMessenger>();

        private readonly Dictionary<string, IMobileControlMessenger> _defaultMessengers =
            new Dictionary<string, IMobileControlMessenger>();

        /// <summary>
        /// Get the custom messengers
        /// </summary>
        public ReadOnlyDictionary<string, IMobileControlMessenger> Messengers => new ReadOnlyDictionary<string, IMobileControlMessenger>(_messengers);

        /// <summary>
        /// Get the default messengers
        /// </summary>
        public ReadOnlyDictionary<string, IMobileControlMessenger> DefaultMessengers => new ReadOnlyDictionary<string, IMobileControlMessenger>(_defaultMessengers);

        private readonly GenericQueue _transmitToServerQueue;

        private readonly GenericQueue _transmitToClientsQueue;

        private bool _disableReconnect;
        private WebSocket _wsClient2;

        /// <summary>
        /// Gets or sets the ApiService
        /// </summary>
        public MobileControlApiService ApiService { get; private set; }

        /// <summary>
        /// Get Room Bridges associated with this controller
        /// </summary>
        public List<MobileControlBridgeBase> RoomBridges => _roomBridges;

        private readonly MobileControlWebsocketServer _directServer;

        /// <summary>
        /// Get the Direct Server instance associated with this controller
        /// </summary>
        public MobileControlWebsocketServer DirectServer => _directServer;

        private readonly object _wsCriticalSection = new();

        /// <summary>
        /// Gets or sets the SystemUrl
        /// </summary>
        public string SystemUrl; //set only from SIMPL Bridge!

        /// <summary>
        /// True if the Mobile Control Edge Server Websocket is connected
        /// </summary>
        public bool Connected => _wsClient2 != null && _wsClient2.IsAlive;

        private IEssentialsRoomCombiner _roomCombiner;

        /// <summary>
        /// Gets the SystemUuid from configuration or SIMPL Bridge
        /// </summary>
        public string SystemUuid
        {
            get
            {
                // Check to see if the SystemUuid value is populated. If not populated from configuration, check for value from SIMPL bridge.
                if (
                    !string.IsNullOrEmpty(ConfigReader.ConfigObject.SystemUuid)
                    && ConfigReader.ConfigObject.SystemUuid != "missing url"
                )
                {
                    return ConfigReader.ConfigObject.SystemUuid;
                }

                this.LogWarning(
                    "No system_url value defined in config.  Checking for value from SIMPL Bridge."
                );

                if (string.IsNullOrEmpty(SystemUrl))
                {
                    this.LogError(
                        "No system_url value defined in config or SIMPL Bridge.  Unable to connect to Mobile Control."
                    );
                    return string.Empty;
                }

                var result = Regex.Match(SystemUrl, @"https?:\/\/.*\/systems\/(.*)\/#.*");
                string uuid = result.Groups[1].Value;
                return uuid;
            }
        }

        /// <summary>
        /// Gets or sets the ApiOnlineAndAuthorized
        /// </summary>
        public BoolFeedback ApiOnlineAndAuthorized { get; private set; }

        /// <summary>
        /// Used for tracking HTTP debugging
        /// </summary>
        private bool _httpDebugEnabled;

        private bool _isAuthorized;

        /// <summary>
        /// Tracks if the system is authorized to the API server
        /// </summary>
        public bool IsAuthorized
        {
            get { return _isAuthorized; }
            private set
            {
                if (value == _isAuthorized)
                    return;

                _isAuthorized = value;
                ApiOnlineAndAuthorized.FireUpdate();
            }
        }

        private DateTime _lastAckMessage;

        /// <summary>
        /// Gets the LastAckMessage timestamp
        /// </summary>
        public DateTime LastAckMessage => _lastAckMessage;

        private Timer _pingTimer;

        private Timer _serverReconnectTimer;
        private LogLevel _wsLogLevel = LogLevel.Error;

        /// <summary>
        /// Initializes a new instance of the <see cref="MobileControlSystemController"/> class.
        /// </summary>
        /// <param name="key">The unique key for this controller.</param>
        /// <param name="name">The name of the controller.</param>
        /// <param name="config">The configuration settings for the controller.</param>
        public MobileControlSystemController(string key, string name, MobileControlConfig config)
            : base(key, name)
        {
            Config = config;

            // The queue that will collect the incoming messages in the order they are received
            //_receiveQueue = new ReceiveQueue(key, ParseStreamRx);
            _receiveQueue = new GenericQueue(
                key + "-rxqueue",
                    System.Threading.ThreadPriority.Highest,
                100
            );

            // The queue that will collect the outgoing messages in the order they are received
            _transmitToServerQueue = new GenericQueue(
                key + "-txqueue",
                System.Threading.ThreadPriority.Highest,
                100
            );

            if (Config.DirectServer != null && Config.DirectServer.EnableDirectServer)
            {
                _directServer = new MobileControlWebsocketServer(
                    Key + "-directServer",
                    Config.DirectServer.Port,
                    this
                );
                DeviceManager.AddDevice(_directServer);

                _transmitToClientsQueue = new GenericQueue(
                    key + "-clienttxqueue",
                    System.Threading.ThreadPriority.Highest,
                    100
                );
            }

            Host = config.ServerUrl;
            if (!Host.StartsWith("http"))
            {
                Host = "https://" + Host;
            }

            ApiService = new MobileControlApiService(Host);

            this.LogInformation(
                "Mobile UI controller initializing for server:{0}",
                config.ServerUrl
            );

            if (Global.Platform == eDevicePlatform.Appliance)
            {
                AddConsoleCommands();
            }

            AddPreActivationAction(() => LinkSystemMonitorToAppServer());

            AddPreActivationAction(() =>
            {
                var actionMessenger = new DeviceActionMessenger("deviceActionMessenger-" + Key, "/action");
                AddDeviceMessenger(actionMessenger);
            });

            AddPreActivationAction(() => AddWebApiPaths());

            AddPreActivationAction(() =>
            {
                _roomCombiner = DeviceManager.AllDevices.OfType<IEssentialsRoomCombiner>().FirstOrDefault();

                if (_roomCombiner == null)
                    return;

                _roomCombiner.RoomCombinationScenarioChanged += OnRoomCombinationScenarioChanged;
            });

            CrestronEnvironment.ProgramStatusEventHandler +=
                CrestronEnvironment_ProgramStatusEventHandler;

            ApiOnlineAndAuthorized = new BoolFeedback("apiOnlineAndAuthorized", () =>
            {
                if (_wsClient2 == null)
                    return false;

                return _wsClient2.IsAlive && IsAuthorized;
            });
        }

        /// <inheritdoc />
        public void AddDefaultMessengersForDevice(EssentialsDevice device, IEnumerable<Type> interfaces = null)
        {
            var interfaceSet = interfaces != null ? new HashSet<Type>(interfaces) : [.. device.GetType().GetInterfaces()];
            var messengerAdded = false;

            foreach (var entry in MessengerFactoryRegistry.Entries)
            {
                if (interfaceSet != null && !interfaceSet.Contains(entry.InterfaceType))
                    continue;

                if (!entry.Matches(device))
                    continue;

                var messenger = entry.Factory(device, $"/device/{device.Key}", Key);

                if (messenger == null)
                    continue;

                AddDefaultDeviceMessenger(messenger);
                messengerAdded = true;
            }

            if (!messengerAdded)
            {
                this.LogDebug("Adding GenericMessenger for {deviceKey}", device.Key);
                AddDefaultDeviceMessenger(new GenericMessenger(
                    $"{device.Key}-{Key}-generic",
                    device,
                    $"/device/{device.Key}"));
            }
        }

        private void AddWebApiPaths()
        {
            var apiServer = DeviceManager
                .AllDevices.OfType<EssentialsWebApi>()
                .FirstOrDefault(d => d.Key == "essentialsWebApi");

            if (apiServer == null)
            {
                this.LogWarning("No API Server available");
                return;
            }

            // TODO: Add routes for the rest of the MC console commands
            var routes = new List<HttpCwsRoute>
            {
                new HttpCwsRoute($"device/{Key}/authorize")
                {
                    Name = "MobileControlAuthorize",
                    RouteHandler = new MobileAuthRequestHandler(this)
                },
                new HttpCwsRoute($"device/{Key}/info")
                {
                    Name = "MobileControlInformation",
                    RouteHandler = new MobileInfoHandler(this)
                },
                new HttpCwsRoute($"device/{Key}/actionPaths")
                {
                    Name = "MobileControlActionPaths",
                    RouteHandler = new ActionPathsHandler(this)
                }
            };

            apiServer.AddRoute(routes);
        }

        private void AddConsoleCommands()
        {
            CrestronConsole.AddNewConsoleCommand(
                AuthorizeSystem,
                "mobileauth",
                "Authorizes system to talk to Mobile Control server",
                ConsoleAccessLevelEnum.AccessOperator
            );
            CrestronConsole.AddNewConsoleCommand(
                s => ShowInfo(),
                "mobileinfo",
                "Shows information for current mobile control session",
                ConsoleAccessLevelEnum.AccessOperator
            );
            CrestronConsole.AddNewConsoleCommand(
                s =>
                {
                    s = s.Trim();
                    if (!string.IsNullOrEmpty(s))
                    {
                        _httpDebugEnabled = (s.Trim() != "0");
                    }
                    CrestronConsole.ConsoleCommandResponse(
                        "HTTP Debug {0}",
                        _httpDebugEnabled ? "Enabled" : "Disabled"
                    );
                },
                "mobilehttpdebug",
                "1 enables more verbose HTTP response debugging",
                ConsoleAccessLevelEnum.AccessOperator
            );
            CrestronConsole.AddNewConsoleCommand(
                TestHttpRequest,
                "mobilehttprequest",
                "Tests an HTTP get to URL given",
                ConsoleAccessLevelEnum.AccessOperator
            );

            CrestronConsole.AddNewConsoleCommand(
                PrintActionDictionaryPaths,
                "mobileshowactionpaths",
                "Prints the paths in the Action Dictionary",
                ConsoleAccessLevelEnum.AccessOperator
            );
            CrestronConsole.AddNewConsoleCommand(
                s =>
                {
                    _disableReconnect = false;

                    CrestronConsole.ConsoleCommandResponse(
                        $"Connecting to MC API server"
                    );

                    ConnectWebsocketClient();
                },
                "mobileconnect",
                "Forces connect of websocket",
                ConsoleAccessLevelEnum.AccessOperator
            );

            CrestronConsole.AddNewConsoleCommand(
                s =>
                {
                    _disableReconnect = true;

                    CleanUpWebsocketClient();

                    CrestronConsole.ConsoleCommandResponse(
                        $"Disonnected from MC API server"
                    );
                },
                "mobiledisco",
                "Disconnects websocket",
                ConsoleAccessLevelEnum.AccessOperator
            );

            CrestronConsole.AddNewConsoleCommand(
                ParseStreamRx,
                "mobilesimulateaction",
                "Simulates a message from the server",
                ConsoleAccessLevelEnum.AccessOperator
            );

            CrestronConsole.AddNewConsoleCommand(
                SetWebsocketDebugLevel,
                "mobilewsdebug",
                "Set Websocket debug level",
                ConsoleAccessLevelEnum.AccessProgrammer
            );
        }

        /// <summary>
        /// Gets or sets the Config
        /// </summary>
        public MobileControlConfig Config { get; private set; }

        /// <summary>
        /// Gets or sets the Host
        /// </summary>
        public string Host { get; private set; }

        /// <summary>
        /// Gets the configured Client App URL
        /// </summary>
        public string ClientAppUrl => Config.ClientAppUrl;

        private void OnRoomCombinationScenarioChanged(
            object sender,
            EventArgs eventArgs
        )
        {
            SendMessageObject(new MobileControlMessage { Type = "/system/roomCombinationChanged" });
        }

        /// <summary>
        /// Checks if a device messenger exists for the given key.
        /// </summary>
        public bool CheckForDeviceMessenger(string key)
        {
            return _messengers.ContainsKey(key);
        }

        /// <summary>
        /// Add the provided messenger to the messengers collection
        /// </summary>
        public void AddDeviceMessenger(IMobileControlMessenger messenger)
        {
            if (_messengers.ContainsKey(messenger.Key))
            {
                this.LogWarning("Messenger with key {messengerKey} already added", messenger.Key);
                return;
            }

            if (messenger is IDelayedConfiguration simplMessenger)
            {
                simplMessenger.ConfigurationIsReady += Bridge_ConfigurationIsReady;
            }

            if (messenger is MobileControlBridgeBase roomBridge)
            {
                _roomBridges.Add(roomBridge);
            }

            this.LogVerbose(
                "Adding messenger with key {messengerKey} for path {messengerPath}",
                messenger.Key,
                messenger.MessagePath
            );

            _messengers.Add(messenger.Key, messenger);

            if (_initialized)
            {
                RegisterMessengerWithServer(messenger);
            }
        }

        private void AddDefaultDeviceMessenger(IMobileControlMessenger messenger)
        {
            if (_defaultMessengers.ContainsKey(messenger.Key))
            {
                this.LogWarning(
                    "Default messenger with key {messengerKey} already added",
                    messenger.Key
                );
                return;
            }

            if (messenger is IDelayedConfiguration simplMessenger)
            {
                simplMessenger.ConfigurationIsReady += Bridge_ConfigurationIsReady;
            }
            this.LogVerbose(
                "Adding default messenger with key {messengerKey} for path {messengerPath}",
                messenger.Key,
                messenger.MessagePath
            );

            _defaultMessengers.Add(messenger.Key, messenger);

            if (_initialized)
            {
                RegisterMessengerWithServer(messenger);
            }
        }

        private void RegisterMessengerWithServer(IMobileControlMessenger messenger)
        {
            this.LogVerbose(
                "Registering messenger with key {messengerKey} for path {messengerPath}",
                messenger.Key,
                messenger.MessagePath
            );

            messenger.RegisterWithAppServer(this);
        }

        /// <inheritdoc />
        protected override void Initialize()
        {
            foreach (var messenger in _messengers)
            {
                try
                {
                    RegisterMessengerWithServer(messenger.Value);
                }
                catch (Exception ex)
                {
                    this.LogException(ex, "Exception registering custom messenger {messengerKey}", messenger.Key);
                    continue;
                }
            }

            foreach (var messenger in _defaultMessengers)
            {
                try
                {
                    RegisterMessengerWithServer(messenger.Value);
                }
                catch (Exception ex)
                {
                    this.LogException(ex, "Exception registering default messenger {messengerKey}", messenger.Key);
                    continue;
                }
            }

            var simplMessengers = _messengers.Values.OfType<IDelayedConfiguration>().ToList();

            if (simplMessengers.Count > 0)
            {
                return;
            }

            _initialized = true;

            RegisterSystemToServer();
        }

        #region IMobileControl Members

        /// <summary>
        /// Gets the App Server instance
        /// </summary>
        public static IMobileControl GetAppServer()
        {
            try
            {
                var appServer =
                    DeviceManager.GetDevices().SingleOrDefault(s => s is IMobileControl)
                    as MobileControlSystemController;
                return appServer;
            }
            catch (Exception e)
            {
                Debug.LogMessage(e, "Unable to find MobileControlSystemController in Devices");
                return null;
            }
        }

        private bool CreateWebsocket()
        {
            _wsClient2?.Close();
            _wsClient2 = null;

            if (string.IsNullOrEmpty(SystemUuid))
            {
                this.LogError(
                    "System UUID not defined. Unable to connect to Mobile Control"
                );
                return false;
            }

            var wsHost = Host.Replace("http", "ws");
            var url = string.Format("{0}/system/join/{1}", wsHost, SystemUuid);

            _wsClient2 = new WebSocket(url)
            {
                Log =
                {
                    Output = (data, message) => Utilities.ConvertWebsocketLog(data, message, this)
                }
            };

            // setting to trace to let level be controlled by appdebug
            _wsClient2.Log.Level = LogLevel.Trace;

            _wsClient2.SslConfiguration.EnabledSslProtocols =
                // System.Security.Authentication.SslProtocols.Tls11
                System.Security.Authentication.SslProtocols.Tls12 
                | System.Security.Authentication.SslProtocols.Tls13;

            _wsClient2.OnMessage += HandleMessage;
            _wsClient2.OnOpen += HandleOpen;
            _wsClient2.OnError += HandleError;
            _wsClient2.OnClose += HandleClose;

            return true;
        }

        /// <summary>
        /// Link the System Monitor to this App server
        /// </summary>
        public void LinkSystemMonitorToAppServer()
        {
            if (CrestronEnvironment.DevicePlatform != eDevicePlatform.Appliance)
            {
                this.LogWarning(
                    "System Monitor does not exist for this platform. Skipping..."
                );
                return;
            }

            if (!(DeviceManager.GetDeviceForKey("systemMonitor") is SystemMonitorController sysMon))
            {
                return;
            }

            var key = sysMon.Key + "-" + Key;
            var messenger = new SystemMonitorMessenger(key, sysMon, "/device/systemMonitor");

            AddDeviceMessenger(messenger);
        }

        #endregion

        private void SetWebsocketDebugLevel(string cmdparameters)
        {
            if (string.IsNullOrEmpty(cmdparameters))
            {
                this.LogInformation("Current Websocket debug level: {webSocketDebugLevel}", _wsLogLevel);
                return;
            }

            if (cmdparameters.ToLower().Contains("help") || cmdparameters.ToLower().Contains("?"))
            {
                CrestronConsole.ConsoleCommandResponse(
                    $"valid options are:\r\n{LogLevel.Trace}\r\n{LogLevel.Debug}\r\n{LogLevel.Info}\r\n{LogLevel.Warn}\r\n{LogLevel.Error}\r\n{LogLevel.Fatal}\r\n"
                );
            }

            try
            {
                var debugLevel = (LogLevel)Enum.Parse(typeof(LogLevel), cmdparameters, true);

                _wsLogLevel = debugLevel;

                if (_wsClient2 != null)
                {
                    _wsClient2.Log.Level = _wsLogLevel;
                }

                _directServer?.SetWebsocketLogLevel(_wsLogLevel);

                CrestronConsole.ConsoleCommandResponse($"Websocket log level set to {debugLevel}");
            }
            catch
            {
                CrestronConsole.ConsoleCommandResponse(
                    $"{cmdparameters} is not a valid debug level. Valid options are:\r\n{LogLevel.Trace}\r\n{LogLevel.Debug}\r\n{LogLevel.Info}\r\n{LogLevel.Warn}\r\n{LogLevel.Error}\r\n{LogLevel.Fatal}\r\n"
                );

            }
        }

        private void CrestronEnvironment_ProgramStatusEventHandler(
            eProgramStatusEventType programEventType
        )
        {
            if (
                programEventType != eProgramStatusEventType.Stopping
                || _wsClient2 == null
                || !_wsClient2.IsAlive
            )
            {
                return;
            }

            _disableReconnect = true;

            StopServerReconnectTimer();
            CleanUpWebsocketClient();
        }

        /// <summary>
        /// PrintActionDictionaryPaths method
        /// </summary>
        public void PrintActionDictionaryPaths(object o)
        {
            CrestronConsole.ConsoleCommandResponse("ActionDictionary Contents:\r\n");

            foreach (var (messengerKey, actionPath) in GetActionDictionaryPaths())
            {
                CrestronConsole.ConsoleCommandResponse($"<{messengerKey}> {actionPath}\r\n");
            }
        }

        /// <summary>
        /// Get action paths for the current actions
        /// </summary>
        public List<(string, string)> GetActionDictionaryPaths()
        {
            var paths = new List<(string, string)>();

            foreach (var item in _actionDictionary)
            {
                var messengers = item.Value.Select(a => a.Messenger).Cast<MessengerBase>();
                foreach (var messenger in messengers)
                {
                    foreach (var actionPath in messenger.GetActionPaths())
                    {
                        paths.Add((messenger.Key, $"{item.Key}{actionPath}"));
                    }
                }
            }

            return paths;
        }

        /// <summary>
        /// Adds an action to the dictionary
        /// </summary>
        /// <param name="messenger">The messenger for the API command</param>
        /// <param name="action">The action to be triggered by the commmand</param>
        public void AddAction<T>(T messenger, Action<string, string, JToken> action)
            where T : IMobileControlMessenger
        {
            if (
                _actionDictionary.TryGetValue(
                    messenger.MessagePath,
                    out List<IMobileControlAction> actionList
                )
            )
            {
                if (
                    actionList.Any(a =>
                        a.Messenger.GetType() == messenger.GetType()
                        && a.Messenger.DeviceKey == messenger.DeviceKey
                    )
                )
                {
                    this.LogWarning("Messenger of type {messengerType} already exists. Skipping actions for {messengerKey}", messenger.GetType().Name, messenger.Key);
                    return;
                }

                actionList.Add(new MobileControlAction(messenger, action));
                return;
            }

            actionList = new List<IMobileControlAction>
            {
                new MobileControlAction(messenger, action)
            };

            _actionDictionary.Add(messenger.MessagePath, actionList);
        }

        /// <summary>
        /// Removes an action from the dictionary
        /// </summary>
        /// <param name="key"></param>
        /// <summary>
        /// RemoveAction method
        /// </summary>
        public void RemoveAction(string key)
        {
            if (_actionDictionary.ContainsKey(key))
            {
                _actionDictionary.Remove(key);
            }
        }

        /// <summary>
        /// Get the room bridge with the provided key
        /// </summary>
        /// <param name="key">The key of the room bridge</param>
        public MobileControlBridgeBase GetRoomBridge(string key)
        {
            return _roomBridges.FirstOrDefault((r) => r.RoomKey.Equals(key));
        }

        /// <summary>
        /// Get the room messenger with the provided key
        /// </summary>
        /// <param name="key">The Key of the rooom messenger</param>
        public IMobileControlRoomMessenger GetRoomMessenger(string key)
        {
            return _roomBridges.FirstOrDefault((r) => r.RoomKey.Equals(key));
        }

        private void Bridge_ConfigurationIsReady(object sender, EventArgs e)
        {
            this.LogDebug("Bridge ready.  Registering");

            // send the configuration object to the server

            if (_wsClient2 == null)
            {
                RegisterSystemToServer();
            }
            else if (!_wsClient2.IsAlive)
            {
                ConnectWebsocketClient();
            }
            else
            {
                SendInitialMessage();
            }
        }

        private void ReconnectToServerTimerCallback(object o)
        {
            this.LogDebug("Attempting to reconnect to server...");

            ConnectWebsocketClient();
        }

        private void AuthorizeSystem(string code)
        {
            if (
                string.IsNullOrEmpty(SystemUuid)
                || SystemUuid.Equals("missing url", StringComparison.OrdinalIgnoreCase)
            )
            {
                CrestronConsole.ConsoleCommandResponse(
                    "System does not have a UUID. Please ensure proper configuration is loaded and restart."
                );
                return;
            }
            if (string.IsNullOrEmpty(code))
            {
                CrestronConsole.ConsoleCommandResponse(
                    "Please enter a grant code to authorize a system"
                );
                return;
            }
            if (string.IsNullOrEmpty(Config.ServerUrl))
            {
                CrestronConsole.ConsoleCommandResponse(
                    "Mobile control API address is not set. Check portal configuration"
                );
                return;
            }

            var authTask = ApiService.SendAuthorizationRequest(Host, code, SystemUuid);

            authTask.ContinueWith(t =>
            {
                var response = t.Result;

                if (response.Authorized)
                {
                    this.LogDebug("System authorized, sending config.");
                    RegisterSystemToServer();
                    return;
                }

                this.LogInformation(response.Reason);
            });
        }

        private void ShowInfo()
        {
            var url = Config != null ? Host : "No config";
            string name;
            string code;
            if (_roomBridges != null && _roomBridges.Count > 0)
            {
                name = _roomBridges[0].RoomName;
                code = _roomBridges[0].UserCode;
            }
            else
            {
                name = "No config";
                code = "Not available";
            }
            var conn = _wsClient2 == null ? "No client" : (_wsClient2.IsAlive ? "Yes" : "No");

            var secSinceLastAck = DateTime.Now - _lastAckMessage;

            if (Config.EnableApiServer)
            {
                CrestronConsole.ConsoleCommandResponse(
                    "Mobile Control Edge Server API Information:\r\n\r\n" +
                    "\tServer address: {0}\r\n" +
                    "\tSystem Name: {1}\r\n" +
                    "\tSystem URL: {2}\r\n" +
                    "\tSystem UUID: {3}\r\n" +
                    "\tSystem User code: {4}\r\n" +
                    "\tConnected?: {5}\r\n" +
                    "\tSeconds Since Last Ack: {6}\r\n",
                    url,
                    name,
                    ConfigReader.ConfigObject.SystemUrl,
                    SystemUuid,
                    code,
                    conn,
                    secSinceLastAck.Seconds
                );
            }
            else
            {
                CrestronConsole.ConsoleCommandResponse(
                    "\r\nMobile Control Edge Server API Information:\r\n" +
                    "    Not Enabled in Config.\r\n"
                );
            }

            if (
                Config.DirectServer != null
                && Config.DirectServer.EnableDirectServer
                && _directServer != null
            )
            {
                CrestronConsole.ConsoleCommandResponse(
                    "\r\nMobile Control Direct Server Information:\r\n" +
                    "    User App URL: {0}\r\n" +
                    "    Server port: {1}\r\n",
                    string.Format("{0}[insert_client_token]", _directServer.UserAppUrlPrefix),
                    _directServer.Port
                );

                CrestronConsole.ConsoleCommandResponse(
                    "\r\n    UI Client Info:\r\n" +
                    "    Tokens Defined: {0}\r\n" +
                    "    Clients Connected: {1}\r\n",
                    _directServer.UiClientContexts.Count,
                    _directServer.ConnectedUiClientsCount
                );

                var clientNo = 1;
                foreach (var clientContext in _directServer.UiClientContexts)
                {
                    var clients = _directServer.UiClients.Values.Where(c => c.TokenKey == clientContext.Key);

                    CrestronConsole.ConsoleCommandResponse(
                        $"\r\nClient {clientNo}:\r\n" +
                        $"  Room Key: {clientContext.Value.Token.RoomKey}\r\n" +
                        $"  Touchpanel Key: {clientContext.Value.Token.TouchpanelKey}\r\n" +
                        $"  Token: {clientContext.Key}\r\n" +
                        $"  Client URL: {_directServer.UserAppUrlPrefix}{clientContext.Key}\r\n" +
                        $"  Clients:\r\n"
                    );

                    if (!clients.Any())
                    {
                        CrestronConsole.ConsoleCommandResponse("    No clients connected");
                    }
                    foreach (var client in clients)
                    {
                        CrestronConsole.ConsoleCommandResponse(
                            $"    ID: {client.Id}\r\n" +
                            $"    Connected: {client.Context.WebSocket.IsAlive}\r\n" +
                            $"    Duration: {(client.Context.WebSocket.IsAlive ? client.ConnectedDuration.TotalSeconds.ToString() : "Not Connected")}\r\n"
                        );
                    }

                    clientNo++;
                }
            }
            else
            {
                CrestronConsole.ConsoleCommandResponse(
                    "\r\nMobile Control Direct Server Information:\r\n" +
                    "    Not Enabled in Config.\r\n"
                );
            }

            var expectedAppVersion = ConfigReader.ConfigObject?.Versions?.TouchpanelWrapperApp?.Version;
            var userAppPath = Global.FilePathPrefix + "mcUserApp" + Global.DirectorySeparator;
            var versionCheck = TouchpanelWrapperAppVersionChecker.CheckDeployedVersion(userAppPath, expectedAppVersion);

            CrestronConsole.ConsoleCommandResponse(
                $"\r\nUI Wrapper App Deployed Version Check:\r\n    {versionCheck.Summary}\r\n"
            );
        }

        /// <summary>
        /// Register this system to the Mobile Control Edge Server
        /// </summary>
        public void RegisterSystemToServer()
        {

            if (!Config.EnableApiServer)
            {
                this.LogInformation(
                    "ApiServer disabled via config.  Cancelling attempt to register to server."
                );
                return;
            }

            var result = CreateWebsocket();

            if (!result)
            {
                this.LogFatal("Unable to create websocket.");
                return;
            }

            ConnectWebsocketClient();
        }

        private void ConnectWebsocketClient()
        {
            lock (_wsCriticalSection)
            {
                // set to 99999 to let things work on 4-Series
                if (
                    (CrestronEnvironment.ProgramCompatibility & eCrestronSeries.Series4)
                    == eCrestronSeries.Series4
                )
                {
                    _wsClient2.Log.Level = (LogLevel)99999;
                }
                else if (
                    (CrestronEnvironment.ProgramCompatibility & eCrestronSeries.Series3)
                    == eCrestronSeries.Series3
                )
                {
                    _wsClient2.Log.Level = _wsLogLevel;
                }

                //This version of the websocket client is TLS1.2 ONLY

                //Fires OnMessage event when PING is received.
                _wsClient2.EmitOnPing = true;

                this.LogDebug(
                    "Connecting mobile control client to {mobileControlUrl}",
                    _wsClient2.Url
                );

                TryConnect();
            }
        }

        private void TryConnect()
        {
            try
            {
                IsAuthorized = false;
                _wsClient2.Connect();
            }
            catch (InvalidOperationException)
            {
                this.LogError(
                    "Maximum retries exceeded. Restarting websocket"
                );
                HandleConnectFailure();
            }
            catch (IOException ex)
            {
                this.LogException(ex, "IO Exception on connect");
                HandleConnectFailure();
            }
            catch (Exception ex)
            {
                this.LogException(
                    ex,
                    "Error on Websocket Connect"
                );
                HandleConnectFailure();
            }
        }

        private void HandleConnectFailure()
        {
            _wsClient2 = null;

            var wsHost = Host.Replace("http", "ws");
            var url = string.Format("{0}/system/join/{1}", wsHost, SystemUuid);
            _wsClient2 = new WebSocket(url)
            {
                Log =
                {
                    Output = (data, s) =>
                        this.LogDebug(
                            "Message from websocket: {message}",
                            data
                        )
                }
            };

            _wsClient2.OnMessage -= HandleMessage;
            _wsClient2.OnOpen -= HandleOpen;
            _wsClient2.OnError -= HandleError;
            _wsClient2.OnClose -= HandleClose;

            _wsClient2.OnMessage += HandleMessage;
            _wsClient2.OnOpen += HandleOpen;
            _wsClient2.OnError += HandleError;
            _wsClient2.OnClose += HandleClose;

            StartServerReconnectTimer();
        }

        private void HandleOpen(object sender, EventArgs e)
        {
            StopServerReconnectTimer();
            StartPingTimer();
            this.LogInformation("Mobile Control API connected");
            SendMessageObject(new MobileControlMessage { Type = "hello" });
        }

        private void HandleMessage(object sender, MessageEventArgs e)
        {
            if (e.IsPing)
            {
                _lastAckMessage = DateTime.Now;
                IsAuthorized = true;
                ResetPingTimer();
                return;
            }

            if (e.IsText && e.Data.Length > 0)
            {
                _receiveQueue.Enqueue(new ProcessStringMessage(e.Data, ParseStreamRx));
            }
        }

        private void HandleError(object sender, ErrorEventArgs e)
        {
            this.LogError("Websocket error {0}", e.Message);

            IsAuthorized = false;
            StartServerReconnectTimer();
        }

        private void HandleClose(object sender, CloseEventArgs e)
        {
            this.LogDebug(
                "Websocket close {code} {reason}, clean={wasClean}",
                e.Code,
                e.Reason,
                e.WasClean
            );
            IsAuthorized = false;
            StopPingTimer();

            // Start the reconnect timer only if disableReconnect is false and the code isn't 4200. 4200 indicates system is not authorized;
            if (_disableReconnect || e.Code == 4200)
            {
                return;
            }

            StartServerReconnectTimer();
        }

        private void SendInitialMessage()
        {
            this.LogInformation("Sending initial join message");

            var touchPanels = DeviceManager
                .AllDevices.OfType<IMobileControlTouchpanelController>()
                .Where(tp => !tp.UseDirectServer)
                .Select(
                    (tp) =>
                    {
                        return new { touchPanelKey = tp.Key, roomKey = tp.DefaultRoomKey };
                    }
                );

            var msg = new MobileControlMessage
            {
                Type = "join",
                Content = JToken.FromObject(
                    new { config = GetConfigWithPluginVersion(), touchPanels }
                )
            };

            SendMessageObject(msg);
        }

        /// <summary>
        /// Get the Essentials configuration with version data
        /// </summary>
        public MobileControlEssentialsConfig GetConfigWithPluginVersion()
        {
            // Populate the application name and version number
            var confObject = new MobileControlEssentialsConfig(ConfigReader.ConfigObject);

            confObject.Info.RuntimeInfo.AppName = Assembly.GetExecutingAssembly().GetName().Name;

            var essentialsVersion = Global.AssemblyVersion;
            confObject.Info.RuntimeInfo.AssemblyVersion = essentialsVersion;


            //            // Set for local testing
            //            confObject.RuntimeInfo.PluginVersion = "4.0.0-localBuild";

            // Populate the plugin version
            var pluginVersion = Assembly
                .GetExecutingAssembly()
                .GetCustomAttributes(typeof(AssemblyInformationalVersionAttribute), false);


            if (pluginVersion[0] is AssemblyInformationalVersionAttribute fullVersionAtt)
            {
                var pluginInformationalVersion = fullVersionAtt.InformationalVersion;

                confObject.RuntimeInfo.PluginVersion = pluginInformationalVersion;
                confObject.RuntimeInfo.EssentialsVersion = Global.AssemblyVersion;
                confObject.RuntimeInfo.PepperDashCoreVersion = PluginLoader.PepperDashCoreAssembly.Version;
                confObject.RuntimeInfo.EssentialsPlugins = PluginLoader.EssentialsPluginAssemblies;
            }
            return confObject;
        }

        /// <summary>
        /// Set the Client URL for a given room
        /// </summary>
        /// <param name="path">new App URL</param>
        /// <param name="roomKey">room key. Default is null</param>
        /// <remarks>
        /// If roomKey is null, the URL will be set for the entire system.
        /// </remarks>
        public void SetClientUrl(string path, string roomKey = null)
        {
            var message = new MobileControlMessage
            {
                Type = string.IsNullOrEmpty(roomKey) ? $"/event/system/setUrl" : $"/event/room/{roomKey}/setUrl",
                Content = JToken.FromObject(new MobileControlSimpleContent<string> { Value = path })
            };

            SendMessageObject(message);
        }

        /// <summary>
        /// Sends any object type to server
        /// </summary>
        /// <param name="o"></param>
        public void SendMessageObject(IMobileControlMessage o)
        {

            if (Config.EnableApiServer)
            {

                _transmitToServerQueue.Enqueue(new TransmitMessage(o, _wsClient2));

            }

            if (
                Config.DirectServer != null
                && Config.DirectServer.EnableDirectServer
                && _directServer != null
            )
            {
                _transmitToClientsQueue.Enqueue(new MessageToClients(o, _directServer));
            }

        }


        /// <summary>
        /// Send a message to a client using the Direct Server
        /// </summary>
        /// <param name="o">object to send</param>
        public void SendMessageObjectToDirectClient(object o)
        {
            if (
                Config.DirectServer != null
                && Config.DirectServer.EnableDirectServer
                && _directServer != null
            )
            {
                _transmitToClientsQueue.Enqueue(new MessageToClients(o, _directServer));
            }
        }

        private void CleanUpWebsocketClient()
        {
            if (_wsClient2 == null)
            {
                return;
            }

            this.LogDebug("Disconnecting websocket");

            _wsClient2.Close();
        }

        private void ResetPingTimer()
        {
            // This tells us we're online with the API and getting pings
            _pingTimer.Stop();
            _pingTimer.Interval = PingInterval;
            _pingTimer.Start();
        }

        private void StartPingTimer()
        {
            StopPingTimer();
            _pingTimer = new Timer(PingInterval) { AutoReset = false };
            _pingTimer.Elapsed += (s, e) => PingTimerCallback(null);
            _pingTimer.Start();
        }

        private void StopPingTimer()
        {
            if (_pingTimer == null)
            {
                return;
            }

            _pingTimer.Stop();
            _pingTimer.Dispose();
            _pingTimer = null;
        }

        private void PingTimerCallback(object o)
        {
            this.LogDebug(

                "Ping timer expired. Closing websocket"
            );

            try
            {
                _wsClient2.Close();
            }
            catch (Exception ex)
            {
                this.LogException(ex,
                    "Exception closing websocket"
                );

                HandleConnectFailure();
            }
        }

        private void StartServerReconnectTimer()
        {
            StopServerReconnectTimer();
            _serverReconnectTimer = new Timer(ServerReconnectInterval) { AutoReset = false };
            _serverReconnectTimer.Elapsed += (s, e) => ReconnectToServerTimerCallback(null);
            _serverReconnectTimer.Start();
            this.LogDebug("Reconnect Timer Started.");
        }

        private void StopServerReconnectTimer()
        {
            if (_serverReconnectTimer == null)
            {
                return;
            }
            _serverReconnectTimer.Stop();
            _serverReconnectTimer = null;
        }

        private void HandleHeartBeat(JToken content)
        {
            SendMessageObject(new MobileControlMessage { Type = "/system/heartbeatAck" });

            var code = content["userCode"];
            if (code == null)
            {
                return;
            }

            foreach (var b in _roomBridges)
            {
                b.SetUserCode(code.Value<string>());
            }
        }

        private void HandleClientJoined(JToken content)
        {
            var clientId = content["clientId"].Value<string>();
            var roomKey = content["roomKey"].Value<string>();
            var touchpanelKey = content.SelectToken("touchpanelKey");

            if (_roomCombiner == null)
            {
                var message = new MobileControlMessage
                {
                    Type = "/system/roomKey",
                    ClientId = clientId,
                    Content = roomKey
                };

                SendMessageObject(message);

                SendDeviceInterfaces(clientId);

                SendTouchpanelKey(clientId, touchpanelKey);
                return;
            }

            if (_roomCombiner.CurrentScenario == null)
            {
                var message = new MobileControlMessage
                {
                    Type = "/system/roomKey",
                    ClientId = clientId,
                    Content = roomKey
                };

                SendMessageObject(message);

                SendDeviceInterfaces(clientId);

                SendTouchpanelKey(clientId, touchpanelKey);
                return;
            }

            if (!_roomCombiner.CurrentScenario.UiMap.ContainsKey(roomKey))
            {

                this.LogWarning(
                    "Unable to find correct roomKey for {roomKey} in current scenario. Returning {roomKey} as roomKey", roomKey);

                var message = new MobileControlMessage
                {
                    Type = "/system/roomKey",
                    ClientId = clientId,
                    Content = roomKey
                };

                SendMessageObject(message);

                SendDeviceInterfaces(clientId);

                SendTouchpanelKey(clientId, touchpanelKey);
                return;
            }

            var newRoomKey = _roomCombiner.CurrentScenario.UiMap[roomKey];

            var newMessage = new MobileControlMessage
            {
                Type = "/system/roomKey",
                ClientId = clientId,
                Content = newRoomKey
            };

            SendMessageObject(newMessage);

            SendDeviceInterfaces(clientId);

            SendTouchpanelKey(clientId, touchpanelKey);
        }

        private void SendTouchpanelKey(string clientId, JToken touchpanelKeyToken)
        {
            if (touchpanelKeyToken == null)
            {
                this.LogWarning("Touchpanel key not found for client {clientId}", clientId);
                return;
            }

            SendMessageObject(new MobileControlMessage
            {
                Type = "/system/touchpanelKey",
                ClientId = clientId,
                Content = touchpanelKeyToken.Value<string>()
            });
        }

        /// <summary>
        /// Handles a batch request for device status. Supports two content formats:
        /// <para>
        /// Granular format: <c>{ "devices": { "deviceKey": ["/fullStatus", "/layoutStatus"], ... } }</c>
        /// </para>
        /// <para>
        /// Legacy format: <c>{ "deviceKeys": ["deviceKey1", "deviceKey2"] }</c> (equivalent to /fullStatus for each)
        /// </para>
        /// Triggers the specified action paths for each device in parallel,
        /// then sends an /system/initialSyncComplete message after all have been processed.
        /// </summary>
        private void HandleBatchDeviceFullStatus(string clientId, JToken content)
        {
            if (content == null)
            {
                this.LogWarning("BatchDeviceFullStatus: content is null");
                return;
            }

            // Build a dictionary of deviceKey -> list of action paths
            Dictionary<string, List<string>> deviceActionPaths;

            var devicesToken = content.SelectToken("devices");
            if (devicesToken != null)
            {
                // Granular format: { "devices": { "key": ["/path1", "/path2"], ... } }
                deviceActionPaths = devicesToken.ToObject<Dictionary<string, List<string>>>();
            }
            else
            {
                // Legacy format: { "deviceKeys": ["key1", "key2"] } -> each gets /fullStatus
                var deviceKeys = content.SelectToken("deviceKeys")?.ToObject<List<string>>();

                if (deviceKeys == null || deviceKeys.Count == 0)
                {
                    this.LogWarning("BatchDeviceFullStatus: No device keys or devices provided");
                    SendMessageObject(new MobileControlMessage
                    {
                        Type = "/system/initialSyncComplete",
                        ClientId = clientId
                    });
                    return;
                }

                deviceActionPaths = deviceKeys.ToDictionary(k => k, _ => new List<string> { "/fullStatus" });
            }

            if (deviceActionPaths == null || deviceActionPaths.Count == 0)
            {
                this.LogWarning("BatchDeviceFullStatus: Empty devices dictionary");
                SendMessageObject(new MobileControlMessage
                {
                    Type = "/system/initialSyncComplete",
                    ClientId = clientId
                });
                return;
            }

            this.LogInformation("BatchDeviceFullStatus: Processing {count} devices", deviceActionPaths.Count);

            var tasks = new List<Task>();

            foreach (var kvp in deviceActionPaths)
            {
                var deviceKey = kvp.Key;
                var actionPaths = kvp.Value;

                if (actionPaths == null || actionPaths.Count == 0)
                {
                    continue;
                }

                foreach (var actionPath in actionPaths)
                {
                    var fullPath = $"/device/{deviceKey}{actionPath}";

                    var handlers = _actionDictionary
                        .Where(kv => fullPath.StartsWith(kv.Key + "/"))
                        .SelectMany(kv => kv.Value)
                        .ToList();

                    if (handlers.Count == 0)
                    {
                        this.LogDebug("BatchDeviceFullStatus: No handlers for {deviceKey} at path {actionPath}", deviceKey, actionPath);
                        continue;
                    }

                    foreach (var handler in handlers)
                    {
                        tasks.Add(Task.Run(() =>
                        {
                            try
                            {
                                handler.Action(fullPath, clientId, JToken.FromObject(new { deviceKey }));
                            }
                            catch (Exception ex)
                            {
                                this.LogError("BatchDeviceFullStatus: Exception in handler for {deviceKey} at {actionPath}: {message}", deviceKey, actionPath, ex.Message);
                            }
                        }));
                    }
                }
            }

            // After all handlers have completed and enqueued their responses, send sync complete
            Task.Run(async () =>
            {
                try
                {
                    await Task.WhenAll(tasks);
                }
                catch (Exception ex)
                {
                    this.LogError("BatchDeviceFullStatus: Exception waiting for tasks: {message}", ex.Message);
                }

                SendMessageObject(new MobileControlMessage
                {
                    Type = "/system/initialSyncComplete",
                    ClientId = clientId
                });
            });
        }

        private void SendDeviceInterfaces(string clientId)
        {
            this.LogDebug("Sending Device interfaces");
            var devices = DeviceManager.GetDevices();
            Dictionary<string, DeviceInterfaceInfo> deviceInterfaces = new Dictionary<string, DeviceInterfaceInfo>();

            foreach (var device in devices)
            {
                var interfaces = device?.GetType().GetInterfaces().Select((i) => i.Name).ToList() ?? new List<string>();

                deviceInterfaces.Add(device.Key, new DeviceInterfaceInfo
                {
                    Key = device.Key,
                    Name = (device as IKeyName)?.Name ?? "",
                    Interfaces = interfaces
                });
            }

            var message = new MobileControlMessage
            {
                Type = "/system/deviceInterfaces",
                ClientId = clientId,
                Content = JToken.FromObject(new { deviceInterfaces })
            };

            SendMessageObject(message);
        }

        private void HandleUserCode(JToken content, Action<string, string> action = null)
        {
            var code = content["userCode"];

            JToken qrChecksum;

            try
            {
                qrChecksum = content.SelectToken("qrChecksum", false);
            }
            catch
            {
                qrChecksum = new JValue(string.Empty);
            }

            if (code == null)
            {
                return;
            }

            if (action == null)
            {
                foreach (var bridge in _roomBridges)
                {
                    bridge.SetUserCode(code.Value<string>(), qrChecksum.Value<string>());
                }

                return;
            }

            action(code.Value<string>(), qrChecksum.Value<string>());
        }

        /// <summary>
        /// Enqueue an incoming message for processing
        /// </summary>
        public void HandleClientMessage(string message)
        {
            _receiveQueue.Enqueue(new ProcessStringMessage(message, ParseStreamRx));
        }

        private void ParseStreamRx(string messageText)
        {
            if (string.IsNullOrEmpty(messageText))
            {
                return;
            }

            if (!messageText.Contains("/system/heartbeat"))
            {
                this.LogDebug(
                    "Message RX: {messageText}",
                    messageText
                );
            }

            try
            {
                var message = JsonConvert.DeserializeObject<MobileControlMessage>(messageText);

                switch (message.Type)
                {
                    case "hello":
                        SendInitialMessage();
                        break;
                    case "/system/heartbeat":
                        HandleHeartBeat(message.Content);
                        break;
                    case "/system/userCode":
                        HandleUserCode(message.Content);
                        break;
                    case "/system/clientJoined":
                        HandleClientJoined(message.Content);
                        break;
                    case "/system/batchDeviceFullStatus":
                        HandleBatchDeviceFullStatus(message.ClientId, message.Content);
                        break;
                    case "/system/reboot":
                        SystemMonitorController.ProcessorReboot();
                        break;
                    case "/system/programReset":
                        SystemMonitorController.ProgramReset(InitialParametersClass.ApplicationNumber);
                        break;
                    case "raw":
                        var wrapper = message.Content.ToObject<DeviceActionWrapper>();
                        DeviceJsonApi.DoDeviceAction(wrapper);
                        break;
                    case "close":
                        this.LogDebug("Received close message from server");
                        break;
                    default:
                        // Incoming message example
                        // /room/roomA/status
                        // /room/roomAB/status

                        // ActionDictionary Keys example
                        // /room/roomA
                        // /room/roomAB

                        // Can't do direct comparison because it will match /room/roomA with /room/roomA/xxx instead of /room/roomAB/xxx
                        var handlers = _actionDictionary.Where(kv => message.Type.StartsWith(kv.Key + "/")).SelectMany(kv => kv.Value).ToList(); // adds trailing slash to ensure above case is handled


                        if (handlers.Count == 0)
                        {
                            this.LogInformation("-- Warning: Incoming message has no registered handler {type}", message.Type);
                            break;
                        }

                        foreach (var handler in handlers)
                        {
                            Task.Run(() =>
                            {
                                try
                                {
                                    handler.Action(message.Type, message.ClientId, message.Content);
                                }
                                catch (Exception ex)
                                {
                                    this.LogError(
                                        "Exception in handler for message type {type}, ClientId {clientId}",
                                        message.Type,
                                        message.ClientId
                                    );
                                    this.LogDebug(ex, "Stack Trace: ");
                                }
                            }).ContinueWith(task =>
                            {
                                if (task.IsFaulted && task.Exception != null)
                                {
                                    this.LogError(
                                        "Unhandled exception in Task for message type {type}, ClientId {clientId}",
                                        message.Type,
                                        message.ClientId
                                    );
                                    this.LogDebug(task.Exception.GetBaseException(), "Stack Trace: ");
                                }
                            }, TaskContinuationOptions.OnlyOnFaulted);
                        }

                        break;
                }
            }
            catch (Exception err)
            {
                this.LogException(
                    err,
                    "Unable to parse {message}",
                    messageText
                );
            }
        }

        private void TestHttpRequest(string s)
        {
            {
                s = s.Trim();
                if (string.IsNullOrEmpty(s))
                {
                    PrintTestHttpRequestUsage();
                    return;
                }
                var tokens = s.Split(' ');
                if (tokens.Length < 2)
                {
                    CrestronConsole.ConsoleCommandResponse("Too few paramaters\r");
                    PrintTestHttpRequestUsage();
                    return;
                }

                try
                {
                    var url = tokens[1];
                    switch (tokens[0].ToLower())
                    {
                        case "get":
                            {
                                var resp = new HttpClient().Get(url);
                                CrestronConsole.ConsoleCommandResponse("RESPONSE:\r{0}\r\r", resp);
                            }
                            break;
                        case "post":
                            {
                                var resp = new HttpClient().Post(url, new byte[] { });
                                CrestronConsole.ConsoleCommandResponse("RESPONSE:\r{0}\r\r", resp);
                            }
                            break;
                        default:
                            CrestronConsole.ConsoleCommandResponse("Only get or post supported\r");
                            PrintTestHttpRequestUsage();
                            break;
                    }
                }
                catch (HttpException e)
                {
                    CrestronConsole.ConsoleCommandResponse("Exception in request:\r");
                    CrestronConsole.ConsoleCommandResponse(
                        "Response URL: {0}\r",
                        e.Response.ResponseUrl
                    );
                    CrestronConsole.ConsoleCommandResponse(
                        "Response Error Code: {0}\r",
                        e.Response.Code
                    );
                    CrestronConsole.ConsoleCommandResponse(
                        "Response body: {0}\r",
                        e.Response.ContentString
                    );
                }
            }
        }

        private void PrintTestHttpRequestUsage()
        {
            CrestronConsole.ConsoleCommandResponse("Usage: mobilehttprequest:N get/post url\r");
        }
    }
}