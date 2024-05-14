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
			if (context.Request.ContentLength < 0)
			{
				context.Response.StatusCode = 400;
				context.Response.StatusDescription = "Bad Request: no body";
				context.Response.End();

				return;
			}

			var data = EssentialsWebApiHelpers.GetRequestBody(context.Request);
			if (string.IsNullOrEmpty(data))
			{
				context.Response.StatusCode = 400;
				context.Response.StatusDescription = "Bad Request: no body";
				context.Response.End();

				return;
			}
			
			try
			{
                var daw = JsonConvert.DeserializeObject<DeviceActionWrapper>(data);
				DeviceJsonApi.DoDeviceActionWithJson(data);

				context.Response.StatusCode = 200;
				context.Response.StatusDescription = "OK";
				context.Response.End();
			}
			catch (Exception ex)
			{
				Debug.LogMessage(ex, "Error handling device command: {Exception}");				

				context.Response.StatusCode = 400;
				context.Response.StatusDescription = "Bad Request";
                context.Response.Write(JsonConvert.SerializeObject(new { error = ex.Message }), false);
				context.Response.End();
			}
		}
	}
}