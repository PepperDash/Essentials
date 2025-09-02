using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Text;
using Crestron.SimplSharp;
using Crestron.SimplSharp.WebScripting;
using Newtonsoft.Json;
using PepperDash.Core;
using PepperDash.Core.Logging;
using PepperDash.Essentials.Core;
using PepperDash.Essentials.Core.DeviceTypeInterfaces;
using PepperDash.Essentials.Core.Web;
using PepperDash.Essentials.RoomBridges;
using PepperDash.Essentials.WebApiHandlers;
using Serilog.Events;
using WebSocketSharp;
using WebSocketSharp.Net;
using WebSocketSharp.Server;


namespace PepperDash.Essentials.WebSocketServer
{
    /// <summary>
    /// Represents a MobileControlWebsocketServer
    /// </summary>
    public class MobileControlWebsocketServer : EssentialsDevice
    {
        private readonly string userAppPath = Global.FilePathPrefix + "mcUserApp" + Global.DirectorySeparator;

        private readonly string localConfigFolderName = "_local-config";

        private readonly string appConfigFileName = "_config.local.json";
        private readonly string appConfigCsFileName = "_config.cs.json";

        /// <summary>
        /// Where the key is the join token and the value is the room key
        /// </summary>
        //private Dictionary<string, JoinToken> _joinTokens;

        private HttpServer _server;

        /// <summary>
        /// Gets the HttpServer instance
        /// </summary>
        public HttpServer Server => _server;

        /// <summary>
        /// Gets the collection of UI client contexts
        /// </summary>
        public Dictionary<string, UiClientContext> UiClients { get; private set; }

        private readonly MobileControlSystemController _parent;

        private WebSocketServerSecretProvider _secretProvider;

        private ServerTokenSecrets _secret;

        private static readonly HttpClient LogClient = new HttpClient();

        private string SecretProviderKey
        {
            get
            {
                return string.Format("{0}:{1}-tokens", Global.ControlSystem.ProgramNumber, Key);
            }
        }

        private string LanIpAddress => CrestronEthernetHelper.GetEthernetParameter(CrestronEthernetHelper.ETHERNET_PARAMETER_TO_GET.GET_CURRENT_IP_ADDRESS, CrestronEthernetHelper.GetAdapterdIdForSpecifiedAdapterType(EthernetAdapterType.EthernetLANAdapter));

        private readonly System.Net.IPAddress csIpAddress;

        private readonly System.Net.IPAddress csSubnetMask;

        /// <summary>
        /// The path for the WebSocket messaging
        /// </summary>
        private readonly string _wsPath = "/mc/api/ui/join/";

        /// <summary>
        /// Gets the WebSocket path
        /// </summary>
        public string WsPath => _wsPath;

        /// <summary>
        /// The path to the location of the files for the user app (single page Angular app)
        /// </summary>
        private readonly string _appPath = string.Format("{0}mcUserApp", Global.FilePathPrefix);

        /// <summary>
        /// The base HREF that the user app uses
        /// </summary>
        private string _userAppBaseHref = "/mc/app";

        /// <summary>
        /// Gets or sets the Port
        /// </summary>
        public int Port { get; private set; }

        /// <summary>
        /// Gets the user app URL prefix
        /// </summary>
        public string UserAppUrlPrefix
        {
            get
            {
                return string.Format("http://{0}:{1}{2}?token=",
                    CrestronEthernetHelper.GetEthernetParameter(CrestronEthernetHelper.ETHERNET_PARAMETER_TO_GET.GET_CURRENT_IP_ADDRESS, 0),
                    Port,
                    _userAppBaseHref);

            }
        }

        /// <summary>
        /// Gets the count of connected UI clients
        /// </summary>
        public int ConnectedUiClientsCount
        {
            get
            {
                var count = 0;

                foreach (var client in UiClients)
                {
                    if (client.Value.Client != null && client.Value.Client.Context.WebSocket.IsAlive)
                    {
                        count++;
                    }
                }

                return count;
            }
        }

