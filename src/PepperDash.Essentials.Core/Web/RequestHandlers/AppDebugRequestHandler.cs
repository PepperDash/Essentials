using Crestron.SimplSharp.WebScripting;
using Newtonsoft.Json;
using PepperDash.Core;
using PepperDash.Core.Web.RequestHandlers;
using System;
using Serilog.Events;

namespace PepperDash.Essentials.Core.Web.RequestHandlers
{
	public class AppDebugRequestHandler : WebApiBaseRequestHandler
	{
		/// <summary>
		/// Constructor
		/// </summary>
		/// <remarks>
		/// base(true) enables CORS support by default
		/// </remarks>		
		public AppDebugRequestHandler()
			: base(true)
		{
		}

		/// <summary>
		/// Handles GET method requests
		/// </summary>
		/// <param name="context"></param>
		protected override void HandleGet(HttpCwsContext context)
		{
			var appDebug = new AppDebug { MinimumLevel = Debug.WebsocketMinimumLogLevel };

			var body = JsonConvert.SerializeObject((appDebug, Formatting.Indented, new JsonSerializerSettings( ));

			context.Response.StatusCode = 200;
			context.Response.StatusDescription = "OK";
			context.Response.Write(body, false);
			context.Response.End();
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

			var appDebug = new AppDebug();
			var requestBody = JsonConvert.DeserializeObject<AppDebug>(data);

			Debug.SetWebSocketMinimumDebugLevel(requestBody.MinimumLevel);

			appDebug.MinimumLevel = Debug.WebsocketMinimumLogLevel;
			var responseBody = JsonConvert.SerializeObject(appDebug, Formatting.Indented);

			context.Response.StatusCode = 200;
			context.Response.StatusDescription = "OK";
			context.Response.Write(responseBody, false);
			context.Response.End();
		}
	}

	public class AppDebug
	{
		[JsonProperty("minimumLevel", NullValueHandling = NullValueHandling.Ignore)]
		public LogEventLevel MinimumLevel { get; set; }
	}
}