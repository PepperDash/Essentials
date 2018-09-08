using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using Crestron.SimplSharp;
using Crestron.SimplSharp.CrestronIO;
using Crestron.SimplSharp.Reflection;
using Crestron.SimplSharpPro.CrestronThread;
using Crestron.SimplSharp.CrestronWebSocketClient;
using Crestron.SimplSharpPro;
using Crestron.SimplSharp.Net.Http;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

using PepperDash.Core;
using PepperDash.Essentials.Core;
using PepperDash.Essentials.Room.Cotija;

namespace PepperDash.Essentials
{
    public class CotijaSystemController : Device
    {
		WebSocketClient WSClient;

		bool LinkUp;

		/// <summary>
		/// Prevents post operations from stomping on each other and getting lost
		/// </summary>
		CEvent PostLockEvent = new CEvent(true, true);

		CEvent RegisterLockEvent = new CEvent(true, true);

		public CotijaConfig Config { get; private set; }

        Dictionary<string, Object> ActionDictionary = new Dictionary<string, Object>(StringComparer.InvariantCultureIgnoreCase);

        Dictionary<string, CTimer> PushedActions = new Dictionary<string, CTimer>();

        CTimer ServerHeartbeatCheckTimer;

        long ServerHeartbeatInterval = 20000;

        CTimer ServerReconnectTimer;

        long ServerReconnectInterval = 5000;

        string SystemUuid;

		List<CotijaBridgeBase> RoomBridges = new List<CotijaBridgeBase>();

        long ButtonHeartbeatInterval = 1000;

		/// <summary>
		/// Used for tracking HTTP debugging
		/// </summary>
		bool HttpDebugEnabled;
		
		/// <summary>
		/// 
		/// </summary>
		/// <param name="key"></param>
		/// <param name="name"></param>
		/// <param name="config"></param>
        public CotijaSystemController(string key, string name, CotijaConfig config) : base(key, name)
        {
            Config = config;
			Debug.Console(0, this, "Mobile UI controller initializing for server:{0}", config.ServerUrl);

			CrestronConsole.AddNewConsoleCommand(AuthorizeSystem,
				"mobileauth", "Authorizes system to talk to cotija server", ConsoleAccessLevelEnum.AccessOperator);
			CrestronConsole.AddNewConsoleCommand(s => ShowInfo(),
				"mobileinfo", "Shows information for current mobile control session", ConsoleAccessLevelEnum.AccessOperator);
			CrestronConsole.AddNewConsoleCommand(s => { 
					s = s.Trim();
					if(!string.IsNullOrEmpty(s))
					{
						HttpDebugEnabled = (s.Trim() != "0"); 
					}
					CrestronConsole.ConsoleCommandResponse("HTTP Debug {0}", HttpDebugEnabled ? "Enabled" : "Disabled");
				},
				"mobilehttpdebug", "1 enables more verbose HTTP response debugging", ConsoleAccessLevelEnum.AccessOperator);
			CrestronConsole.AddNewConsoleCommand(TestHttpRequest,
			"mobilehttprequest", "Tests an HTTP get to URL given", ConsoleAccessLevelEnum.AccessOperator);

            CrestronConsole.AddNewConsoleCommand(PrintActionDictionaryPaths, "showactionpaths", "Prints the paths in teh Action Dictionary", ConsoleAccessLevelEnum.AccessOperator);


            CrestronEnvironment.ProgramStatusEventHandler += new ProgramStatusEventHandler(CrestronEnvironment_ProgramStatusEventHandler);
			CrestronEnvironment.EthernetEventHandler += new EthernetEventHandler(CrestronEnvironment_EthernetEventHandler);
				
        }

		/// <summary>
		/// 
		/// </summary>
		/// <param name="ethernetEventArgs"></param>
		void CrestronEnvironment_EthernetEventHandler(EthernetEventArgs ethernetEventArgs)
		{
			Debug.Console(1, this, Debug.ErrorLogLevel.Warning, "Ethernet status change, port {0}: {1}",
				ethernetEventArgs.EthernetAdapter, ethernetEventArgs.EthernetEventType);

			if (ethernetEventArgs.EthernetEventType == eEthernetEventType.LinkDown)
			{
				LinkUp = false;
			}
		}

