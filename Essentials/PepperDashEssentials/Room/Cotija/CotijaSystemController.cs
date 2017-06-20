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

        Dictionary<string, Object> ActionDictionary;

        CTimer Heartbeat;

        CTimer Reconnect;

        string SystemUuid;

        public List<CotijaEssentialsHuddleSpaceRoomBridge> CotijaRooms { get; private set; }

        public CotijaSystemController(string key, string name, CotijaConfig config) : base(key, name)
        {
            Config = config;

            ActionDictionary = new Dictionary<string, Object>();

            CotijaRooms = new List<CotijaEssentialsHuddleSpaceRoomBridge>();

            CrestronConsole.AddNewConsoleCommand(ConnectSseClient, "InitializeHttpClient", "Initializes a new HTTP client connection to a specified URL", ConsoleAccessLevelEnum.AccessOperator);
            CrestronConsole.AddNewConsoleCommand(DisconnectSseClient, "CloseHttpClient", "Closes the active HTTP client", ConsoleAccessLevelEnum.AccessOperator);

            AddPostActivationAction(() => RegisterSystemToServer(null));
        }

        public void AddAction(string key, object action)
        {
            // This might blow up if an action with that key already exists
            ActionDictionary.Add(key, action);
        }

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
                    Debug.Console(1, this, "Reading Configuration File");

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
            if(Client == null)
                Client = new HttpClient();

            HttpClientRequest request = new HttpClientRequest();

            Client.Verbose = true;
            Client.KeepAlive = true;

            string url = string.Format("http://{0}/api/room/{1}", Config.serverUrl, string.Format("{0}-{1}", SystemUuid, room.Key));

            request.Url.Parse(url);
            request.RequestType = RequestType.Post;
            request.Header.SetHeaderValue("Content-Type", "application/json");
            request.ContentString = o.ToString();

            Client.DispatchAsync(request, PostConnectionCallback);
        }

        /// <summary>
        /// Disconnects the SSE Client and stops the heartbeat timer
        /// </summary>
        /// <param name="command"></param>
        void DisconnectSseClient(string command)
        {
            if(SseClient != null)
                SseClient.Disconnect();

            if (Heartbeat != null)
            {
                Heartbeat.Stop();

                Heartbeat = null;
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
                    if (SseClient == null)
                    {
                        ConnectSseClient(null);
                    }
                }
                else
                {
                    Debug.Console(0, this, "Unable to initialize SSE Client");
                }
            }
            catch (Exception e)
            {
                Debug.Console(1, this, "Error Initializeing SSE Client: {0}", e);
            }
        }

        /// <summary>
        /// Executes when we don't get a heartbeat message in time.  Triggers reconnect.
        /// </summary>
        /// <param name="o"></param>
        void HeartbeatExpired(object o)
        {
             if (Heartbeat != null)
            {
                Heartbeat.Stop();

                Heartbeat = null;
            }

            // Start the reconnect timer
            Reconnect = new CTimer(ReconnectToServer, null, 5000, 5000);

            Reconnect.Reset(5000, 5000);
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

            //Heartbeat = new CTimer(HeartbeatExpired, null, 20000, 20000);

            //Heartbeat.Reset(20000, 20000);
        }

        void LineGathered_LineReceived(object sender, GenericCommMethodReceiveTextArgs e)
        {
            //Debug.Console(1, this, "Received from Server: '{0}'", e.Text);

            if(e.Text.IndexOf("data:") > -1)
            {
                var message = e.Text.Substring(6);

                string roomId = null;

                Debug.Console(1, this, "Message: '{0}'", message);

                try
                {
                    var messageObj = JObject.Parse(message);

                    var type = messageObj["type"].Value<string>();

                    if(type == "/system/hearbeat")
                    {
                        //Heartbeat.Reset(20000, 20000);
                    }
                    else if(type == "close")
                    {
                        SseClient.Disconnect();

                        // Start the reconnect timer
                        Reconnect = new CTimer(ConnectSseClient, null, 5000, 5000);

                        Reconnect.Reset(5000, 5000);
                    }
                    else
                    {


                        // Check path against Action dictionary
                        if (ActionDictionary.ContainsKey(type))
                        {
                            var action = ActionDictionary[type];

                            if (action is Action<bool>)
                            {
                                var stateString = messageObj["content"]["state"].Value<string>();

                                // Look for a button press event
                                if(!string.IsNullOrEmpty(stateString))
                                {
#warning deal with held state later
                                    if (stateString == "held")
                                        return;

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