        /// <summary>
        /// Initializes a new instance of the MobileControlWebsocketServer class.
        /// </summary>
        public MobileControlWebsocketServer(string key, int customPort, MobileControlSystemController parent)
            : base(key)
        {
            _parent = parent;

            // Set the default port to be 50000 plus the slot number of the program
            Port = 50000 + (int)Global.ControlSystem.ProgramNumber;

            if (customPort != 0)
            {
                Port = customPort;
            }

            if (parent.Config.DirectServer.AutomaticallyForwardPortToCSLAN == true)
            {
                try
                {
                    Debug.LogMessage(LogEventLevel.Information, "Automatically forwarding port {0} to CS LAN", Port);

                    var csAdapterId = CrestronEthernetHelper.GetAdapterdIdForSpecifiedAdapterType(EthernetAdapterType.EthernetCSAdapter);
                    var csIp = CrestronEthernetHelper.GetEthernetParameter(CrestronEthernetHelper.ETHERNET_PARAMETER_TO_GET.GET_CURRENT_IP_ADDRESS, csAdapterId);

                    var result = CrestronEthernetHelper.AddPortForwarding((ushort)Port, (ushort)Port, csIp, CrestronEthernetHelper.ePortMapTransport.TCP);

                    if (result != CrestronEthernetHelper.PortForwardingUserPatRetCodes.NoErr)
                    {
                        Debug.LogMessage(LogEventLevel.Error, "Error adding port forwarding: {0}", result);
                    }
                }
                catch (ArgumentException)
                {
                    Debug.LogMessage(LogEventLevel.Information, "This processor does not have a CS LAN", this);
                }
                catch (Exception ex)
                {
                    Debug.LogMessage(ex, "Error automatically forwarding port to CS LAN");
                }
            }

            try
            {
                var csAdapterId = CrestronEthernetHelper.GetAdapterdIdForSpecifiedAdapterType(EthernetAdapterType.EthernetCSAdapter);
                var csSubnetMask = CrestronEthernetHelper.GetEthernetParameter(CrestronEthernetHelper.ETHERNET_PARAMETER_TO_GET.GET_CURRENT_IP_MASK, csAdapterId);
                var csIpAddress = CrestronEthernetHelper.GetEthernetParameter(CrestronEthernetHelper.ETHERNET_PARAMETER_TO_GET.GET_CURRENT_IP_ADDRESS, csAdapterId);

                this.csSubnetMask = System.Net.IPAddress.Parse(csSubnetMask);
                this.csIpAddress = System.Net.IPAddress.Parse(csIpAddress);
            }
            catch (ArgumentException)
            {
                if (parent.Config.DirectServer.AutomaticallyForwardPortToCSLAN == false)
                {
                    Debug.LogMessage(LogEventLevel.Information, "This processor does not have a CS LAN", this);
                }
            }


            UiClients = new Dictionary<string, UiClientContext>();

            //_joinTokens = new Dictionary<string, JoinToken>();

            if (Global.Platform == eDevicePlatform.Appliance)
            {
                AddConsoleCommands();
            }

            AddPreActivationAction(() => AddWebApiPaths());
        }

        private void AddWebApiPaths()
        {
            var apiServer = DeviceManager.AllDevices.OfType<EssentialsWebApi>().FirstOrDefault();

            if (apiServer == null)
            {
                this.LogInformation("No API Server available");
                return;
            }

            var routes = new List<HttpCwsRoute>
            {
                new HttpCwsRoute($"devices/{Key}/client")
                {
                    Name = "ClientHandler",
                    RouteHandler = new UiClientHandler(this)
                },
            };

            apiServer.AddRoute(routes);
        }

        private void AddConsoleCommands()
        {
            CrestronConsole.AddNewConsoleCommand(GenerateClientTokenFromConsole, "MobileAddUiClient", "Adds a client and generates a token. ? for more help", ConsoleAccessLevelEnum.AccessOperator);
            CrestronConsole.AddNewConsoleCommand(RemoveToken, "MobileRemoveUiClient", "Removes a client. ? for more help", ConsoleAccessLevelEnum.AccessOperator);
            CrestronConsole.AddNewConsoleCommand((s) => PrintClientInfo(), "MobileGetClientInfo", "Displays the current client info", ConsoleAccessLevelEnum.AccessOperator);
            CrestronConsole.AddNewConsoleCommand(RemoveAllTokens, "MobileRemoveAllClients", "Removes all clients", ConsoleAccessLevelEnum.AccessOperator);
        }


        /// <summary>
        /// Initialize method
        /// </summary>
        /// <inheritdoc />
        public override void Initialize()
        {
            try
            {
                base.Initialize();

                _server = new HttpServer(Port, false);

                _server.OnGet += Server_OnGet;

                _server.OnOptions += Server_OnOptions;

                if (_parent.Config.DirectServer.Logging.EnableRemoteLogging)
                {
                    _server.OnPost += Server_OnPost;
                }

                CrestronEnvironment.ProgramStatusEventHandler += CrestronEnvironment_ProgramStatusEventHandler;

                _server.Start();

                if (_server.IsListening)
                {
                    Debug.LogMessage(LogEventLevel.Information, "Mobile Control WebSocket Server listening on port {port}", this, _server.Port);
                }

                CrestronEnvironment.ProgramStatusEventHandler += OnProgramStop;

                RetrieveSecret();

                CreateFolderStructure();

                AddClientsForTouchpanels();
            }
            catch (Exception ex)
            {
                Debug.LogMessage(ex, "Exception intializing websocket server", this);
            }
        }

