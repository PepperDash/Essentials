using System.Text;
using Crestron.SimplSharp.WebScripting;
using Newtonsoft.Json;
using PepperDash.Core.Web.RequestHandlers;
using PepperDash.Essentials.Core.Web.Attributes;

namespace PepperDash.Essentials.Core.Web.RequestHandlers
{
	[HttpGet]
	[OpenApiOperation(
		Summary = "DevProps",
		Description = "Retrieve properties for a specific device",
		OperationId = "getDeviceProperties")]
	[OpenApiParameter("deviceKey", Description = "The key of the device")]
	[OpenApiResponse(200, Description = "Successful response", ContentType = "application/json")]
	[OpenApiResponse(400, Description = "Bad Request")]
	[OpenApiResponse(404, Description = "Device not found")]
	public class DevPropsRequestHandler : WebApiBaseRequestHandler
	{
		/// <summary>
		/// Constructor
		/// </summary>
		/// <remarks>
		/// base(true) enables CORS support by default
		/// </remarks>
		public DevPropsRequestHandler()
			: base(true)
		{
		}

        /// <summary>
        /// Handles GET method requests
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

            object deviceObj;
            if (!routeData.Values.TryGetValue("deviceKey", out deviceObj))
            {
                context.Response.StatusCode = 400;
                context.Response.StatusDescription = "Bad Request";
                context.Response.End();

                return;
            }

            var device = DeviceManager.GetDeviceForKey(deviceObj.ToString());

            if (device == null)
            {
                context.Response.StatusCode = 404;
                context.Response.StatusDescription = "Device Not Found";
                context.Response.End();

                return;
            }

            var deviceProperties = DeviceJsonApi.GetProperties(device.Key);
            if (deviceProperties == null || deviceProperties.ToLower().Contains("no device"))
            {
                context.Response.StatusCode = 404;
                context.Response.StatusDescription = "Not Found";
                context.Response.End();

                return;
            }

            context.Response.StatusCode = 200;
            context.Response.StatusDescription = "OK";
            context.Response.ContentType = "application/json";
            context.Response.ContentEncoding = Encoding.UTF8;
            context.Response.Write(deviceProperties, false);
            context.Response.End();
        }
    }
}