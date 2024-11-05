using System;
using Crestron.SimplSharp.WebScripting;
using Newtonsoft.Json;
using PepperDash.Core;
using PepperDash.Core.Web.RequestHandlers;
using Serilog.Events;

namespace PepperDash.Essentials.Core.Web.RequestHandlers
{
	public class DevJsonRequestHandler : WebApiBaseRequestHandler
	{
		/// <summary>
		/// Constructor
		/// </summary>
		/// <remarks>
		/// base(true) enables CORS support by default
		/// </remarks>
		public DevJsonRequestHandler()
			: base(true)
		{
		}

		/// <summary>
		/// Handles POST method requests
		/// </summary>
		/// <param name="context"></param>
		protected override void HandlePost(HttpCwsContext context)
		{
            var routeData = context.Request.RouteData;

            if(routeData == null)
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

			if (context.Request.ContentLength < 0)
			{
				context.Response.StatusCode = 400;
				context.Response.StatusDescription = "Bad Request: no body";
				context.Response.End();

				return;
			}

			var data = context.Request.GetRequestBody();

			if (string.IsNullOrEmpty(data))
			{
				context.Response.StatusCode = 400;
				context.Response.StatusDescription = "Bad Request: no body";
				context.Response.End();

				return;
			}
			
			try
			{                
                var daw = new DeviceActionWrapper { DeviceKey = (string) deviceKey};

                JsonConvert.PopulateObject(data, daw);
                Debug.LogMessage<DevJsonRequestHandler>(LogEventLevel.Verbose, "Device Action Wrapper: {@wrapper}", null, daw);

				DeviceJsonApi.DoDeviceAction(daw);

				context.Response.StatusCode = 200;
				context.Response.StatusDescription = "OK";
				context.Response.End();
			}
			catch (Exception ex)
			{
				Debug.LogError<DevJsonRequestHandler>(ex, "Error handling device command: {Exception}");				

				context.Response.StatusCode = 400;
				context.Response.StatusDescription = "Bad Request";
                context.Response.Write(JsonConvert.SerializeObject(new { error = ex.Message }), false);
				context.Response.End();
			}
		}
	}
}