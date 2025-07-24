using Crestron.SimplSharp;
using Crestron.SimplSharp.WebScripting;
using Newtonsoft.Json;
using PepperDash.Core.Web.RequestHandlers;
using PepperDash.Essentials.Core.Web.Attributes;

namespace PepperDash.Essentials.Core.Web.RequestHandlers
{
	[HttpGet]
	[OpenApiOperation(
		Summary = "GetPaths",
		Description = "Get available API paths and routes",
		OperationId = "getApiPaths")]
	[OpenApiResponse(200, Description = "Successful response")]
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

    public class  RoutesResponseObject 
    {
        [JsonProperty("url")]
        public string Url { set; get; }

        [JsonProperty("routes")]
        public HttpCwsRouteCollection Routes { get; set; }
    }
}
