using Crestron.SimplSharp.WebScripting;
using Newtonsoft.Json;
using PepperDash.Core.Web.RequestHandlers;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace PepperDash.Essentials.Core.Web.RequestHandlers
{
    /// <summary>
    /// Represents a GetRoutingPortsHandler
    /// </summary>
    public class GetRoutingPortsHandler : WebApiBaseRequestHandler
    {
        /// <summary>
        /// Constructor
        /// </summary>
        public GetRoutingPortsHandler() : base(true) { }

        /// <summary>
        /// Handles the GET request
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

            if(!routeData.Values.TryGetValue("deviceKey", out var deviceKey))
            {
                context.Response.StatusCode = 400;
                context.Response.StatusDescription = "Bad Request";
                context.Response.End();
                return;
            }

            var device = DeviceManager.GetDeviceForKey(deviceKey.ToString());

            if (device == null)
            {
                context.Response.StatusCode = 404;
                context.Response.StatusDescription = "Device Not Found";
                context.Response.End();
                return;
            }

            var inputPorts = (device as IRoutingInputs)?.InputPorts;
            var outputPorts = (device as IRoutingOutputs)?.OutputPorts;
            
            var response = JsonConvert.SerializeObject( new ReturnValue
            {
                InputPorts = inputPorts?.Select(p => p.Key).ToList(),
                OutputPorts = outputPorts?.Select(p => p.Key).ToList()
            });

            context.Response.StatusCode = 200;
            context.Response.StatusDescription = "OK";
            context.Response.ContentType = "application/json";
            context.Response.ContentEncoding = Encoding.UTF8;
            context.Response.Write(response, false);
            context.Response.End();

        }
    }

    internal class ReturnValue 
    {
        /// <summary>
        /// Gets or sets the InputPorts
        /// </summary>
        [JsonProperty("inputPorts", NullValueHandling = NullValueHandling.Ignore)]
        public List<string> InputPorts { get; set; }

        /// <summary>
        /// Gets or sets the OutputPorts
        /// </summary>
        [JsonProperty("outputPorts", NullValueHandling = NullValueHandling.Ignore)]
        public List<string> OutputPorts { get; set; }
    }
}
