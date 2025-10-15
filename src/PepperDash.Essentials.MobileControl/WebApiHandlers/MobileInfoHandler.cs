using System;
using System.Collections.Generic;
using System.Linq;
using Crestron.SimplSharp.WebScripting;
using Newtonsoft.Json;
using PepperDash.Core;
using PepperDash.Core.Web.RequestHandlers;
using PepperDash.Essentials.Core.Config;
using PepperDash.Essentials.WebSocketServer;

namespace PepperDash.Essentials.WebApiHandlers
{
    /// <summary>
    /// Represents a MobileInfoHandler. Used with the Essentials CWS API
    /// </summary>
    public class MobileInfoHandler : WebApiBaseRequestHandler
    {
        private readonly MobileControlSystemController mcController;

        /// <summary>
        /// Create an instance of the <see cref="MobileInfoHandler"/> class.
        /// </summary>
        /// <param name="controller"></param>
        public MobileInfoHandler(MobileControlSystemController controller) : base(true)
        {
            mcController = controller;
        }

        /// <summary>
        /// Get Mobile Control Information
        /// </summary>
        /// <param name="context"></param>
        protected override void HandleGet(HttpCwsContext context)
        {
            try
            {
                var response = new InformationResponse(mcController);

                context.Response.StatusCode = 200;
                context.Response.ContentType = "application/json";
                context.Response.Write(JsonConvert.SerializeObject(response), false);
                context.Response.End();
            }
            catch (Exception ex)
            {
                Debug.LogMessage(ex, "exception showing mobile info");

                context.Response.StatusCode = 500;
                context.Response.End();
            }
        }
    }

    /// <summary>
    /// Represents a InformationResponse
    /// </summary>
    public class InformationResponse
    {
        [JsonIgnore]
        private readonly MobileControlSystemController mcController;

        /// <summary>
        /// Edge Server. Null if edge server is disabled
        /// </summary>
        [JsonProperty("edgeServer", NullValueHandling = NullValueHandling.Ignore)]
        public MobileControlEdgeServer EdgeServer => mcController.Config.EnableApiServer ? new MobileControlEdgeServer(mcController) : null;

        /// <summary>
        /// Direct server. Null if the direct server is disabled
        /// </summary>
        [JsonProperty("directServer", NullValueHandling = NullValueHandling.Ignore)]
        public MobileControlDirectServer DirectServer => mcController.Config.DirectServer.EnableDirectServer ? new MobileControlDirectServer(mcController.DirectServer) : null;

        /// <summary>
        /// Create an instance of the <see cref="InformationResponse"/> class.
        /// </summary>
        /// <param name="controller"></param>
        public InformationResponse(MobileControlSystemController controller)
        {
            mcController = controller;
        }
    }

    /// <summary>
    /// Represents a MobileControlEdgeServer
    /// </summary>
    public class MobileControlEdgeServer
    {
        [JsonIgnore]
        private readonly MobileControlSystemController mcController;

        /// <summary>
        /// Mobile Control Edge Server address for this system
        /// </summary>
        [JsonProperty("serverAddress")]
        public string ServerAddress => mcController.Config == null ? "No Config" : mcController.Host;

        /// <summary>
        /// System Name for this system
        /// </summary>
        [JsonProperty("systemName")]
        public string SystemName => mcController.RoomBridges.Count > 0 ? mcController.RoomBridges[0].RoomName : "No Config";

        /// <summary>
        /// System URL for this system
        /// </summary>
        [JsonProperty("systemUrl")]
        public string SystemUrl => ConfigReader.ConfigObject.SystemUrl;

        /// <summary>
        /// User code to use in MC UI for this system
        /// </summary>
        [JsonProperty("userCode")]
        public string UserCode => mcController.RoomBridges.Count > 0 ? mcController.RoomBridges[0].UserCode : "Not available";

        /// <summary>
        /// True if connected to edge server
        /// </summary>
        [JsonProperty("connected")]
        public bool Connected => mcController.Connected;

        /// <summary>
        /// Seconds since last comms with edge server
        /// </summary>
        [JsonProperty("secondsSinceLastAck")]
        public int SecondsSinceLastAck => (DateTime.Now - mcController.LastAckMessage).Seconds;

        /// <summary>
        /// Create an instance of the <see cref="MobileControlEdgeServer"/> class.
        /// </summary>
        /// <param name="controller">controller to use for this</param>
        public MobileControlEdgeServer(MobileControlSystemController controller)
        {
            mcController = controller;
        }
    }

