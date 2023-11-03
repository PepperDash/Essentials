using Crestron.SimplSharp;
using Newtonsoft.Json;
using PepperDash.Core;
using PepperDash.Core.Web.RequestHandlers;
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

        protected override void HandleGet(Crestron.SimplSharp.WebScripting.HttpCwsContext context)
        {
            var routeData = context.Request.RouteData;
            if (routeData == null)
            {
                context.Response.StatusCode = 400;
                context.Response.StatusDescription = "Bad Request";
                context.Response.End();

                return;
            }

            // Generate a random port within a specified range

            // Start the WS Server

            // Return the port number with the full url of the WS Server

            var ip = CrestronEthernetHelper.GetEthernetParameter(
                CrestronEthernetHelper.ETHERNET_PARAMETER_TO_GET.GET_CURRENT_IP_ADDRESS, 0);

            var port = new Random().Next(65435, 65535);

            object data = new {
                url = string.Format(@"ws://{ip}:{port}", ip, port)
            };

            var res = JsonConvert.SerializeObject(data);

            context.Response.Write(res, false);
            context.Response.ContentType = "application/json";
            context.Response.StatusCode = 200;
            context.Response.StatusDescription = "OK";
            context.Response.End();
        }   

    }
}