        private void AddClientsForTouchpanels()
        {
            var touchpanels = DeviceManager.AllDevices
                .OfType<IMobileControlTouchpanelController>().Where(tp => tp.UseDirectServer);


            var touchpanelsToAdd = new List<IMobileControlTouchpanelController>();

            if (_secret != null)
            {
                var newTouchpanels = touchpanels.Where(tp => !_secret.Tokens.Any(t => t.Value.TouchpanelKey != null && t.Value.TouchpanelKey.Equals(tp.Key, StringComparison.InvariantCultureIgnoreCase)));

                touchpanelsToAdd.AddRange(newTouchpanels);
            }
            else
            {
                touchpanelsToAdd.AddRange(touchpanels);
            }

            foreach (var client in touchpanelsToAdd)
            {
                var bridge = _parent.GetRoomBridge(client.DefaultRoomKey);

                if (bridge == null)
                {
                    this.LogWarning("Unable to find room with key: {defaultRoomKey}", client.DefaultRoomKey);
                    return;
                }

                var (key, path) = GenerateClientToken(bridge, client.Key);

                if (key == null)
                {
                    this.LogWarning("Unable to generate a client for {clientKey}", client.Key);
                    continue;
                }
            }

            var lanAdapterId = CrestronEthernetHelper.GetAdapterdIdForSpecifiedAdapterType(EthernetAdapterType.EthernetLANAdapter);

            var processorIp = CrestronEthernetHelper.GetEthernetParameter(CrestronEthernetHelper.ETHERNET_PARAMETER_TO_GET.GET_CURRENT_IP_ADDRESS, lanAdapterId);

            foreach (var touchpanel in touchpanels.Select(tp =>
            {
                var token = _secret.Tokens.FirstOrDefault((t) => t.Value.TouchpanelKey.Equals(tp.Key, StringComparison.InvariantCultureIgnoreCase));

                var messenger = _parent.GetRoomBridge(tp.DefaultRoomKey);

                return new { token.Key, Touchpanel = tp, Messenger = messenger };
            }))
            {
                if (touchpanel.Key == null)
                {
                    this.LogWarning("Token for touchpanel {touchpanelKey} not found", touchpanel.Touchpanel.Key);
                    continue;
                }

                if (touchpanel.Messenger == null)
                {
                    this.LogWarning("Unable to find room messenger for {defaultRoomKey}", touchpanel.Touchpanel.DefaultRoomKey);
                    continue;
                }

                string ip = processorIp;

                // Moved to the MobileControlTouchpanelController class in the GetUrlWithCorrectIp method
                // triggered by the Panel.IpInformationChange event so that we know we have the necessary info
                // to make the determination of which IP to use.
                //if (touchpanel.Touchpanel is IMobileControlCrestronTouchpanelController crestronTouchpanel && csIpAddress != null)
                //{
                //    ip = crestronTouchpanel.ConnectedIps.Any(ipInfo =>
                //    {
                //        if (System.Net.IPAddress.TryParse(ipInfo.DeviceIpAddress, out var parsedIp))
                //        {
                //            return csIpAddress.IsInSameSubnet(parsedIp, csSubnetMask);
                //        }
                //        this.LogWarning("Invalid IP address: {deviceIpAddress}", ipInfo.DeviceIpAddress);
                //        return false;
                //    }) ? csIpAddress.ToString() : processorIp;
                //}

                if (_parent.Config.DirectServer.CSLanUiDeviceKeys != null && _parent.Config.DirectServer.CSLanUiDeviceKeys.Any(k => k.Equals(touchpanel.Touchpanel.Key, StringComparison.InvariantCultureIgnoreCase)) && csIpAddress != null)
                {
                    ip = csIpAddress.ToString();
                }

                var appUrl = $"http://{ip}:{_parent.Config.DirectServer.Port}/mc/app?token={touchpanel.Key}";

                this.LogVerbose("Sending URL {appUrl}", appUrl);

                touchpanel.Messenger.UpdateAppUrl($"http://{ip}:{_parent.Config.DirectServer.Port}/mc/app?token={touchpanel.Key}");
            }
        }

        private void OnProgramStop(eProgramStatusEventType programEventType)
        {
            switch (programEventType)
            {
                case eProgramStatusEventType.Stopping:
                    _server.Stop();
                    break;
            }
        }

        private void CreateFolderStructure()
        {
            if (!Directory.Exists(userAppPath))
            {
                Directory.CreateDirectory(userAppPath);
            }

            if (!Directory.Exists($"{userAppPath}{localConfigFolderName}"))
            {
                Directory.CreateDirectory($"{userAppPath}{localConfigFolderName}");
            }

            using (var sw = new StreamWriter(File.Open($"{userAppPath}{localConfigFolderName}{Global.DirectorySeparator}{appConfigFileName}", FileMode.Create, FileAccess.ReadWrite)))
            {
                // Write the LAN application configuration file. Used when a request comes in for the application config from the LAN 
                var lanAdapterId = CrestronEthernetHelper.GetAdapterdIdForSpecifiedAdapterType(EthernetAdapterType.EthernetLANAdapter);

                this.LogDebug("LAN Adapter ID: {lanAdapterId}", lanAdapterId);

                var processorIp = CrestronEthernetHelper.GetEthernetParameter(CrestronEthernetHelper.ETHERNET_PARAMETER_TO_GET.GET_CURRENT_IP_ADDRESS, lanAdapterId);

                var config = GetApplicationConfig(processorIp);

                var contents = JsonConvert.SerializeObject(config, Formatting.Indented);

                sw.Write(contents);
            }

            short csAdapterId;
            try
            {
                csAdapterId = CrestronEthernetHelper.GetAdapterdIdForSpecifiedAdapterType(EthernetAdapterType.EthernetCSAdapter);
            }
            catch (ArgumentException)
            {
                this.LogDebug("This processor does not have a CS LAN");
                return;
            }

            if (csAdapterId == -1)
            {
                this.LogDebug("CS LAN Adapter not found");
                return;
            }

            this.LogDebug("CS LAN Adapter ID: {csAdapterId}. Adding CS Config", csAdapterId);

            using (var sw = new StreamWriter(File.Open($"{userAppPath}{localConfigFolderName}{Global.DirectorySeparator}{appConfigCsFileName}", FileMode.Create, FileAccess.ReadWrite)))
            {
                // Write the CS application configuration file. Used when a request comes in for the application config from the CS
                var processorIp = CrestronEthernetHelper.GetEthernetParameter(CrestronEthernetHelper.ETHERNET_PARAMETER_TO_GET.GET_CURRENT_IP_ADDRESS, csAdapterId);

                var config = GetApplicationConfig(processorIp);

                var contents = JsonConvert.SerializeObject(config, Formatting.Indented);

                sw.Write(contents);
            }
        }

