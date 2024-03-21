using System;
using Crestron.SimplSharp.WebScripting;
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
				context.Response.StatusDescription = "Bad Request";
				context.Response.End();

				return;
			}

			var data = EssentialsWebApiHelpers.GetRequestBody(context.Request);
			if (string.IsNullOrEmpty(data))
			{
				context.Response.StatusCode = 400;
				context.Response.StatusDescription = "Bad Request";
				context.Response.End();

				return;
			}
			
			try
			{
				DeviceJsonApi.DoDeviceActionWithJson(data);

				context.Response.StatusCode = 200;
				context.Response.StatusDescription = "OK";
				context.Response.End();
			}
			catch (Exception ex)
			{
				Debug.LogMessage(LogEventLevel.Error, "Exception Message: {0}", ex.Message);
				Debug.LogMessage(LogEventLevel.Verbose, "Exception Stack Trace: {0}", ex.StackTrace);
				if(ex.InnerException != null) Debug.LogMessage(LogEventLevel.Error, "Exception Inner: {0}", ex.InnerException);

				context.Response.StatusCode = 400;
				context.Response.StatusDescription = "Bad Request";
				context.Response.End();
			}
		}
	}
}