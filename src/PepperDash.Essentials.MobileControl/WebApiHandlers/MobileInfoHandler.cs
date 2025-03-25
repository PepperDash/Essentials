using Crestron.SimplSharp.WebScripting;
using Newtonsoft.Json;
using PepperDash.Core;
using PepperDash.Core.Web.RequestHandlers;
using PepperDash.Essentials.Core.Config;
using System;
using System.Collections.Generic;
using System.Linq;

namespace PepperDash.Essentials.WebApiHandlers
{
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
                Debug.Console(1, $"exception showing mobile info: {ex.Message}");
                Debug.Console(2, $"stack trace: {ex.StackTrace}");

                context.Response.StatusCode = 500;
                context.Response.End();
            }
        }
    }

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

    public class MobileControlDirectServer
    {
        [JsonIgnore]
        private readonly MobileControlWebsocketServer directServer;

        [JsonProperty("userAppUrl")]
        public string UserAppUrl => $"{directServer.UserAppUrlPrefix}/[insert_client_token]";

        [JsonProperty("serverPort")]
        public int ServerPort => directServer.Port;

        [JsonProperty("tokensDefined")]
        public int TokensDefined => directServer.UiClients.Count;

        [JsonProperty("clientsConnected")]
        public int ClientsConnected => directServer.ConnectedUiClientsCount;

        [JsonProperty("clients")]
        public List<MobileControlDirectClient> Clients => directServer.UiClients.Select((c, i) => { return new MobileControlDirectClient(c, i, directServer.UserAppUrlPrefix); }).ToList();

        public MobileControlDirectServer(MobileControlWebsocketServer server)
        {
            directServer = server;
        }
    }

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

        [JsonProperty("connected")]
        public bool Connected => context.Client == null ? false : context.Client.Context.WebSocket.IsAlive;

        [JsonProperty("duration")]
        public double Duration => context.Client == null ? 0 : context.Client.ConnectedDuration.TotalSeconds;

        public MobileControlDirectClient(KeyValuePair<string, UiClientContext> clientContext, int index, string urlPrefix)
        {
            context = clientContext.Value;
            Key = clientContext.Key;
            clientNumber = index;
            this.urlPrefix = urlPrefix;
        }
    }
}