        private MobileControlApplicationConfig GetApplicationConfig(string processorIp)
        {
            try
            {
                var config = new MobileControlApplicationConfig
                {
                    ApiPath = string.Format("http://{0}:{1}/mc/api", processorIp, _parent.Config.DirectServer.Port),
                    GatewayAppPath = "",
                    LogoPath = _parent.Config.ApplicationConfig?.LogoPath ?? "logo/logo.png",
                    EnableDev = _parent.Config.ApplicationConfig?.EnableDev ?? false,
                    IconSet = _parent.Config.ApplicationConfig?.IconSet ?? MCIconSet.GOOGLE,
                    LoginMode = _parent.Config.ApplicationConfig?.LoginMode ?? "room-list",
                    Modes = _parent.Config.ApplicationConfig?.Modes ?? new Dictionary<string, McMode>
                    {
                        {
                            "room-list",
                            new McMode {
                                ListPageText = "Please select your room",
                                LoginHelpText = "Please select your room from the list, then enter the code shown on the display.",
                                PasscodePageText = "Please enter the code shown on this room's display"
                            }
                        }
                    },
                    Logging = _parent.Config.ApplicationConfig?.Logging ?? false,
                    PartnerMetadata = _parent.Config.ApplicationConfig?.PartnerMetadata ?? new List<MobileControlPartnerMetadata>()
                };

                return config;
            }
            catch (Exception ex)
            {
                this.LogError(ex, "Error getting application configuration");

                return null;
            }
        }

        /// <summary>
        /// Attempts to retrieve secrets previously stored in memory
        /// </summary>
        private void RetrieveSecret()
        {
            try
            {
                // Add secret provider
                _secretProvider = new WebSocketServerSecretProvider(SecretProviderKey);

                // Check for existing secrets
                var secret = _secretProvider.GetSecret(SecretProviderKey);

                if (secret != null)
                {
                    Debug.LogMessage(LogEventLevel.Information, "Secret successfully retrieved", this);

                    Debug.LogMessage(LogEventLevel.Debug, "Secret: {0}", this, secret.Value.ToString());


                    // populate the local secrets object
                    _secret = JsonConvert.DeserializeObject<ServerTokenSecrets>(secret.Value.ToString());

                    if (_secret != null && _secret.Tokens != null)
                    {
                        // populate the _uiClient collection
                        foreach (var token in _secret.Tokens)
                        {
                            if (token.Value == null)
                            {
                                Debug.LogMessage(LogEventLevel.Warning, "Token value is null", this);
                                continue;
                            }

                            Debug.LogMessage(LogEventLevel.Information, "Adding token: {0} for room: {1}", this, token.Key, token.Value.RoomKey);

                            if (UiClients == null)
                            {
                                Debug.LogMessage(LogEventLevel.Warning, "UiClients is null", this);
                                UiClients = new Dictionary<string, UiClientContext>();
                            }

                            UiClients.Add(token.Key, new UiClientContext(token.Value));
                        }
                    }

                    if (UiClients.Count > 0)
                    {
                        Debug.LogMessage(LogEventLevel.Information, "Restored {uiClientCount} UiClients from secrets data", this, UiClients.Count);

                        foreach (var client in UiClients)
                        {
                            var key = client.Key;
                            var path = _wsPath + key;
                            var roomKey = client.Value.Token.RoomKey;

                            _server.AddWebSocketService(path, () =>
                            {
                                var c = new UiClient();
                                Debug.LogMessage(LogEventLevel.Debug, "Constructing UiClient with id: {key}", this, key);

                                c.Controller = _parent;
                                c.RoomKey = roomKey;
                                UiClients[key].SetClient(c);
                                return c;
                            });


                            //_server.WebSocketServices.AddService<UiClient>(path, (c) =>
                            //{
                            //    Debug.Console(2, this, "Constructing UiClient with id: {0}", key);
                            //    c.Controller = _parent;
                            //    c.RoomKey = roomKey;
                            //    UiClients[key].SetClient(c);
                            //});
                        }
                    }
                }
                else
                {
                    Debug.LogMessage(LogEventLevel.Warning, "No secret found");
                }

                Debug.LogMessage(LogEventLevel.Debug, "{uiClientCount} UiClients restored from secrets data", this, UiClients.Count);
            }
            catch (Exception ex)
            {
                Debug.LogMessage(ex, "Exception retrieving secret", this);
            }
        }

        /// <summary>
        /// UpdateSecret method
        /// </summary>
        public void UpdateSecret()
        {
            try
            {
                if (_secret == null)
                {
                    Debug.LogMessage(LogEventLevel.Error, "Secret is null", this);

                    _secret = new ServerTokenSecrets(string.Empty);
                }

                _secret.Tokens.Clear();

                foreach (var uiClientContext in UiClients)
                {
                    _secret.Tokens.Add(uiClientContext.Key, uiClientContext.Value.Token);
                }

                var serializedSecret = JsonConvert.SerializeObject(_secret);

                _secretProvider.SetSecret(SecretProviderKey, serializedSecret);
            }
            catch (Exception ex)
            {
                Debug.LogMessage(ex, "Exception updating secret", this);
            }
        }

