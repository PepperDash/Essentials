using Crestron.SimplSharp;
using Crestron.SimplSharp.WebScripting;
using Crestron.SimplSharpPro.EthernetCommunication;
using Newtonsoft.Json;
using PepperDash.Core;
using PepperDash.Core.Web.RequestHandlers;
using Serilog.Events;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PepperDash.Essentials.Core.Web.RequestHandlers
{
    public class DebugSessionRequestHandler : WebApiBaseRequestHandler
    {    
        public DebugSessionRequestHandler()
            : base(true)
        {
        }

        /// <summary>
        /// Gets details for a debug session
        /// </summary>
        /// <param name="context"></param>
        protected override void HandleGet(HttpCwsContext context)
        {
            var routeData = context.Request.RouteData;
            if (routeData == null)
            {
                context.Response.StatusCode = 400;
                context.Response.StatusDescription = "Bad Request";
                context.Response.End();

                return;
            }

            try
            {
                var data = new
                {
                    url = "" // TODO: Add the URL of the websocket server
                };

                Debug.LogMessage(LogEventLevel.Information, "Debug Session URL: {0}", data.url);

                // Return the port number with the full url of the WS Server
                var res = JsonConvert.SerializeObject(data);

                context.Response.ContentType = "application/json";
                context.Response.ContentEncoding = Encoding.UTF8;
                context.Response.StatusCode = 200;
                context.Response.StatusDescription = "OK";
                context.Response.Write(res, false);
                context.Response.End();
            }
            catch (Exception e)
            {
                Debug.LogMessage(LogEventLevel.Information, "Error: {0}", e);
            }
        }

        /// <summary>
        /// Stops a debug session
        /// </summary>
        /// <param name="context"></param>
        protected override void HandlePost(HttpCwsContext context)
        {
            context.Response.StatusCode = 200;
            context.Response.StatusDescription = "OK";
            context.Response.End();

            Debug.LogMessage(LogEventLevel.Information, "Websocket Debug Session Stopped");
        }

    }
}
