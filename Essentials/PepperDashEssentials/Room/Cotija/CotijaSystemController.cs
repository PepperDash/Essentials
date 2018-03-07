using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using Crestron.SimplSharp;
using Crestron.SimplSharp.CrestronIO;
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
		int SseMessageLengthBeforeFailureCount;

		WebSocketClient WSClient;

		//GenericHttpSseClient SseClient;

		/// <summary>
		/// Prevents post operations from stomping on each other and getting lost
		/// </summary>
		CEvent PostLockEvent = new CEvent(true, true);

		public CotijaConfig Config { get; private set; }

		HttpClient Client;

        Dictionary<string, Object> ActionDictionary = new Dictionary<string, Object>(StringComparer.InvariantCultureIgnoreCase);

        Dictionary<string, CTimer> PushedActions = new Dictionary<string, CTimer>();

        CTimer ServerHeartbeatCheckTimer;

        long ServerHeartbeatInterval = 20000;

        CTimer ServerReconnectTimer;

        long ServerReconnectInterval = 5000;

        string SystemUuid;

		List<CotijaBridgeBase> RoomBridges = new List<CotijaBridgeBase>();

        long ButtonHeartbeatInterval = 1000;

		bool NeedNewClient;

		/// <summary>
		/// Used to count retries in PostToServer
		/// </summary>
		int RetryCounter;
		
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

            CrestronConsole.AddNewConsoleCommand(DisconnectStreamClient, 
				"CloseHttpClient", "Closes the active HTTP client", ConsoleAccessLevelEnum.AccessOperator);
			CrestronConsole.AddNewConsoleCommand(AuthorizeSystem,
				"cotijaauth", "Authorizes system to talk to cotija server", ConsoleAccessLevelEnum.AccessOperator);

			//AddPostActivationAction(() => RegisterSystemToServer());
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
			if (string.IsNullOrEmpty(code))
			{
				CrestronConsole.ConsoleCommandResponse("Please enter a user code to authorize a system");
				return;
			}


			var req = new HttpClientRequest();
			string url = string.Format("http://{0}/api/system/grantcode/{1}", Config.ServerUrl, code);
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
					if (r.Code == 200)
					{
						Debug.Console(0, this, "System authorized, sending config.");
						RegisterSystemToServer();
					}
					else
						Debug.Console(0, this, "HTTP Error {0} in authorizing system", r.Code);
				});
			}
			catch (Exception e)
			{
				Debug.Console(0, this, "Error in authorizing: {0}", e);
			}
		}

        /// <summary>
        /// Registers the room with the server
        /// </summary>
        /// <param name="url">URL of the server, including the port number, if not 80.  Format: "serverUrlOrIp:port"</param>
        void RegisterSystemToServer()
        {
			try
            {
				var confObject = ConfigReader.ConfigObject;
				string postBody = JsonConvert.SerializeObject(confObject);
				SystemUuid = confObject.SystemUuid;

                if (string.IsNullOrEmpty(postBody))
                {
                    Debug.Console(1, this, "ERROR: Config post body is empty. Cannot register with server.");
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

					var err = regClient.DispatchAsync(request, PostConnectionCallback);
                }

            }
            catch (Exception e)
            {
                Debug.Console(0, this, "ERROR: Initilizing Room: {0}", e);
            }

        }

        /// <summary>
        /// Posts a message to the server from a room
        /// </summary>
        /// <param name="room">room from which the message originates</param>
        /// <param name="o">object to be serialized and sent in post body</param>
        public void PostToServer(JObject o)
        {
			CrestronInvoke.BeginInvoke(oo => 
			{
				if (string.IsNullOrEmpty(SystemUuid))
				{
					Debug.Console(1, this, "Status post attempt before UUID is set. Ignoring.");
					return;
				}
				var ready = PostLockEvent.Wait(2000);
				if (!ready)
				{
					Debug.Console(1, this, "PostToServer failed to enter after 2 seconds.  Ignoring");
					return;
				}

				PostLockEvent.Reset();
				try
				{
					if (Client == null || NeedNewClient)
					{
						NeedNewClient = false;
						Client = new HttpClient();
					}
					Client.Verbose = false;
					Client.KeepAlive = true;

					HttpClientRequest request = new HttpClientRequest();
					request.RequestType = RequestType.Post;
					string url = string.Format("http://{0}/api/system/{1}/status", Config.ServerUrl, SystemUuid);
					request.Url.Parse(url);
					request.KeepAlive = true;
					request.Header.ContentType = "application/json";
					// Ignore any null objects when serializing and remove formatting
					string ignored = JsonConvert.SerializeObject(o, Formatting.None, new JsonSerializerSettings { NullValueHandling = NullValueHandling.Ignore });
					Debug.Console(1, this, "Posting to '{0}':\n{1}", url, ignored);
					request.ContentString = ignored;
					request.FinalizeHeader();
					Client.DispatchAsync(request, (r, err) =>
					{
						Debug.Console(1, this, "POST result: {0}", err);

						if (err == HTTP_CALLBACK_ERROR.COMPLETED)
						{
							Debug.Console(1, this, "Status Response Code: {0}", r.Code);
							PostLockEvent.Set();
							RetryCounter = 0;
						}
						else
						{
							// Try again.  This client is hosed.
							NeedNewClient = true;
							RetryCounter++;
							// instant retry on first try.
							if (RetryCounter >= 2 && RetryCounter < 5)
								CrestronEnvironment.Sleep(1000);
							else if (RetryCounter >= 5 && RetryCounter <= 10)
								CrestronEnvironment.Sleep(5000);
							// give up 
							else if (RetryCounter > 10)
							{
								Debug.Console(1, this, "Giving up on server POST");
								RetryCounter = 0;
								return;
							}
							Debug.Console(1, this, "POST retry #{0}", RetryCounter);
							PostLockEvent.Set();
							PostToServer(o);
						}
					});
				}
				catch (Exception e)
				{
					Debug.Console(1, this, "Error Posting to Server: {0}", e);
					PostLockEvent.Set();
				}

			});

        }

        /// <summary>
        /// Disconnects the SSE Client and stops the heartbeat timer
        /// </summary>
        /// <param name="command"></param>
        void DisconnectStreamClient(string command)
        {
			//if(SseClient != null)
			//    SseClient.Disconnect();

			if (WSClient != null && WSClient.Connected)
				WSClient.Disconnect();

            if (ServerHeartbeatCheckTimer != null)
            {
                ServerHeartbeatCheckTimer.Stop();

                ServerHeartbeatCheckTimer = null;
            }
        }
        
        /// <summary>
        /// The callback that fires when we get a response from our registration attempt
        /// </summary>
        /// <param name="resp"></param>
        /// <param name="err"></param>
        void PostConnectionCallback(HttpClientResponse resp, HTTP_CALLBACK_ERROR err)
        {
			Debug.Console(1, this, "PostConnectionCallback: {0}", err);
            try
            {
                if (resp != null && resp.Code == 200)
                {
                    if(ServerReconnectTimer != null)
                    {
                        ServerReconnectTimer.Stop();
                        ServerReconnectTimer = null;
                    }

                    ConnectStreamClient(null);
                }
                else
                {
					if (resp != null)
						Debug.Console(1, this, "Response from server: {0}\n{1}", resp.Code, err);
					else
					{
						Debug.Console(1, this, "Null response received from server.");
						NeedNewClient = true;
					}
                }
            }
            catch (Exception e)
            {
                Debug.Console(1, this, "Error Initializing Stream Client: {0}", e);
            }
        }

        /// <summary>
        /// Executes when we don't get a heartbeat message in time.  Triggers reconnect.
        /// </summary>
        /// <param name="o"></param>
        void HeartbeatExpiredTimerCallback(object o)
        {
			Debug.Console(1, this, "Heartbeat Timer Expired.");
			if (ServerHeartbeatCheckTimer != null)
            {
                ServerHeartbeatCheckTimer.Stop();
                ServerHeartbeatCheckTimer = null;
            }
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


        /// <summary>
        /// Connects the SSE Client
        /// </summary>
        /// <param name="o"></param>
        void ConnectStreamClient(object o)
        {
            Debug.Console(0, this, "Initializing Stream client to server.");

			if (WSClient == null)
			{
				WSClient = new WebSocketClient();
				WSClient.URL = string.Format("wss://{0}/system/join/{1}", Config.ServerUrl, this.SystemUuid);
				WSClient.Connect();
				Debug.Console(0, this, "Websocket connected");
				WSClient.ReceiveCallBack = WebsocketReceive;
				WSClient.ReceiveAsync();
			}


			//// **********************************
			//if (SseClient == null)
			//{
			//    SseClient = new GenericHttpSseClient(string.Format("{0}-SseClient", Key), Name);

			//    CommunicationGather LineGathered = new CommunicationGather(SseClient, "\x0d\x0a");

			//    LineGathered.LineReceived += new EventHandler<GenericCommMethodReceiveTextArgs>(SSEClient_LineReceived);
			//}
			//else
			//{
			//    if (SseClient.IsConnected)
			//    {
			//        SseClient.Disconnect();
			//    }
			//}

            string uuid = Essentials.ConfigReader.ConfigObject.SystemUuid;

			//SseClient.Url = string.Format("http://{0}/api/system/stream/{1}", Config.ServerUrl, uuid);

			//SseClient.Connect();

			// ***********************************
        }


		/// <summary>
		/// Resets reconnect timer and updates usercode
		/// </summary>
		/// <param name="content"></param>
		void HandleHeartBeat(JToken content)
		{
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
		/// 
		/// </summary>
		/// <param name="data"></param>
		/// <param name="length"></param>
		/// <param name="opcode"></param>
		/// <param name="err"></param>
		int WebsocketReceive(byte[] data, uint length, WebSocketClient.WEBSOCKET_PACKET_TYPES opcode,
			WebSocketClient.WEBSOCKET_RESULT_CODES err)
		{
			var rx = System.Text.Encoding.UTF8.GetString(data, 0, (int)length);

			Debug.Console(0, this, "WS RECEIVED {0}", rx);
			WSClient.ReceiveAsync();
			return 1;
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
        void SSEClient_LineReceived(object sender, GenericCommMethodReceiveTextArgs e)
        {
            if(e.Text.IndexOf("data:") > -1)
            {
				SseMessageLengthBeforeFailureCount += e.Text.Length;

                var message = e.Text.Substring(6);

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
						WSClient.Disconnect();
						//SseClient.Disconnect();

						ServerHeartbeatCheckTimer.Stop();
						// Start the reconnect timer
						StartReconnectTimer();
						//ServerReconnectTimer = new CTimer(ConnectSseClient, null, ServerReconnectInterval, ServerReconnectInterval);
						//ServerReconnectTimer.Reset(ServerReconnectInterval, ServerReconnectInterval);
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
												if (!PushedActions.ContainsKey(type))
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
					Debug.Console(1, "SseMessageLengthBeforeFailureCount: {0}", SseMessageLengthBeforeFailureCount);
					SseMessageLengthBeforeFailureCount = 0;
                    Debug.Console(1, this, "Unable to parse message: {0}", err);	
                }
            }
        }
    }
}