    /// <summary>
    /// Represents a MobileControlDirectServer
    /// </summary>
    public class MobileControlDirectServer
    {
        [JsonIgnore]
        private readonly MobileControlWebsocketServer directServer;

        /// <summary>
        /// URL to use to interact with this server
        /// </summary>
        [JsonProperty("userAppUrl")]
        public string UserAppUrl => $"{directServer.UserAppUrlPrefix}/[insert_client_token]";

        /// <summary>
        /// TCP/IP Port this server is configured to use
        /// </summary>
        [JsonProperty("serverPort")]
        public int ServerPort => directServer.Port;

        /// <summary>
        /// Count of defined tokens for this server
        /// </summary>
        [JsonProperty("tokensDefined")]
        public int TokensDefined => directServer.UiClientContexts.Count;

        /// <summary>
        /// Count of connected clients
        /// </summary>
        [JsonProperty("clientsConnected")]
        public int ClientsConnected => directServer.ConnectedUiClientsCount;

        /// <summary>
        /// List of tokens and connected clients for this server
        /// </summary>
        [JsonProperty("clients")]
        public List<MobileControlDirectClient> Clients => directServer.UiClientContexts
            .Select(context => (context, clients: directServer.UiClients.Where(client => client.Value.Token == context.Value.Token.Token).Select(c => c.Value).ToList()))
            .Select((clientTuple, i) => new MobileControlDirectClient(clientTuple.clients, clientTuple.context, i, directServer.UserAppUrlPrefix))
            .ToList();

        /// <summary>
        /// Create an instance of the <see cref="MobileControlDirectServer"/> class.
        /// </summary>
        /// <param name="server"></param>
        public MobileControlDirectServer(MobileControlWebsocketServer server)
        {
            directServer = server;
        }
    }

    /// <summary>
    /// Represents a MobileControlDirectClient
    /// </summary>
    public class MobileControlDirectClient
    {
        [JsonIgnore]
        private readonly UiClientContext context;

        [JsonIgnore]
        private readonly string Key;

        [JsonIgnore]
        private readonly int clientNumber;

        [JsonIgnore]
        private readonly string urlPrefix;

        /// <summary>
        /// Client number for this client
        /// </summary>
        [JsonProperty("clientNumber")]
        public string ClientNumber => $"{clientNumber}";

        /// <summary>
        /// Room Key for this client
        /// </summary>
        [JsonProperty("roomKey")]
        public string RoomKey => context.Token.RoomKey;

        /// <summary>
        /// Touchpanel Key, if defined, for this client
        /// </summary>
        [JsonProperty("touchpanelKey")]
        public string TouchpanelKey => context.Token.TouchpanelKey;

        /// <summary>
        /// URL for this client
        /// </summary>
        [JsonProperty("url")]
        public string Url => $"{urlPrefix}{Key}";

        /// <summary>
        /// Token for this client
        /// </summary>
        [JsonProperty("token")]
        public string Token => Key;

        private readonly List<UiClient> clients;

        /// <summary>
        /// List of status for all connected UI Clients
        /// </summary>
        [JsonProperty("clientStatus")]
        public List<ClientStatus> ClientStatus => clients.Select(c => new ClientStatus(c)).ToList();

        /// <summary>
        /// Create an instance of the <see cref="MobileControlDirectClient"/> class.
        /// </summary>
        /// <param name="clients">List of Websocket Clients</param>
        /// <param name="context">Context for the client</param>
        /// <param name="index">Index of the client</param>
        /// <param name="urlPrefix">URL prefix for the client</param>
        public MobileControlDirectClient(List<UiClient> clients, KeyValuePair<string, UiClientContext> context, int index, string urlPrefix)
        {
            this.context = context.Value;
            Key = context.Key;
            clientNumber = index;
            this.urlPrefix = urlPrefix;
            this.clients = clients;
        }
    }

    /// <summary>
    /// Report the status of a UiClient
    /// </summary>
    public class ClientStatus
    {
        private readonly UiClient client;

        /// <summary>
        /// True if client is connected
        /// </summary>
        public bool Connected => client != null && client.Context.WebSocket.IsAlive;

        /// <summary>
        /// Get the time this client has been connected
        /// </summary>
        public double Duration => client == null ? 0 : client.ConnectedDuration.TotalSeconds;

        /// <summary>
        /// Create an instance of the <see cref="ClientStatus"/> class for the specified client
        /// </summary>
        /// <param name="client">client to report on</param>
        public ClientStatus(UiClient client)
        {
            this.client = client;
        }
    }
}
