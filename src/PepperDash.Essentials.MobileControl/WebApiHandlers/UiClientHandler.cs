using Crestron.SimplSharp.WebScripting;
using Newtonsoft.Json;
using PepperDash.Core;
using PepperDash.Core.Web.RequestHandlers;
using PepperDash.Essentials.Core.Web;

namespace PepperDash.Essentials.WebApiHandlers
{
    public class UiClientHandler : WebApiBaseRequestHandler
    {
        private readonly MobileControlWebsocketServer server;
        public UiClientHandler(MobileControlWebsocketServer directServer) : base(true)
        {
            server = directServer;
        }

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



            if (!server.UiClients.TryGetValue(request.Token, out UiClientContext clientContext))
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
                Debug.Console(0, $"Unable to remove client with token {request.Token}");

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

            server.UiClients.Remove(request.Token);

            server.UpdateSecret();

            res.StatusCode = 200;
            res.End();
        }
    }

    public class ClientRequest
    {
        [JsonProperty("roomKey", NullValueHandling = NullValueHandling.Ignore)]
        public string RoomKey { get; set; }

        [JsonProperty("grantCode", NullValueHandling = NullValueHandling.Ignore)]
        public string GrantCode { get; set; }

        [JsonProperty("token", NullValueHandling = NullValueHandling.Ignore)]
        public string Token { get; set; }
    }

    public class ClientResponse
    {
        [JsonProperty("error", NullValueHandling = NullValueHandling.Ignore)]
        public string Error { get; set; }

        [JsonProperty("token", NullValueHandling = NullValueHandling.Ignore)]
        public string Token { get; set; }

        [JsonProperty("path", NullValueHandling = NullValueHandling.Ignore)]
        public string Path { get; set; }
    }
}
