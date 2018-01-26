using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using Crestron.SimplSharp;
using Crestron.SimplSharp.CrestronIO;
using Crestron.SimplSharpPro.CrestronThread;
using Crestron.SimplSharpPro;
using Crestron.SimplSharp.Net.Http;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using PepperDash.Core;
using PepperDash.Essentials.Core;

namespace PepperDash.Essentials
{
    public class CotijaSystemController : Device
    {
        GenericHttpSseClient SseClient;

        CCriticalSection FileLock;

		/// <summary>
		/// Prevents post operations from stomping on each other and getting lost
		/// </summary>
		CEvent PostLockEvent = new CEvent(true, true);

        CotijaConfig Config;

		HttpClient Client;

        Dictionary<string, Object> ActionDictionary = new Dictionary<string, Object>(StringComparer.InvariantCultureIgnoreCase);

        Dictionary<string, CTimer> PushedActions = new Dictionary<string, CTimer>();

        CTimer ServerHeartbeatCheckTimer;

        long ServerHeartbeatInterval = 20000;

        CTimer ServerReconnectTimer;

        long ServerReconnectInterval = 5000;

        string SystemUuid;

        public List<CotijaEssentialsHuddleSpaceRoomBridge> CotijaRooms { get; private set; }

        long ButtonHeartbeatInterval = 1000;

		bool NeedNewClient;

		/// <summary>
		/// Used to count retries in PostToServer
		/// </summary>
		int RetryCounter;

        public CotijaSystemController(string key, string name, CotijaConfig config) : base(key, name)
        {
            Config = config;
			Debug.Console(0, this, "Mobile UI controller initializing for server:{0}", config.ServerUrl);

            CotijaRooms = new List<CotijaEssentialsHuddleSpaceRoomBridge>();

			//CrestronConsole.AddNewConsoleCommand(s => RegisterSystemToServer(), 
			//    "CotiInitializeHttpClient", "Initializes a new HTTP client connection to a specified URL", ConsoleAccessLevelEnum.AccessOperator);
            CrestronConsole.AddNewConsoleCommand(DisconnectSseClient, 
				"CloseHttpClient", "Closes the active HTTP client", ConsoleAccessLevelEnum.AccessOperator);

			CrestronConsole.AddNewConsoleCommand(AuthorizeSystem,
				"cotijaauth", "Authorizes system to talk to cotija server", ConsoleAccessLevelEnum.AccessOperator);

            AddPostActivationAction(() => RegisterSystemToServer());
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
                string filePath = string.Format(@"\NVRAM\Program{0}\configurationFile.json", Global.ControlSystem.ProgramNumber);
                string postBody = null;

                if (string.IsNullOrEmpty(filePath))
                {
                    Debug.Console(0, this, "Error reading file.  No path specified.");
                    return;
                }

                FileLock = new CCriticalSection();
#warning NEIL I think we need to review this usage. Don't think it ever blocks

				if (FileLock.TryEnter())
                {
                    Debug.Console(1, this, "Reading configuration file to extract system UUID...");

                    postBody = File.ReadToEnd(filePath, Encoding.ASCII);

                    Debug.Console(2, this, "{0}", postBody);

                    FileLock.Leave();
                }

                if (string.IsNullOrEmpty(postBody))
                {
                    Debug.Console(1, "Post Body is null or empty");
                }
                else
                {
					if(Client == null || NeedNewClient)
		                Client = new HttpClient();
					Client.Verbose = true;
					Client.KeepAlive = true;
	
                    SystemUuid = Essentials.ConfigReader.ConfigObject.SystemUuid;

                    string url = string.Format("http://{0}/api/system/join/{1}", Config.ServerUrl, SystemUuid);
					Debug.Console(1, this, "Sending config to {0}", url);

                    HttpClientRequest request = new HttpClientRequest();
                    request.Url.Parse(url);
                    request.RequestType = RequestType.Post;
                    request.Header.SetHeaderValue("Content-Type", "application/json");
                    request.ContentString = postBody;

					var err = Client.DispatchAsync(request, PostConnectionCallback);
                }

            }
            catch (Exception e)
            {
                Debug.Console(0, this, "Error Initilizing Room: {0}", e);
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
        void DisconnectSseClient(string command)
        {
            if(SseClient != null)
                SseClient.Disconnect();

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
			Debug.Console(1, this, "PostConnectionCallback err: {0}", err);
            try
            {
                if (resp != null && resp.Code == 200)
                {
                    if(ServerReconnectTimer != null)
                    {
                        ServerReconnectTimer.Stop();
                        ServerReconnectTimer = null;
                    }

                    ConnectSseClient(null);
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
                Debug.Console(1, this, "Error Initializing SSE Client: {0}", e);
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
        void ConnectSseClient(object o)
        {
            Debug.Console(0, this, "Initializing SSE Client.");

            if (SseClient == null)
            {
                SseClient = new GenericHttpSseClient(string.Format("{0}-SseClient", Key), Name);

                CommunicationGather LineGathered = new CommunicationGather(SseClient, "\x0d\x0a");

                LineGathered.LineReceived += new EventHandler<GenericCommMethodReceiveTextArgs>(SSEClient_LineReceived);
            }
            else
            {
                if (SseClient.IsConnected)
                {
                    SseClient.Disconnect();
                }
            }

            string uuid = Essentials.ConfigReader.ConfigObject.SystemUuid;

            SseClient.Url = string.Format("http://{0}/api/system/stream/{1}", Config.ServerUrl, uuid);

            SseClient.Connect();
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
						ResetOrStartHearbeatTimer();
					}
					else if (type == "close")
					{
						SseClient.Disconnect();

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
                    Debug.Console(1, this, "Unable to parse message: {0}", err);
                }
            }
        }
    }

    
}