        /// <summary>
        /// Generates a new token based on validating a room key and grant code passed in.  If valid, returns a token and adds a service to the server for that token's path
        /// </summary>
        /// <param name="s"></param>
        private void GenerateClientTokenFromConsole(string s)
        {
            if (s == "?" || string.IsNullOrEmpty(s))
            {
                CrestronConsole.ConsoleCommandResponse(@"[RoomKey] [GrantCode] Validates the room key against the grant code and returns a token for use in a UI client");
                return;
            }

            var values = s.Split(' ');

            if (values.Length < 2)
            {
                CrestronConsole.ConsoleCommandResponse("Invalid number of arguments.  Please provide a room key and a grant code");
                return;
            }


            var roomKey = values[0];
            var grantCode = values[1];

            var bridge = _parent.GetRoomBridge(roomKey);

            if (bridge == null)
            {
                CrestronConsole.ConsoleCommandResponse(string.Format("Unable to find room with key: {0}", roomKey));
                return;
            }

            var (token, path) = ValidateGrantCode(grantCode, bridge);

            if (token == null)
            {
                CrestronConsole.ConsoleCommandResponse("Grant Code is not valid");
                return;
            }

            CrestronConsole.ConsoleCommandResponse($"Added new WebSocket UiClient service at path: {path}");
            CrestronConsole.ConsoleCommandResponse($"Token: {token}");
        }

        /// <summary>
        /// Validates the grant code against the room key
        /// </summary>
        public (string, string) ValidateGrantCode(string grantCode, string roomKey)
        {
            var bridge = _parent.GetRoomBridge(roomKey);

            if (bridge == null)
            {
                this.LogWarning("Unable to find room with key: {roomKey}", roomKey);
                return (null, null);
            }

            return ValidateGrantCode(grantCode, bridge);
        }

        /// <summary>
        /// Validates the grant code against the room key
        /// </summary>
        public (string, string) ValidateGrantCode(string grantCode, MobileControlBridgeBase bridge)
        {
            // TODO: Authenticate grant code passed in
            // For now, we just generate a random guid as the token and use it as the ClientId as well
            var grantCodeIsValid = true;

            if (grantCodeIsValid)
            {
                if (_secret == null)
                {
                    _secret = new ServerTokenSecrets(grantCode);
                }

                return GenerateClientToken(bridge, "");
            }
            else
            {
                return (null, null);
            }
        }

        /// <summary>
        /// Generates a new client token for the specified bridge
        /// </summary>
        public (string, string) GenerateClientToken(MobileControlBridgeBase bridge, string touchPanelKey = "")
        {
            var key = Guid.NewGuid().ToString();

            var token = new JoinToken { Code = bridge.UserCode, RoomKey = bridge.RoomKey, Uuid = _parent.SystemUuid, TouchpanelKey = touchPanelKey };

            UiClients.Add(key, new UiClientContext(token));

            var path = _wsPath + key;

            _server.AddWebSocketService(path, () =>
            {
                var c = new UiClient();
                Debug.LogMessage(LogEventLevel.Verbose, "Constructing UiClient with id: {0}", this, key);
                c.Controller = _parent;
                c.RoomKey = bridge.RoomKey;
                UiClients[key].SetClient(c);
                return c;
            });

            Debug.LogMessage(LogEventLevel.Information, "Added new WebSocket UiClient service at path: {path}", this, path);
            Debug.LogMessage(LogEventLevel.Information, "Token: {@token}", this, token);

            Debug.LogMessage(LogEventLevel.Verbose, "{serviceCount} websocket services present", this, _server.WebSocketServices.Count);

            UpdateSecret();

            return (key, path);
        }

        /// <summary>
        /// Removes all clients from the server
        /// </summary>
        private void RemoveAllTokens(string s)
        {
            if (s == "?" || string.IsNullOrEmpty(s))
            {
                CrestronConsole.ConsoleCommandResponse(@"Removes all clients from the server.  To execute add 'confirm' to command");
                return;
            }

            if (s != "confirm")
            {
                CrestronConsole.ConsoleCommandResponse(@"To remove all clients, add 'confirm' to the command");
                return;
            }

            foreach (var client in UiClients)
            {
                if (client.Value.Client != null && client.Value.Client.Context.WebSocket.IsAlive)
                {
                    client.Value.Client.Context.WebSocket.Close(CloseStatusCode.Normal, "Server Shutting Down");
                }

                var path = _wsPath + client.Key;
                if (_server.RemoveWebSocketService(path))
                {
                    CrestronConsole.ConsoleCommandResponse(string.Format("Client removed with token: {0}", client.Key));
                }
                else
                {
                    CrestronConsole.ConsoleCommandResponse(string.Format("Unable to remove client with token : {0}", client.Key));
                }
            }

            UiClients.Clear();

            UpdateSecret();
        }

        /// <summary>
        /// Removes a client with the specified token value
        /// </summary>
        /// <param name="s"></param>
        private void RemoveToken(string s)
        {
            if (s == "?" || string.IsNullOrEmpty(s))
            {
                CrestronConsole.ConsoleCommandResponse(@"[token] Removes the client with the specified token value");
                return;
            }

            var key = s;

            if (UiClients.ContainsKey(key))
            {
                var uiClientContext = UiClients[key];

                if (uiClientContext.Client != null && uiClientContext.Client.Context.WebSocket.IsAlive)
                {
                    uiClientContext.Client.Context.WebSocket.Close(CloseStatusCode.Normal, "Token removed from server");
                }

                var path = _wsPath + key;
                if (_server.RemoveWebSocketService(path))
                {
                    UiClients.Remove(key);

                    UpdateSecret();

                    CrestronConsole.ConsoleCommandResponse(string.Format("Client removed with token: {0}", key));
                }
                else
                {
                    CrestronConsole.ConsoleCommandResponse(string.Format("Unable to remove client with token : {0}", key));
                }
            }
            else
            {
                CrestronConsole.ConsoleCommandResponse(string.Format("Unable to find client with token: {0}", key));
            }
        }

