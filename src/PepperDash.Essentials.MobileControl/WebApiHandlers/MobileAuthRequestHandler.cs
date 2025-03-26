using Crestron.SimplSharp.WebScripting;
using Newtonsoft.Json;
using PepperDash.Core;
using PepperDash.Core.Web.RequestHandlers;
using PepperDash.Essentials.Core.Web;
using System;
using System.Threading.Tasks;

namespace PepperDash.Essentials.WebApiHandlers
{
    public class MobileAuthRequestHandler : WebApiBaseRequestAsyncHandler
    {
        private readonly MobileControlSystemController mcController;

        public MobileAuthRequestHandler(MobileControlSystemController controller) : base(true)
        {
            mcController = controller;
        }

        protected override async Task HandlePost(HttpCwsContext context)
        {
            try
            {
                var requestBody = EssentialsWebApiHelpers.GetRequestBody(context.Request);

                var grantCode = JsonConvert.DeserializeObject<AuthorizationRequest>(requestBody);

                if (string.IsNullOrEmpty(grantCode?.GrantCode))
                {
                    Debug.LogMessage(Serilog.Events.LogEventLevel.Error, "Missing grant code");
                    context.Response.StatusCode = 400;
                    context.Response.StatusDescription = "Missing grant code";
                    context.Response.End();
                    return;
                }

                var response = await mcController.ApiService.SendAuthorizationRequest(mcController.Host, grantCode.GrantCode, mcController.SystemUuid);

                Debug.LogMessage(Serilog.Events.LogEventLevel.Debug, $"response received");
                if (response.Authorized)
                {
                    mcController.RegisterSystemToServer();
                }


                context.Response.StatusCode = 200;
                var responseBody = JsonConvert.SerializeObject(response, Formatting.None);
                context.Response.ContentType = "application/json";
                context.Response.Headers.Add("Content-Type", "application/json");
                context.Response.Write(responseBody, false);
                context.Response.End();
            }
            catch (Exception ex)
            {
                Debug.LogMessage(ex, "Exception recieved authorizing system");
            }
        }
    }
}
