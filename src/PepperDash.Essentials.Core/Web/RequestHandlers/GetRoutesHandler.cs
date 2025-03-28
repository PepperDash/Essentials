using Crestron.SimplSharp.WebScripting;
using Newtonsoft.Json;
using PepperDash.Core.Web.RequestHandlers;

namespace PepperDash.Essentials.Core.Web.RequestHandlers
{
    public class GetRoutesHandler:WebApiBaseRequestHandler
    {
        private HttpCwsRouteCollection routeCollection;
        public GetRoutesHandler(HttpCwsRouteCollection routeCollection) {
            this.routeCollection = routeCollection;
        }

        protected override void HandleGet(HttpCwsContext context)
        {
            var response = JsonConvert.SerializeObject(routeCollection);

            context.Response.StatusCode = 200;
            context.Response.ContentType = "application/json";
            context.Response.Headers.Add("Content-Type", "application/json");
            context.Response.Write(response, false);
            context.Response.End();
        }
    }
}
