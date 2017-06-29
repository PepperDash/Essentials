using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using Crestron.SimplSharp;
using Crestron.SimplSharp.CrestronIO;
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

        CotijaConfig Config;

        HttpClient Client;

        Dictionary<string, Object> ActionDictionary = new Dictionary<string, Object>(StringComparer.InvariantCultureIgnoreCase);

        Dictionary<string, CTimer> PushedActions = new Dictionary<string, CTimer>();

        CTimer ServerHeartbeat;

        long ServerHeartbeatInterval = 20000;

        CTimer ServerReconnect;

        long ServerReconnectInterval = 5000;

        string SystemUuid;

        public List<CotijaEssentialsHuddleSpaceRoomBridge> CotijaRooms { get; private set; }

        long ButtonHeartbeatInterval = 1000;

        public CotijaSystemController(string key, string name, CotijaConfig config) : base(key, name)
        {
            Config = config;

            CotijaRooms = new List<CotijaEssentialsHuddleSpaceRoomBridge>();

            CrestronConsole.AddNewConsoleCommand(RegisterSystemToServer, "InitializeHttpClient", "Initializes a new HTTP client connection to a specified URL", ConsoleAccessLevelEnum.AccessOperator);
            CrestronConsole.AddNewConsoleCommand(DisconnectSseClient, "CloseHttpClient", "Closes the active HTTP client", ConsoleAccessLevelEnum.AccessOperator);

            AddPostActivationAction(() => RegisterSystemToServer(null));
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
                Debug.Console(1, this, "Cannot add action with key '{0}' because key already exists in ActionDictionary.");
            }
        }

        /// <summary>
        /// Removes and action from the dictionary
        /// </summary>
        /// <param name="key"></param>
        public void RemoveAction(string key)
        {
            if (ActionDictionary.ContainsKey(key))
                ActionDictionary.Remove(key);
        }

        void ReconnectToServer(object o)
        {
            RegisterSystemToServer(null);
        }

        /// <summary>
        /// Registers the room with the server
        /// </summary>
        /// <param name="url">URL of the server, including the port number, if not 80.  Format: "serverUrlOrIp:port"</param>
        void RegisterSystemToServer(string command)
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
                    Client = new HttpClient();

                    HttpClientRequest request = new HttpClientRequest();

                    Client.Verbose = true;
                    Client.KeepAlive = true;

                    SystemUuid = Essentials.ConfigReader.ConfigObject.SystemUuid;

                    string url = string.Format("http://{0}/api/system/join/{1}", Config.serverUrl, SystemUuid);

                    request.Url.Parse(url);
                    request.RequestType = RequestType.Post;
                    request.Header.SetHeaderValue("Content-Type", "application/json");
                    request.ContentString = postBody;

                    Client.DispatchAsync(request, PostConnectionCallback);
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
        public void PostToServer(EssentialsRoomBase room, JObject o)
        {
            try
            {
                if (Client == null)
                    Client = new HttpClient();

                //HttpClient client = new HttpClient();

                HttpClientRequest request = new HttpClientRequest();

                Client.Verbose = true;
                Client.KeepAlive = true;

                string url = string.Format("http://{0}/api/room/{1}/status", Config.serverUrl, string.Format("{0}--{1}", SystemUuid, room.Key));

                request.Url.Parse(url);
                request.RequestType = RequestType.Post;
                request.Header.SetHeaderValue("Content-Type", "application/json");
                request.KeepAlive = true;

                // Ignore any null objects when serializing and remove formatting
                string ignored = JsonConvert.SerializeObject(o, Formatting.None, new JsonSerializerSettings { NullValueHandling = NullValueHandling.Ignore });
                request.ContentString = ignored;

                Debug.Console(1, this, "Posting to '{0}':\n{1}", url, request.ContentString);

                Client.DispatchAsync(request, (r, err) => { if (r != null) { Debug.Console(1, this, "Status Response Code: {0}", r.Code); } });

                StartReconnectTimer(ServerReconnectInterval, ServerReconnectInterval);
            }
            catch(Exception e)
            {
                Debug.Console(1, this, "Error Posting to Server: {0}", e);
            }
        }

        /// <summary>
        /// Disconnects the SSE Client and stops the heartbeat timer
        /// </summary>
        /// <param name="command"></param>
        void DisconnectSseClient(string command)
        {
            if(SseClient != null)
                SseClient.Disconnect();

            if (ServerHeartbeat != null)
            {
                ServerHeartbeat.Stop();

                ServerHeartbeat = null;
            }
        }
        
        /// <summary>
        /// The callback that fires when we get a response from our registration attempt
        /// </summary>
        /// <param name="resp"></param>
        /// <param name="err"></param>
        void PostConnectionCallback(HttpClientResponse resp, HTTP_CALLBACK_ERROR err)
        {
            try
            {
                if (resp != null && resp.Code == 200)
                {
                    if(ServerReconnect != null)
                    {
                        ServerReconnect.Stop();

                        ServerReconnect = null;
                    }

                    if (SseClient == null)
                    {
                        ConnectSseClient(null);
                    }
                }
                else
                {
                    if (resp != null)
                        Debug.Console(1, this, "Response from server: {0}\n{1}", resp.Code, err);
                    else
                        Debug.Console(1, this, "Null response received from server.");
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
        void HeartbeatExpired(object o)
        {
             if (ServerHeartbeat != null)
            {
                Debug.Console(1, this, "Heartbeat Timer Expired.");

                ServerHeartbeat.Stop();

                ServerHeartbeat = null;
            }

             StartReconnectTimer(ServerReconnectInterval, ServerReconnectInterval);
        }

        void StartReconnectTimer(long dueTime, long repeatTime)
        {
            // Start the reconnect timer
            ServerReconnect = new CTimer(ReconnectToServer, null, dueTime, repeatTime);

            ServerReconnect.Reset(dueTime, repeatTime);
        }

        void StartHearbeatTimer(long dueTime, long repeatTime)
        {
            if (ServerHeartbeat == null)
            {
                ServerHeartbeat = new CTimer(HeartbeatExpired, null, dueTime, repeatTime);

                Debug.Console(2, this, "Heartbeat Timer Started.");
            }

            ServerHeartbeat.Reset(dueTime, repeatTime);

            Debug.Console(2, this, "Heartbeat Timer Reset.");
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

                LineGathered.LineReceived += new EventHandler<GenericCommMethodReceiveTextArgs>(LineGathered_LineReceived);
            }
            else
            {
                if (SseClient.IsConnected)
                {
                    SseClient.Disconnect();
                }
            }

            string uuid = Essentials.ConfigReader.ConfigObject.SystemUuid;

            SseClient.Url = string.Format("http://{0}/api/system/stream/{1}", Config.serverUrl, uuid);

            SseClient.Connect();
        }
            

        void LineGathered_LineReceived(object sender, GenericCommMethodReceiveTextArgs e)
        {
            //Debug.Console(1, this, "Received from Server: '{0}'", e.Text);

            if(e.Text.IndexOf("data:") > -1)
            {
                var message = e.Text.Substring(6);

                Debug.Console(1, this, "Message: '{0}'", message);

                try
                {
                    var messageObj = JObject.Parse(message);

                    var type = messageObj["type"].Value<string>();

                    if (type == "hello")
                    {
                        StartHearbeatTimer(ServerHeartbeatInterval, ServerHeartbeatInterval);
                    }
                    else if (type == "/system/heartbeat")
                    {
                        StartHearbeatTimer(ServerHeartbeatInterval, ServerHeartbeatInterval);
                    }
                    else if (type == "close")
                    {
                        SseClient.Disconnect();

                        // Start the reconnect timer
                        ServerReconnect = new CTimer(ConnectSseClient, null, ServerReconnectInterval, ServerReconnectInterval);

                        ServerReconnect.Reset(ServerReconnectInterval, ServerReconnectInterval);
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