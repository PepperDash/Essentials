using Crestron.SimplSharp.WebScripting;
using Newtonsoft.Json;
using PepperDash.Core;
using PepperDash.Core.Web.RequestHandlers;
using PepperDash.Essentials.Core.Web;
using PepperDash.Essentials.WebSocketServer;
using Serilog.Events;

namespace PepperDash.Essentials.WebApiHandlers
{
    /// <summary>
    /// Represents a UiClientHandler
    /// </summary>
    public class UiClientHandler : WebApiBaseRequestHandler
    {
        private readonly MobileControlWebsocketServer server;

        /// <summary>
        /// Essentials CWS API handler for the MC Direct Server
        /// </summary>
        /// <param name="directServer">Direct Server instance</param>
        public UiClientHandler(MobileControlWebsocketServer directServer) : base(true)
        {
            server = directServer;
        }

        /// <summary>
        /// Create a client for the Direct Server
        /// </summary>
        /// <param name="context">HTTP Context for this request</param>
        protected override void HandlePost(HttpCwsContext context)
        {
            var req = context.Request;
            var res = context.Response;
            var body = EssentialsWebApiHelpers.GetRequestBody(req);

            var request = JsonConvert.DeserializeObject<ClientRequest>(body);

            var response = new ClientResponse();

            if (string.IsNullOrEmpty(request?.RoomKey))
            {
                response.Error = "roomKey is required";

                res.StatusCode = 400;
                res.ContentType = "application/json";
                res.Headers.Add("Content-Type", "application/json");
                res.Write(JsonConvert.SerializeObject(response), false);
                res.End();
                return;
            }

            if (string.IsNullOrEmpty(request.GrantCode))
            {
                response.Error = "grantCode is required";

                res.StatusCode = 400;
                res.ContentType = "application/json";
                res.Headers.Add("Content-Type", "application/json");
                res.Write(JsonConvert.SerializeObject(response), false);
                res.End();
                return;
            }

            var (token, path) = server.ValidateGrantCode(request.GrantCode, request.RoomKey);

            response.Token = token;
            response.Path = path;

            res.StatusCode = 200;
            res.ContentType = "application/json";
            res.Headers.Add("Content-Type", "application/json");
            res.Write(JsonConvert.SerializeObject(response), false);
            res.End();
        }

        /// <summary>
        /// Handle DELETE request for a Client
        /// </summary>
        /// <param name="context"></param>
        protected override void HandleDelete(HttpCwsContext context)
        {
            var req = context.Request;
            var res = context.Response;
            var body = EssentialsWebApiHelpers.GetRequestBody(req);

            var request = JsonConvert.DeserializeObject<ClientRequest>(body);



            if (string.IsNullOrEmpty(request?.Token))
            {
                var response = new ClientResponse
                {
                    Error = "token is required"
                };

                res.StatusCode = 400;
                res.ContentType = "application/json";
                res.Headers.Add("Content-Type", "application/json");
                res.Write(JsonConvert.SerializeObject(response), false);
                res.End();

                return;
            }



            if (!server.UiClientContexts.TryGetValue(request.Token, out UiClientContext clientContext))
            {
                var response = new ClientResponse
                {
                    Error = $"Unable to find client with token: {request.Token}"
                };

                res.StatusCode = 200;
                res.ContentType = "application/json";
                res.Headers.Add("Content-Type", "application/json");
                res.Write(JsonConvert.SerializeObject(response), false);
                res.End();

                return;
            }

            if (clientContext.Client != null && clientContext.Client.Context.WebSocket.IsAlive)
            {
                clientContext.Client.Context.WebSocket.Close(WebSocketSharp.CloseStatusCode.Normal, "Token removed from server");
            }

            var path = server.WsPath + request.Token;

            if (!server.Server.RemoveWebSocketService(path))
            {
                Debug.LogMessage(LogEventLevel.Warning, "Unable to remove client with token {token}", request.Token);

                var response = new ClientResponse
                {
                    Error = $"Unable to remove client with token {request.Token}"
                };

                res.StatusCode = 500;
                res.ContentType = "application/json";
                res.Headers.Add("Content-Type", "application/json");
                res.Write(JsonConvert.SerializeObject(response), false);
                res.End();

                return;
            }

            server.UiClientContexts.Remove(request.Token);

            server.UpdateSecret();

            res.StatusCode = 200;
            res.End();
        }
    }

    /// <summary>
    /// Represents a ClientRequest
    /// </summary>
    public class ClientRequest
    {

        /// <summary>
        /// Gets or sets the RoomKey
        /// </summary>
        [JsonProperty("roomKey", NullValueHandling = NullValueHandling.Ignore)]
        public string RoomKey { get; set; }


        /// <summary>
        /// Gets or sets the GrantCode
        /// </summary>
        [JsonProperty("grantCode", NullValueHandling = NullValueHandling.Ignore)]
        public string GrantCode { get; set; }


        /// <summary>
        /// Gets or sets the Token
        /// </summary>
        [JsonProperty("token", NullValueHandling = NullValueHandling.Ignore)]
        public string Token { get; set; }
    }

    /// <summary>
    /// Represents a ClientResponse
    /// </summary>
    public class ClientResponse
    {

        /// <summary>
        /// Gets or sets the Error
        /// </summary>
        [JsonProperty("error", NullValueHandling = NullValueHandling.Ignore)]
        public string Error { get; set; }


        /// <summary>
        /// Gets or sets the Token
        /// </summary>
        [JsonProperty("token", NullValueHandling = NullValueHandling.Ignore)]
        public string Token { get; set; }


        /// <summary>
        /// Gets or sets the Path
        /// </summary>
        [JsonProperty("path", NullValueHandling = NullValueHandling.Ignore)]
        public string Path { get; set; }
    }
}
