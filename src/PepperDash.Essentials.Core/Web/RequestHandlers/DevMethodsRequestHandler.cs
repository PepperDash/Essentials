using System.Text;
using Crestron.SimplSharp.WebScripting;
using Newtonsoft.Json;
using PepperDash.Core;
using PepperDash.Core.Web.RequestHandlers;

namespace PepperDash.Essentials.Core.Web.RequestHandlers;

	public class DevMethodsRequestHandler : WebApiBaseRequestHandler
	{
		/// <summary>
		/// Constructor
		/// </summary>
		/// <remarks>
		/// base(true) enables CORS support by default
		/// </remarks>
		public DevMethodsRequestHandler()
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
        Debug.LogMessage(Serilog.Events.LogEventLevel.Debug, "Getting DevMethods: {@routeData}", routeData);
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

        var deviceMethods = DeviceJsonApi.GetMethods(device.Key);
			if (deviceMethods == null || deviceMethods.ToLower().Contains("no device"))
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
			context.Response.Write(deviceMethods, false);
			context.Response.End();
		}
	}