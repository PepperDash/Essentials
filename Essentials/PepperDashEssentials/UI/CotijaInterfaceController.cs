using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Crestron.SimplSharp;
using Crestron.SimplSharp.CrestronIO;
using Crestron.SimplSharpPro;
using Crestron.SimplSharp.Net.Http;
using PepperDash.Core;
using PepperDash.Essentials.Core;
using PepperDash.Essentials.Core.PageManagers;

namespace PepperDash.Essentials
{
    public class CotijaInterfaceController : Device
    {
        GenericHttpSseClient SseClient;

        CCriticalSection FileLock;

        string ServerUrl;

        public CotijaInterfaceController(string key) : base(key)
        {
            CrestronConsole.AddNewConsoleCommand(RegisterRoomToServer, "InitializeHttpClient", "Initializes a new HTTP client connection to a specified URL", ConsoleAccessLevelEnum.AccessOperator);
        }

        /// <summary>
        /// Registers the room with the server 
        /// </summary>
        /// <param name="url">URL of the server, including the port number, if not 80.  Format: "serverUrlOrIp:port"</param>
        void RegisterRoomToServer(string url)
        {
            try
            {
                ServerUrl = url;

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
                    HttpClient Client = new HttpClient();

                    HttpClientRequest Request = new HttpClientRequest();

                    Client.Verbose = true;
                    Client.KeepAlive = true;

                    string uuid = Essentials.ConfigReader.ConfigObject.SystemUuid;

                    url = string.Format("http://{0}/api/system/join/{1}", ServerUrl, uuid);

                    Request.Url.Parse(url);
                    Request.RequestType = RequestType.Post;
                    Request.Header.SetHeaderValue("Content-Type", "application/json");
                    Request.ContentString = postBody;

                    Client.DispatchAsync(Request, PostConnectionCallback);
                }

            }
            catch (Exception e)
            {
                Debug.Console(0, this, "Error Initilizing Room: {0}", e);
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
                if (resp.Code == 200)
                {
                    Debug.Console(0, this, "Initializing SSE Client.");

                    if (SseClient == null)
                    {
                        SseClient = new GenericHttpSseClient(string.Format("{0}-SseClient", Key), Name);
                    }
                    else
                    {
                        if (SseClient.IsConnected)
                        {
                            SseClient.Disconnect();
                        }

                        string uuid = Essentials.ConfigReader.ConfigObject.SystemUuid;

                        SseClient.Url = string.Format("http://{0}/api/system/stream/{1}", ServerUrl, uuid);

                        SseClient.Connect();
                    }

                    CommunicationGather LineGathered = new CommunicationGather(SseClient, "\x0d\x0a");

                    LineGathered.LineReceived += new EventHandler<GenericCommMethodReceiveTextArgs>(LineGathered_LineReceived);


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

        void LineGathered_LineReceived(object sender, GenericCommMethodReceiveTextArgs e)
        {
            Debug.Console(1, this, "Received from Node Server: '{0}'", e.Text);
        }

    }
}