        /// <summary>
        /// Sends message to server to indicate the system is shutting down
        /// </summary>
        /// <param name="programEventType"></param>
        void CrestronEnvironment_ProgramStatusEventHandler(eProgramStatusEventType programEventType)
        {
            if (programEventType == eProgramStatusEventType.Stopping && WSClient.Connected)
            {
                SendMessageToServer(JObject.FromObject( new
                {
                    type = "/system/close"
                }));

            }
        }

        public void PrintActionDictionaryPaths(object o)
        {
            Debug.Console(0, this, "ActionDictionary Contents:");

            foreach (var item in ActionDictionary)
            {
                Debug.Console(0, this, "{0}", item.Key);
            }
        }

        /// <summary>
        /// Adds an action to the dictionary
        /// </summary>
        /// <param name="key">The path of the API command</param>
        /// <param name="action">The action to be triggered by the commmand</param>
        public void AddAction(string key, object action)
        {
            if (!ActionDictionary.ContainsKey(key))
            {
                ActionDictionary.Add(key, action);
            }
            else
            {
                Debug.Console(1, this, "Cannot add action with key '{0}' because key already exists in ActionDictionary.", key);
            }
        }

        /// <summary>
        /// Removes an action from the dictionary
        /// </summary>
        /// <param name="key"></param>
        public void RemoveAction(string key)
        {
            if (ActionDictionary.ContainsKey(key))
                ActionDictionary.Remove(key);
        }