        /// <summary>
        /// Prints out info about current client IDs
        /// </summary>
        private void PrintClientInfo()
        {
            CrestronConsole.ConsoleCommandResponse("Mobile Control UI Client Info:\r");

            CrestronConsole.ConsoleCommandResponse(string.Format("{0} clients found:\r", UiClients.Count));

            foreach (var client in UiClients)
            {
                CrestronConsole.ConsoleCommandResponse(string.Format("RoomKey: {0} Token: {1}\r", client.Value.Token.RoomKey, client.Key));
            }
        }

        private void CrestronEnvironment_ProgramStatusEventHandler(eProgramStatusEventType programEventType)
        {
            if (programEventType == eProgramStatusEventType.Stopping)
            {
                foreach (var client in UiClients.Values)
                {
                    if (client.Client != null && client.Client.Context.WebSocket.IsAlive)
                    {
                        client.Client.Context.WebSocket.Close(CloseStatusCode.Normal, "Server Shutting Down");
                    }
                }

                StopServer();
            }
        }

        /// <summary>
        /// Handler for GET requests to server
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Server_OnGet(object sender, HttpRequestEventArgs e)
        {
            try
            {
                var req = e.Request;
                var res = e.Response;
                res.ContentEncoding = Encoding.UTF8;

                res.AddHeader("Access-Control-Allow-Origin", "*");

                var path = req.RawUrl;

                this.LogVerbose("GET Request received at path: {path}", path);

                // Call for user app to join the room with a token
                if (path.StartsWith("/mc/api/ui/joinroom"))
                {
                    HandleJoinRequest(req, res);
                }
                // Call to get the server version
                else if (path.StartsWith("/mc/api/version"))
                {
                    HandleVersionRequest(res);
                }
                else if (path.StartsWith("/mc/app/logo"))
                {
                    HandleImageRequest(req, res);
                }
                // Call to serve the user app
                else if (path.StartsWith(_userAppBaseHref))
                {
                    HandleUserAppRequest(req, res, path);
                }
                else
                {
                    // All other paths
                    res.StatusCode = 404;
                    res.Close();
                }
            }
            catch (Exception ex)
            {
                Debug.LogMessage(ex, "Caught an exception in the OnGet handler", this);
            }
        }

        private async void Server_OnPost(object sender, HttpRequestEventArgs e)
        {
            try
            {
                var req = e.Request;
                var res = e.Response;

                res.AddHeader("Access-Control-Allow-Origin", "*");

                var path = req.RawUrl;
                var ip = req.RemoteEndPoint.Address.ToString();

                this.LogVerbose("POST Request received at path: {path} from host {host}", path, ip);

                var body = new StreamReader(req.InputStream).ReadToEnd();

                if (path.StartsWith("/mc/api/log"))
                {
                    res.StatusCode = 200;
                    res.Close();

                    var logRequest = new HttpRequestMessage(HttpMethod.Post, $"http://{_parent.Config.DirectServer.Logging.Host}:{_parent.Config.DirectServer.Logging.Port}/logs")
                    {
                        Content = new StringContent(body, Encoding.UTF8, "application/json"),
                    };

                    logRequest.Headers.Add("x-pepperdash-host", ip);

                    await LogClient.SendAsync(logRequest);

                    this.LogVerbose("Log data sent to {host}:{port}", _parent.Config.DirectServer.Logging.Host, _parent.Config.DirectServer.Logging.Port);
                }
                else
                {
                    res.StatusCode = 404;
                    res.Close();
                }
            }
            catch (Exception ex)
            {
                this.LogException(ex, "Caught an exception in the OnPost handler");
            }
        }

        private void Server_OnOptions(object sender, HttpRequestEventArgs e)
        {
            try
            {
                var res = e.Response;

                res.AddHeader("Access-Control-Allow-Origin", "*");
                res.AddHeader("Access-Control-Allow-Methods", "GET, POST, OPTIONS");
                res.AddHeader("Access-Control-Allow-Headers", "Content-Type, Accept, X-Requested-With, remember-me");

                res.StatusCode = 200;
                res.Close();
            }
            catch (Exception ex)
            {
                Debug.LogMessage(ex, "Caught an exception in the OnPost handler", this);
            }
        }

