using Crestron.SimplSharp;
using Crestron.SimplSharp.WebScripting;
using Newtonsoft.Json;
using PepperDash.Core.Web.RequestHandlers;

namespace PepperDash.Essentials.Core.Web.RequestHandlers
{
    /// <summary>
    /// Represents a GetRoutesHandler
    /// </summary>
    public class GetRoutesHandler:WebApiBaseRequestHandler
    {
        private HttpCwsRouteCollection routeCollection;
        private string basePath;

        public GetRoutesHandler(HttpCwsRouteCollection routeCollection, string basePath) {
            this.routeCollection = routeCollection;
            this.basePath = basePath;
        }

        protected override void HandleGet(HttpCwsContext context)
        {
            var currentIp = CrestronEthernetHelper.GetEthernetParameter(
                CrestronEthernetHelper.ETHERNET_PARAMETER_TO_GET.GET_CURRENT_IP_ADDRESS, 0);

            var hostname = CrestronEthernetHelper.GetEthernetParameter(
                    CrestronEthernetHelper.ETHERNET_PARAMETER_TO_GET.GET_HOSTNAME, 0);

            var path = CrestronEnvironment.DevicePlatform == eDevicePlatform.Server
                ? $"https://{hostname}/VirtualControl/Rooms/{InitialParametersClass.RoomId}/cws{basePath}"
                : $"https://{currentIp}/cws{basePath}";

            var response = JsonConvert.SerializeObject(new RoutesResponseObject()
            {
                Url = path,
                Routes = routeCollection
            });

            context.Response.StatusCode = 200;
            context.Response.ContentType = "application/json";
            context.Response.Headers.Add("Content-Type", "application/json");
            context.Response.Write(response, false);
            context.Response.End();
        }
    }

    /// <summary>
    /// Represents a RoutesResponseObject
    /// </summary>
    public class  RoutesResponseObject 
    {
        [JsonProperty("url")]
        /// <summary>
        /// Gets or sets the Url
        /// </summary>
        public string Url { set; get; }

        [JsonProperty("routes")]
        /// <summary>
        /// Gets or sets the Routes
        /// </summary>
        public HttpCwsRouteCollection Routes { get; set; }
    }
}