		/// <summary>
		/// 
		/// </summary>
		/// <param name="bridge"></param>
		public void AddBridge(CotijaBridgeBase bridge)
		{
			RoomBridges.Add(bridge);
			var b = bridge as IDelayedConfiguration;
			if (b != null)
			{
				Debug.Console(0, this, "Adding room bridge with delayed configuration");
				b.ConfigurationIsReady += new EventHandler<EventArgs>(bridge_ConfigurationIsReady);
			}
			else
			{
				Debug.Console(0, this, "Adding room bridge and sending configuration");
				RegisterSystemToServer();
			}
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		void bridge_ConfigurationIsReady(object sender, EventArgs e)
		{
			Debug.Console(1, this, "Bridge ready.  Registering");
			// send the configuration object to the server
			RegisterSystemToServer();
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="o"></param>
        void ReconnectToServerTimerCallback(object o)
        {
            RegisterSystemToServer();
        }

		/// <summary>
		/// Verifies system connection with servers
		/// </summary>
		/// <param name="command"></param>
		void AuthorizeSystem(string code)
		{
			if (string.IsNullOrEmpty(SystemUuid))
			{
				CrestronConsole.ConsoleCommandResponse("System does not have a UUID. Please ensure proper portal-format configuration is loaded and restart.");
				return;
			}

			if (string.IsNullOrEmpty(code))
			{
				CrestronConsole.ConsoleCommandResponse("Please enter a user code to authorize a system");
				return;
			}

			var req = new HttpClientRequest();
			string url = string.Format("http://{0}/api/system/grantcode/{1}/{2}", Config.ServerUrl, code, SystemUuid);
			Debug.Console(0, this, "Authorizing to: {0}", url);

			if (string.IsNullOrEmpty(Config.ServerUrl))
			{
				CrestronConsole.ConsoleCommandResponse("Config URL address is not set.  Check portal configuration");
				return;
			}
			try
			{
				req.Url.Parse(url);
				new HttpClient().DispatchAsync(req, (r, e) =>
				{
					CheckHttpDebug(r, e);
					if (e == HTTP_CALLBACK_ERROR.COMPLETED)
					{
						if (r.Code == 200)
						{
							Debug.Console(0, "System authorized, sending config.");
							RegisterSystemToServer();
						}
						else if (r.Code == 404)
						{
							if (r.ContentString.Contains("codeNotFound"))
							{
								Debug.Console(0, "Authorization failed, code not found for system UUID {0}", SystemUuid);
							}
							else if (r.ContentString.Contains("uuidNotFound"))
							{
								Debug.Console(0, "Authorization failed, uuid {0} not found. Check Essentials configuration is correct",
									SystemUuid);
							}
						}
					}
					else
						Debug.Console(0, this, "Error {0} in authorizing system", e);
				});
			}
			catch (Exception e)
			{
				Debug.Console(0, this, "Error in authorizing: {0}", e);
			}
		}

		/// <summary>
		/// Dumps info in response to console command.
		/// </summary>
		void ShowInfo()
		{
			var url = Config != null ? Config.ServerUrl : "No config";
			string name;
			string code;
			if (RoomBridges != null && RoomBridges.Count > 0)
			{
				name = RoomBridges[0].RoomName;
				code = RoomBridges[0].UserCode;
			}
			else
			{
				name = "No config";
				code = "Not available";
			}
			var conn = WSClient == null ? "No client" : (WSClient.Connected ? "Yes" : "No");

			CrestronConsole.ConsoleCommandResponse(@"Mobile Control Information:
	Server address: {0}
	System Name: {1}
	System UUID: {2}
	System User code: {3}
	Connected?: {4}", url, name, SystemUuid, 
					code, conn);
		}

        /// <summary>
        /// Registers the room with the server
        /// </summary>
        /// <param name="url">URL of the server, including the port number, if not 80.  Format: "serverUrlOrIp:port"</param>
        void RegisterSystemToServer()
        {
			var ready = RegisterLockEvent.Wait(20000);
			if (!ready)
			{
				Debug.Console(1, this, "RegisterSystemToServer failed to enter after 20 seconds.  Ignoring");
				return;
			}
			RegisterLockEvent.Reset();

			try
            {
				var confObject = ConfigReader.ConfigObject;
                confObject.Info.RuntimeInfo.AppName = Assembly.GetExecutingAssembly().GetName().Name;
                var version = Assembly.GetExecutingAssembly().GetName().Version;
                confObject.Info.RuntimeInfo.AssemblyVersion = string.Format("{0}.{1}.{2}", version.Major, version.Minor, version.Build);

				string postBody = JsonConvert.SerializeObject(confObject);
				SystemUuid = confObject.SystemUuid;

                if (string.IsNullOrEmpty(postBody))
                {
                    Debug.Console(1, this, "ERROR: Config body is empty. Cannot register with server.");
                }
                else
                {
					var regClient = new HttpClient();
					regClient.Verbose = true;
					regClient.KeepAlive = true;

					string url = string.Format("http://{0}/api/system/join/{1}", Config.ServerUrl, SystemUuid);
					Debug.Console(1, this, "Joining server at {0}", url);

                    HttpClientRequest request = new HttpClientRequest();
                    request.Url.Parse(url);
                    request.RequestType = RequestType.Post;
                    request.Header.SetHeaderValue("Content-Type", "application/json");
                    request.ContentString = postBody;
					try
					{
						regClient.DispatchAsync(request, RegistrationConnectionCallback);
					}
					catch (Exception e)
					{
						Debug.Console(1, this, "Cannot register with app server: {0}", e);
					}
                }

            }
            catch (Exception e)
            {
                Debug.Console(0, this, "ERROR: Initilizing app server controller: {0}", e);
				RegisterLockEvent.Set();
				StartReconnectTimer();
            }

        }

        /// <summary>
        /// Sends a message to the server from a room
        /// </summary>
        /// <param name="room">room from which the message originates</param>
        /// <param name="o">object to be serialized and sent in post body</param>
        public void SendMessageToServer(JObject o)
        {

            if (WSClient != null && WSClient.Connected)
            {
                string message = JsonConvert.SerializeObject(o, Formatting.None, new JsonSerializerSettings { NullValueHandling = NullValueHandling.Ignore });
				Debug.Console(1, this, "Message TX: {0}", message);
                var messageBytes = System.Text.Encoding.UTF8.GetBytes(message);
				//WSClient.Send(messageBytes, (uint)messageBytes.Length, WebSocketClient.WEBSOCKET_PACKET_TYPES.LWS_WS_OPCODE_07__TEXT_FRAME);
				WSClient.SendAsync(messageBytes, (uint)messageBytes.Length, WebSocketClient.WEBSOCKET_PACKET_TYPES.LWS_WS_OPCODE_07__TEXT_FRAME);
            }
 
        }

        /// <summary>
        /// Disconnects the SSE Client and stops the heartbeat timer
        /// </summary>
        /// <param name="command"></param>
        void DisconnectWebsocketClient()
        {
			Debug.Console(1, this, Debug.ErrorLogLevel.Notice, "Disconnecting websocket");
			if (WSClient != null) // && WSClient.Connected)
			{
				WSClient.Disconnect();
				WSClient.SendCallBack = null;
				WSClient.ReceiveCallBack = null;
				WSClient.ConnectionCallBack = null;
				WSClient.Dispose();
				WSClient = null;
			}
        }
        
        /// <summary>
        /// The callback that fires when we get a response from our registration attempt
        /// </summary>
        /// <param name="resp"></param>
        /// <param name="err"></param>
        void RegistrationConnectionCallback(HttpClientResponse resp, HTTP_CALLBACK_ERROR err)
        {
			CheckHttpDebug(resp, err);
			Debug.Console(1, this, "RegistrationConnectionCallback: {0}", err);
            try
            {
                if (resp != null && resp.Code == 200)
                {
                    if(ServerReconnectTimer != null)
                    {
                        ServerReconnectTimer.Stop();
                        ServerReconnectTimer = null;
                    }

					// Success here!
                    ConnectStreamClient();
                }
                else
                {
					if (resp != null)
					{
						if (resp.Code == 502)
						{
							Debug.Console(1, this, "Cannot reach App Server behind web server. Check that service/app is running on server");
						}
						else
						{
							Debug.Console(1, this, "Error response from server: {0}\n{1}", resp.Code, err);
						}
					}
					else
					{
						Debug.Console(1, this, "No response. Server is likely unreachable");
					}
					StartReconnectTimer();
                }
            }
            catch (Exception e)
            {
                Debug.Console(1, this, "Error Initializing Stream Client: {0}", e);
				StartReconnectTimer();
            }
			RegisterLockEvent.Set();
        }

        /// <summary>
        /// Executes when we don't get a heartbeat message in time.  Triggers reconnect.
        /// </summary>
        /// <param name="o">For CTimer callback. Not used</param>
        void HeartbeatExpiredTimerCallback(object o)
        {
			Debug.Console(1, this, "Heartbeat Timer Expired.");
			if (ServerHeartbeatCheckTimer != null)
            {
                ServerHeartbeatCheckTimer.Stop();
                ServerHeartbeatCheckTimer = null;
            }
			DisconnectWebsocketClient();
            StartReconnectTimer();
        }

		/// <summary>
		/// 
		/// </summary>
		/// <param name="dueTime"></param>
		/// <param name="repeatTime"></param>
        void StartReconnectTimer()
        {
            // Start the reconnect timer
			if (ServerReconnectTimer == null)
			{
				ServerReconnectTimer = new CTimer(ReconnectToServerTimerCallback, null, ServerReconnectInterval, ServerReconnectInterval);
				Debug.Console(1, this, "Reconnect Timer Started.");
			}
			ServerReconnectTimer.Reset(ServerReconnectInterval, ServerReconnectInterval);
        }

		/// <summary>
		/// 
		/// </summary>
		/// <param name="dueTime"></param>
		/// <param name="repeatTime"></param>
        void ResetOrStartHearbeatTimer()
        {
            if (ServerHeartbeatCheckTimer == null)
            {
				ServerHeartbeatCheckTimer = new CTimer(HeartbeatExpiredTimerCallback, null, ServerHeartbeatInterval, ServerHeartbeatInterval);

                Debug.Console(1, this, "Heartbeat Timer Started.");
            }

			ServerHeartbeatCheckTimer.Reset(ServerHeartbeatInterval, ServerHeartbeatInterval);
        }


#warning notes here

		/*
		 * Need to understand why this is being sent from a server that should not have been closing,
		 * and also why essentials is not recovering
		 * 
		 [17:36:09.044]App 10:[appServer] Joining server at http://bosd-node01.pepperdash.net/mobilecontrolapi/api/system/join/02cf80a1-ce35-482c-8fa6-286457ff2826
[17:36:13.086]App 10:[appServer] RegistrationConnectionCallback: COMPLETED
[17:36:13.088]App 10:[appServer] Response from server: 502COMPLETED
[17:36:18.113]App 10:[appServer] Joining server at http://bosd-node01.pepperdash.net/mobilecontrolapi/api/system/join/02cf80a1-ce35-482c-8fa6-286457ff2826
[17:36:22.159]App 10:[appServer] RegistrationConnectionCallback: COMPLETED
[17:36:22.160]App 10:[appServer] Response from server: 502COMPLETED
[17:36:27.005]App 10:[ciscoSparkCodec-1] Retrieving Booking Info from Codec. Current Time: 9/6/2018 5:36:27 PM
[17:36:27.187]App 10:[appServer] Joining server at http://bosd-node01.pepperdash.net/mobilecontrolapi/api/system/join/02cf80a1-ce35-482c-8fa6-286457ff2826
[17:36:31.233]App 10:[appServer] RegistrationConnectionCallback: COMPLETED
[17:36:31.234]App 10:[appServer] Response from server: 502COMPLETED
[17:36:36.338]App 10:[appServer] Joining server at http://bosd-node01.pepperdash.net/mobilecontrolapi/api/system/join/02cf80a1-ce35-482c-8fa6-286457ff2826
[17:36:42.151]App 10:[appServer] RegistrationConnectionCallback: COMPLETED
[17:36:42.153]App 10:[appServer] Initializing Stream client to server.
[17:36:42.887]App 10:[appServer] Websocket connected
[17:36:42.916]App 10:[appServer] Joining server at http://bosd-node01.pepperdash.net/mobilecontrolapi/api/system/join/02cf80a1-ce35-482c-8fa6-286457ff2826
[17:36:42.978]App 10:[appServer] RegistrationConnectionCallback: COMPLETED
[17:36:42.979]App 10:[appServer] Initializing Stream client to server.
[17:36:42.980]App 10:[appServer] Websocket connected
[17:36:42.990]App 10:[appServer] Message RX: '{"type":"close"}'
		 */


		/// <summary>
        /// Connects the Websocket Client
        /// </summary>
        /// <param name="o"></param>
        void ConnectStreamClient()
        {
            Debug.Console(0, this, "Initializing Stream client to server.");

			if (WSClient != null)
			{
				DisconnectWebsocketClient();
			}

			WSClient = new WebSocketClient();
			WSClient.URL = string.Format("wss://{0}/system/join/{1}", Config.ServerUrl, this.SystemUuid);
			WSClient.ConnectionCallBack = ConnectCallback;
			WSClient.DisconnectCallBack = DisconnectCallback;
			WSClient.Connect();
			Debug.Console(1, this, "Websocket connected");
			WSClient.SendCallBack = WebsocketSendCallback;
			WSClient.ReceiveCallBack = WebsocketReceiveCallback;
			WSClient.ReceiveAsync();
        }

		/// <summary>
		/// Waits two and goes again
		/// </summary>
		void ReconnectStreamClient()
		{
			new CTimer(o => ConnectStreamClient(), 2000);
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="code"></param>
		/// <returns></returns>
		int ConnectCallback(WebSocketClient.WEBSOCKET_RESULT_CODES code)
		{
			Debug.Console(1, this, Debug.ErrorLogLevel.Notice, "Websocket status change: {0}", code);
			if (code != WebSocketClient.WEBSOCKET_RESULT_CODES.WEBSOCKET_CLIENT_SUCCESS)
			{
				Debug.Console(1, this, "Web socket connection failed: {0}", code);
				ReconnectStreamClient();
			}
			return 0;
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="code"></param>
		/// <returns></returns>
		int DisconnectCallback(WebSocketClient.WEBSOCKET_RESULT_CODES code, object o)
		{
			Debug.Console(1, this, Debug.ErrorLogLevel.Warning, "Websocket disconnected with code: {0}", code);
			ReconnectStreamClient();
			return 0;
		}


		/// <summary>
		/// Resets reconnect timer and updates usercode
		/// </summary>
		/// <param name="content"></param>
		void HandleHeartBeat(JToken content)
		{
            SendMessageToServer(JObject.FromObject(new
            {
                type = "/system/heartbeatAck"
            }));

			var code = content["userCode"];
			if(code != null) 
			{
				foreach (var b in RoomBridges)
				{
					b.SetUserCode(code.Value<string>());
				}
			}
			ResetOrStartHearbeatTimer();
		}

		/// <summary>
		/// Outputs debug info when enabled
		/// </summary>
		/// <param name="req"></param>
		/// <param name="r"></param>
		/// <param name="e"></param>
		void CheckHttpDebug(HttpClientResponse r, HTTP_CALLBACK_ERROR e)
		{
			if (HttpDebugEnabled)
			{
				try
				{
					Debug.Console(0, this, "------ Begin HTTP Debug ---------------------------------------");
					if (r != null)
					{
						Debug.Console(0, this, "HTTP Response URL: {0}", r.ResponseUrl != null ? r.ResponseUrl.ToString() : "NONE");
						Debug.Console(0, this, "HTTP Response code: {0}", r.Code);
						Debug.Console(0, this, "HTTP Response content: \r{0}", r.ContentString);
					}
					else
					{
						Debug.Console(0, this, "No HTTP response");
					}
					Debug.Console(0, this, "HTTP Response 'error' {0}", e);
					Debug.Console(0, this, "------ End HTTP Debug -----------------------------------------");
				}
				catch (Exception ex)
				{
					Debug.Console(0, this, "HttpDebugError: {0}", ex);
				}
			}
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="data"></param>
		/// <param name="length"></param>
		/// <param name="opcode"></param>
		/// <param name="err"></param>
		int WebsocketReceiveCallback(byte[] data, uint length, WebSocketClient.WEBSOCKET_PACKET_TYPES opcode,
			WebSocketClient.WEBSOCKET_RESULT_CODES err)
		{
			var rx = System.Text.Encoding.UTF8.GetString(data, 0, (int)length);
			if(rx.Length > 0)
				ParseStreamRx(rx);
			WSClient.ReceiveAsync();
			return 0;
		}

        /// <summary>
        /// Callback to catch possible errors in sending via the websocket
        /// </summary>
        /// <param name="result"></param>
        /// <returns></returns>
        int WebsocketSendCallback(Crestron.SimplSharp.CrestronWebSocketClient.WebSocketClient.WEBSOCKET_RESULT_CODES result)
        {
			if(result != WebSocketClient.WEBSOCKET_RESULT_CODES.WEBSOCKET_CLIENT_SUCCESS)
	            Debug.Console(1, this, Debug.ErrorLogLevel.Notice, "SendCallback questionable result: {0}", result);
		

            return 0;
        }

		/// <summary>
		/// 
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
        void ParseStreamRx(string message)
		{
            if(string.IsNullOrEmpty(message))
                return;

            Debug.Console(1, this, "Message RX: '{0}'", message);
            try
            {
                var messageObj = JObject.Parse(message);

                var type = messageObj["type"].Value<string>();

                if (type == "hello")
                {
                    ResetOrStartHearbeatTimer();
                }
				else if (type == "/system/heartbeat")
				{
					HandleHeartBeat(messageObj["content"]);
				}
				else if (type == "close")
				{
					DisconnectWebsocketClient();

					if(ServerHeartbeatCheckTimer != null)
						ServerHeartbeatCheckTimer.Stop();
					// Start the reconnect timer
					StartReconnectTimer();
				}
				else
				{
					// Check path against Action dictionary
					if (ActionDictionary.ContainsKey(type))
					{
						var action = ActionDictionary[type];

						if (action is Action)
						{
							(action as Action)();
						}
						else if (action is PressAndHoldAction)
						{
							var stateString = messageObj["content"]["state"].Value<string>();

							// Look for a button press event
							if (!string.IsNullOrEmpty(stateString))
							{
								switch (stateString)
								{
									case "true":
										{
											if (!PushedActions.ContainsKey(type))
											{
												PushedActions.Add(type, new CTimer(o =>
												{
													(action as PressAndHoldAction)(false);
													PushedActions.Remove(type);
												}, null, ButtonHeartbeatInterval, ButtonHeartbeatInterval));
											}
											// Maybe add an else to reset the timer
											break;
										}
									case "held":
										{
											if (PushedActions.ContainsKey(type))
											{
												PushedActions[type].Reset(ButtonHeartbeatInterval, ButtonHeartbeatInterval);
											}
											return;
										}
									case "false":
										{
											if (PushedActions.ContainsKey(type))
											{
												PushedActions[type].Stop();
												PushedActions.Remove(type);
											}
											break;
										}
								}

								(action as PressAndHoldAction)(stateString == "true");
							}
						}
						else if (action is Action<bool>)
						{
							var stateString = messageObj["content"]["state"].Value<string>();

							if (!string.IsNullOrEmpty(stateString))
							{
								(action as Action<bool>)(stateString == "true");
							}
						}
						else if (action is Action<ushort>)
						{
							(action as Action<ushort>)(messageObj["content"]["value"].Value<ushort>());
						}
						else if (action is Action<string>)
						{
							(action as Action<string>)(messageObj["content"]["value"].Value<string>());
						}
						else if (action is Action<SourceSelectMessageContent>)
						{
							(action as Action<SourceSelectMessageContent>)(messageObj["content"]
								.ToObject<SourceSelectMessageContent>());
						}
					}
					else
					{
						Debug.Console(1, this, "-- Warning: Incoming message has no registered handler");
					}
				}
            }
            catch (Exception err)
            {
                //Debug.Console(1, "SseMessageLengthBeforeFailureCount: {0}", SseMessageLengthBeforeFailureCount);
                //SseMessageLengthBeforeFailureCount = 0;
                Debug.Console(1, this, "Unable to parse message: {0}", err);	
            }
        }

		void TestHttpRequest(string s)
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
					if (tokens[0].ToLower() == "get")
					{
						var resp = new HttpClient().Get(url);
						CrestronConsole.ConsoleCommandResponse("RESPONSE:\r{0}\r\r", resp);
					}
					else if (tokens[0].ToLower() == "post")
					{
						var resp = new HttpClient().Post(url, new byte[] { });
						CrestronConsole.ConsoleCommandResponse("RESPONSE:\r{0}\r\r", resp);
					}

					else
					{
						CrestronConsole.ConsoleCommandResponse("Only get or post supported\r");
						PrintTestHttpRequestUsage();
					}
				}
				catch (HttpException e)
				{
					CrestronConsole.ConsoleCommandResponse("Exception in request:\r");
					CrestronConsole.ConsoleCommandResponse("Response URL: {0}\r", e.Response.ResponseUrl);
					CrestronConsole.ConsoleCommandResponse("Response Error Code: {0}\r", e.Response.Code);
					CrestronConsole.ConsoleCommandResponse("Response body: {0}\r", e.Response.ContentString);
				}

			}
		}

		void PrintTestHttpRequestUsage()
		{
			CrestronConsole.ConsoleCommandResponse("Usage: mobilehttprequest:N get/post url\r");
		}
    }
}