        /// <summary>
        /// Handle the request to join the room with a token
        /// </summary>
        /// <param name="req"></param>
        /// <param name="res"></param>
        private void HandleJoinRequest(HttpListenerRequest req, HttpListenerResponse res)
        {
            var qp = req.QueryString;
            var token = qp["token"];

            this.LogVerbose("Join Room Request with token: {token}", token);


            if (UiClients.TryGetValue(token, out UiClientContext clientContext))
            {
                var bridge = _parent.GetRoomBridge(clientContext.Token.RoomKey);

                if (bridge != null)
                {
                    res.StatusCode = 200;
                    res.ContentType = "application/json";

                    // Construct the response object
                    JoinResponse jRes = new JoinResponse
                    {
                        ClientId = token,
                        RoomKey = bridge.RoomKey,
                        SystemUuid = _parent.SystemUuid,
                        RoomUuid = _parent.SystemUuid,
                        Config = _parent.GetConfigWithPluginVersion(),
                        CodeExpires = new DateTime().AddYears(1),
                        UserCode = bridge.UserCode,
                        UserAppUrl = string.Format("http://{0}:{1}/mc/app",
                        CrestronEthernetHelper.GetEthernetParameter(CrestronEthernetHelper.ETHERNET_PARAMETER_TO_GET.GET_CURRENT_IP_ADDRESS, 0),
                        Port),
                        EnableDebug = false
                    };

                    // Serialize to JSON and convert to Byte[]
                    var json = JsonConvert.SerializeObject(jRes);
                    var body = Encoding.UTF8.GetBytes(json);
                    res.ContentLength64 = body.LongLength;

                    // Send the response
                    res.Close(body, true);
                }
                else
                {
                    var message = string.Format("Unable to find bridge with key: {0}", clientContext.Token.RoomKey);
                    res.StatusCode = 404;
                    res.ContentType = "application/json";
                    this.LogVerbose("{message}", message);
                    var body = Encoding.UTF8.GetBytes(message);
                    res.ContentLength64 = body.LongLength;
                    res.Close(body, true);

                }
            }
            else
            {
                var message = "Token invalid or has expired";
                res.StatusCode = 401;
                res.ContentType = "application/json";
                this.LogVerbose("{message}", message);
                var body = Encoding.UTF8.GetBytes(message);
                res.ContentLength64 = body.LongLength;
                res.Close(body, true);
            }
        }

        /// <summary>
        /// Handles a server version request
        /// </summary>
        /// <param name="res"></param>
        private void HandleVersionRequest(HttpListenerResponse res)
        {
            res.StatusCode = 200;
            res.ContentType = "application/json";
            var version = new Version() { ServerVersion = _parent.GetConfigWithPluginVersion().RuntimeInfo.PluginVersion };
            var message = JsonConvert.SerializeObject(version);
            this.LogVerbose("{message}", message);

            var body = Encoding.UTF8.GetBytes(message);
            res.ContentLength64 = body.LongLength;
            res.Close(body, true);
        }

        /// <summary>
        /// Handler to return images requested by the user app
        /// </summary>
        /// <param name="req"></param>
        /// <param name="res"></param>
        private void HandleImageRequest(HttpListenerRequest req, HttpListenerResponse res)
        {
            var path = req.RawUrl;

            Debug.LogMessage(LogEventLevel.Verbose, "Requesting Image: {0}", this, path);

            var imageBasePath = Global.DirectorySeparator + "html" + Global.DirectorySeparator + "logo" + Global.DirectorySeparator;

            var image = path.Split('/').Last();

            var filePath = imageBasePath + image;

            Debug.LogMessage(LogEventLevel.Verbose, "Retrieving Image: {0}", this, filePath);

            if (File.Exists(filePath))
            {
                if (filePath.EndsWith(".png"))
                {
                    res.ContentType = "image/png";
                }
                else if (filePath.EndsWith(".jpg"))
                {
                    res.ContentType = "image/jpeg";
                }
                else if (filePath.EndsWith(".gif"))
                {
                    res.ContentType = "image/gif";
                }
                else if (filePath.EndsWith(".svg"))
                {
                    res.ContentType = "image/svg+xml";
                }
                byte[] contents = File.ReadAllBytes(filePath);
                res.ContentLength64 = contents.LongLength;
                res.Close(contents, true);
            }
            else
            {
                res.StatusCode = (int)HttpStatusCode.NotFound;
                res.Close();
            }
        }

        /// <summary>
        /// Handles requests to serve files for the Angular single page app
        /// </summary>
        /// <param name="req"></param>
        /// <param name="res"></param>
        /// <param name="path"></param>
        private void HandleUserAppRequest(HttpListenerRequest req, HttpListenerResponse res, string path)
        {
            this.LogVerbose("Requesting User app file");

            string filePath = path.Split('?')[0];

            // remove the token from the path if found
            //string filePath = path.Replace(string.Format("?token={0}", token), "");

            // if there's no file suffix strip any extra path data after the base href
            if (filePath != _userAppBaseHref && !filePath.Contains(".") && (!filePath.EndsWith(_userAppBaseHref) || !filePath.EndsWith(_userAppBaseHref += "/")))
            {
                var suffix = filePath.Substring(_userAppBaseHref.Length, filePath.Length - _userAppBaseHref.Length);
                if (suffix != "/")
                {
                    //Debug.Console(2, this, "Suffix: {0}", suffix);
                    filePath = filePath.Replace(suffix, "");
                }
            }

            // swap the base href prefix for the file path prefix
            filePath = filePath.Replace(_userAppBaseHref, _appPath);

            this.LogVerbose("filepath: {filePath}", filePath);


            // append index.html if no specific file is specified
            if (!filePath.Contains("."))
            {
                if (filePath.EndsWith("/"))
                {
                    filePath += "index.html";
                }
                else
                {
                    filePath += "/index.html";
                }
            }

            // Set ContentType based on file type
            if (filePath.EndsWith(".html"))
            {
                this.LogVerbose("Client requesting User App");

                res.ContentType = "text/html";
            }
            else
            {
                if (path.EndsWith(".js"))
                {
                    res.ContentType = "application/javascript";
                }
                else if (path.EndsWith(".css"))
                {
                    res.ContentType = "text/css";
                }
                else if (path.EndsWith(".json"))
                {
                    res.ContentType = "application/json";
                }
            }

            this.LogVerbose("Attempting to serve file: {filePath}", filePath);

            var remoteIp = req.RemoteEndPoint.Address;

            // Check if the request is coming from the CS LAN and if so, send the CS config instead of the LAN config
            if (csSubnetMask != null && csIpAddress != null && remoteIp.IsInSameSubnet(csIpAddress, csSubnetMask) && filePath.Contains(appConfigFileName))
            {
                filePath = filePath.Replace(appConfigFileName, appConfigCsFileName);
            }

            byte[] contents;
            if (File.Exists(filePath))
            {
                this.LogVerbose("File found: {filePath}", filePath);
                contents = File.ReadAllBytes(filePath);
            }
            else
            {
                this.LogVerbose("File not found: {filePath}", filePath);
                res.StatusCode = (int)HttpStatusCode.NotFound;
                res.Close();
                return;
            }

            res.ContentLength64 = contents.LongLength;
            res.Close(contents, true);
        }

