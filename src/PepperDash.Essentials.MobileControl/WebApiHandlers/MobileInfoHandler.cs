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
    /// Represents a MobileInfoHandler
    /// </summary>
    public class MobileInfoHandler : WebApiBaseRequestHandler
    {
        private readonly MobileControlSystemController mcController;
        public MobileInfoHandler(MobileControlSystemController controller) : base(true)
        {
            mcController = controller;
        }

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

        [JsonProperty("edgeServer", NullValueHandling = NullValueHandling.Ignore)]
        public MobileControlEdgeServer EdgeServer => mcController.Config.EnableApiServer ? new MobileControlEdgeServer(mcController) : null;


        [JsonProperty("directServer", NullValueHandling = NullValueHandling.Ignore)]
        public MobileControlDirectServer DirectServer => mcController.Config.DirectServer.EnableDirectServer ? new MobileControlDirectServer(mcController.DirectServer) : null;


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

        [JsonProperty("serverAddress")]
        public string ServerAddress => mcController.Config == null ? "No Config" : mcController.Host;

        [JsonProperty("systemName")]
        public string SystemName => mcController.RoomBridges.Count > 0 ? mcController.RoomBridges[0].RoomName : "No Config";

        [JsonProperty("systemUrl")]
        public string SystemUrl => ConfigReader.ConfigObject.SystemUrl;

        [JsonProperty("userCode")]
        public string UserCode => mcController.RoomBridges.Count > 0 ? mcController.RoomBridges[0].UserCode : "Not available";

        [JsonProperty("connected")]
        public bool Connected => mcController.Connected;

        [JsonProperty("secondsSinceLastAck")]
        public int SecondsSinceLastAck => (DateTime.Now - mcController.LastAckMessage).Seconds;

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

        [JsonProperty("userAppUrl")]
        public string UserAppUrl => $"{directServer.UserAppUrlPrefix}/[insert_client_token]";

        [JsonProperty("serverPort")]
        public int ServerPort => directServer.Port;

        [JsonProperty("tokensDefined")]
        public int TokensDefined => directServer.UiClientContexts.Count;

        [JsonProperty("clientsConnected")]
        public int ClientsConnected => directServer.ConnectedUiClientsCount;

        [JsonProperty("clients")]
        public List<MobileControlDirectClient> Clients => directServer.UiClientContexts
            .Select(context => (context, clients: directServer.UiClients.Where(client => client.Value.Token == context.Value.Token.Token).Select(c => c.Value).ToList()))
            .Select((clientTuple, i) => new MobileControlDirectClient(clientTuple.clients, clientTuple.context, i, directServer.UserAppUrlPrefix))
            .ToList();


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

        [JsonProperty("clientNumber")]
        public string ClientNumber => $"{clientNumber}";

        [JsonProperty("roomKey")]
        public string RoomKey => context.Token.RoomKey;

        [JsonProperty("touchpanelKey")]
        public string TouchpanelKey => context.Token.TouchpanelKey;

        [JsonProperty("url")]
        public string Url => $"{urlPrefix}{Key}";

        [JsonProperty("token")]
        public string Token => Key;

        private readonly List<UiClient> clients;

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

    public class ClientStatus
    {
        private readonly UiClient client;

        public bool Connected => client != null && client.Context.WebSocket.IsAlive;

        public double Duration => client == null ? 0 : client.ConnectedDuration.TotalSeconds;

        public ClientStatus(UiClient client)
        {
            this.client = client;
        }
    }
}
