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

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="routeCollection"></param>
        /// <param name="basePath"></param>
        public GetRoutesHandler(HttpCwsRouteCollection routeCollection, string basePath) {
            this.routeCollection = routeCollection;
            this.basePath = basePath;
        }

        /// <summary>
        /// Handles GET method requests
        /// </summary>
        /// <param name="context"></param>
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
        /// <summary>
        /// Gets or sets the Url
        /// </summary>
        [JsonProperty("url")]
        public string Url { set; get; }

        /// <summary>
        /// Gets or sets the Routes
        /// </summary>
        [JsonProperty("routes")]
        public HttpCwsRouteCollection Routes { get; set; }
    }
}