        /// <summary>
        /// StopServer method
        /// </summary>
        public void StopServer()
        {
            this.LogVerbose("Stopping WebSocket Server");
            _server.Stop(CloseStatusCode.Normal, "Server Shutting Down");
        }

        /// <summary>
        /// Sends a message to all connectd clients
        /// </summary>
        /// <param name="message"></param>
        /// <summary>
        /// SendMessageToAllClients method
        /// </summary>
        public void SendMessageToAllClients(string message)
        {
            foreach (var clientContext in UiClients.Values)
            {
                if (clientContext.Client != null && clientContext.Client.Context.WebSocket.IsAlive)
                {
                    clientContext.Client.Context.WebSocket.Send(message);
                }
            }
        }

        /// <summary>
        /// Sends a message to a specific client
        /// </summary>
        /// <param name="clientId"></param>
        /// <param name="message"></param>
        /// <summary>
        /// SendMessageToClient method
        /// </summary>
        public void SendMessageToClient(object clientId, string message)
        {
            if (clientId == null)
            {
                return;
            }

            if (UiClients.TryGetValue((string)clientId, out UiClientContext clientContext))
            {
                if (clientContext.Client != null)
                {
                    var socket = clientContext.Client.Context.WebSocket;

                    if (socket.IsAlive)
                    {
                        socket.Send(message);
                    }
                }
            }
            else
            {
                this.LogWarning("Unable to find client with ID: {clientId}", clientId);
            }
        }
    }

    /// <summary>
    /// Represents a Version
    /// </summary>
    public class Version
    {
        [JsonProperty("serverVersion")]
        public string ServerVersion { get; set; }

        [JsonProperty("serverIsRunningOnProcessorHardware")]
        public bool ServerIsRunningOnProcessorHardware { get; private set; }

        public Version()
        {
            ServerIsRunningOnProcessorHardware = true;
        }
    }

    /// <summary>
    /// Represents a UiClientContext
    /// </summary>
    public class UiClientContext
    {
        /// <summary>
        /// Gets or sets the Client
        /// </summary>
        public UiClient Client { get; private set; }
        /// <summary>
        /// Gets or sets the Token
        /// </summary>
        public JoinToken Token { get; private set; }

        public UiClientContext(JoinToken token)
        {
            Token = token;
        }

        /// <summary>
        /// SetClient method
        /// </summary>
        public void SetClient(UiClient client)
        {
            Client = client;
        }

    }

    /// <summary>
    /// Represents a ServerTokenSecrets
    /// </summary>
    public class ServerTokenSecrets
    {
        /// <summary>
        /// Gets or sets the GrantCode
        /// </summary>
        public string GrantCode { get; set; }

        public Dictionary<string, JoinToken> Tokens { get; set; }

        public ServerTokenSecrets(string grantCode)
        {
            GrantCode = grantCode;
            Tokens = new Dictionary<string, JoinToken>();
        }
    }

    /// <summary>
    /// Represents a JoinToken
    /// </summary>
    public class JoinToken
    {
        /// <summary>
        /// Gets or sets the Code
        /// </summary>
        public string Code { get; set; }

        public string RoomKey { get; set; }

        public string Uuid { get; set; }

        public string TouchpanelKey { get; set; } = "";

        /// <summary>
        /// Gets or sets the Token
        /// </summary>
        public string Token { get; set; } = null;
    }

    /// <summary>
    /// Represents a JoinResponse
    /// </summary>
    public class JoinResponse
    {
        [JsonProperty("clientId")]
        /// <summary>
        /// Gets or sets the ClientId
        /// </summary>
        public string ClientId { get; set; }

        [JsonProperty("roomKey")]
        public string RoomKey { get; set; }

        [JsonProperty("systemUUid")]
        public string SystemUuid { get; set; }

        [JsonProperty("roomUUid")]
        /// <summary>
        /// Gets or sets the RoomUuid
        /// </summary>
        public string RoomUuid { get; set; }

        [JsonProperty("config")]
        /// <summary>
        /// Gets or sets the Config
        /// </summary>
        public object Config { get; set; }

        [JsonProperty("codeExpires")]
        /// <summary>
        /// Gets or sets the CodeExpires
        /// </summary>
        public DateTime CodeExpires { get; set; }

        [JsonProperty("userCode")]
        /// <summary>
        /// Gets or sets the UserCode
        /// </summary>
        public string UserCode { get; set; }

        [JsonProperty("userAppUrl")]
        /// <summary>
        /// Gets or sets the UserAppUrl
        /// </summary>
        public string UserAppUrl { get; set; }

        [JsonProperty("enableDebug")]
        /// <summary>
        /// Gets or sets the EnableDebug
        /// </summary>
        public bool EnableDebug { get; set; }
    }
}
