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
    /// Represents a DeleteAllUiClientsHandler
    /// </summary>
    public class DeleteAllUiClientsHandler : WebApiBaseRequestHandler
    {
        private readonly MobileControlWebsocketServer server;

        /// <summary>
        /// Essentials CWS API handler for the MC Direct Server
        /// </summary>
        /// <param name="directServer">Direct Server instance</param>
        public DeleteAllUiClientsHandler(MobileControlWebsocketServer directServer) : base(true)
        {
            server = directServer;
        }

        /// <summary>
        /// Deletes all clients from the Direct Server
        /// </summary>
        /// <param name="context">HTTP Context for this request</param>
        protected override void HandleDelete(HttpCwsContext context)
        {
            server.RemoveAllTokens("confirm");

            var res = context.Response;
            res.StatusCode = 200;
            res.ContentType = "application/json";
            res.Headers.Add("Content-Type", "application/json");
            res.Write(JsonConvert.SerializeObject(new { success = true }), false);
            res.End();
        }
    }
}