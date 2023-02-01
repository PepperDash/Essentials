using System;
using System.Collections.Generic;
using System.Linq;
using Crestron.SimplSharp.WebScripting;
using Newtonsoft.Json;
using PepperDash.Core;
using PepperDash.Core.Web.RequestHandlers;

namespace PepperDash.Essentials.Core.Web.RequestHandlers
{
	public class GetTypesRequestHandler : WebApiBaseRequestHandler
	{
		private const string Key = "GetTypesRequestHandler";
		private const uint Trace = 0;
		private const uint Info = 1;
		private const uint Verbose = 2;

		/// <summary>
		/// Handles CONNECT method requests
		/// </summary>
		/// <param name="context"></param>
		protected override void HandleConnect(HttpCwsContext context)
		{
			context.Response.StatusCode = 501;
			context.Response.StatusDescription = "Not Implemented";
			context.Response.End();
		}

		/// <summary>
		/// Handles DELETE method requests
		/// </summary>
		/// <param name="context"></param>
		protected override void HandleDelete(HttpCwsContext context)
		{
			context.Response.StatusCode = 501;
			context.Response.StatusDescription = "Not Implemented";
			context.Response.End();
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

			var routeDataJson = JsonConvert.SerializeObject(routeData, Formatting.Indented);
			Debug.Console(Verbose, "[{0}] routeData:\n{1}", Key.ToLower(), routeDataJson);

			var types = DeviceFactory.GetDeviceFactoryDictionary(string.Empty).Select(type => new
			{
				Type = type.Key,
				Description = type.Value.Description,
				CType = type.Value.CType == null ? "---" : type.Value.CType.ToString()
			}).Cast<object>().ToList();

			if (types == null)
			{
				context.Response.StatusCode = 400;
				context.Response.StatusDescription = "Bad Request";
				context.Response.End();

				return;
			}

			try
			{
				var js = JsonConvert.SerializeObject(types, Formatting.Indented);
				//Debug.Console(Verbose, "[{0}] HandleGet: \x0d\x0a{1}", Key.ToLower(), js);

				context.Response.StatusCode = 200;
				context.Response.StatusDescription = "OK";
				context.Response.ContentType = "application/json";
				context.Response.ContentEncoding = System.Text.Encoding.UTF8;
				context.Response.Write(js, false);
				context.Response.End();
			}
			catch (Exception ex)
			{
				Debug.Console(Info, "[{0}] HandleGet Exception Message: {1}", Key.ToLower(), ex.Message);
				Debug.Console(Verbose, "[{0}] HandleGet Exception StackTrace: {1}", Key.ToLower(), ex.StackTrace);
				if (ex.InnerException != null) Debug.Console(Verbose, "[{0}] HandleGet Exception InnerException: {1}", Key.ToLower(), ex.InnerException);

				context.Response.StatusCode = 500;
				context.Response.StatusDescription = "Internal Server Error";
				context.Response.End();
			}
		}

		/// <summary>
		/// Handles HEAD method requests
		/// </summary>
		/// <param name="context"></param>
		protected override void HandleHead(HttpCwsContext context)
		{
			context.Response.StatusCode = 501;
			context.Response.StatusDescription = "Not Implemented";
			context.Response.End();
		}

		/// <summary>
		/// Handles OPTIONS method requests
		/// </summary>
		/// <param name="context"></param>
		protected override void HandleOptions(HttpCwsContext context)
		{
			context.Response.StatusCode = 501;
			context.Response.StatusDescription = "Not Implemented";
			context.Response.End();
		}

		/// <summary>
		/// Handles PATCH method requests
		/// </summary>
		/// <param name="context"></param>
		protected override void HandlePatch(HttpCwsContext context)
		{
			context.Response.StatusCode = 501;
			context.Response.StatusDescription = "Not Implemented";
			context.Response.End();
		}

		/// <summary>
		/// Handles POST method requests
		/// </summary>
		/// <param name="context"></param>
		protected override void HandlePost(HttpCwsContext context)
		{
			context.Response.StatusCode = 501;
			context.Response.StatusDescription = "Not Implemented";
			context.Response.End();
		}

		/// <summary>
		/// Handles PUT method requests
		/// </summary>
		/// <param name="context"></param>
		protected override void HandlePut(HttpCwsContext context)
		{
			context.Response.StatusCode = 501;
			context.Response.StatusDescription = "Not Implemented";
			context.Response.End();
		}

		/// <summary>
		/// Handles TRACE method requests
		/// </summary>
		/// <param name="context"></param>
		protected override void HandleTrace(HttpCwsContext context)
		{
			context.Response.StatusCode = 501;
			context.Response.StatusDescription = "Not Implemented";
			context.Response.End();
		}
	}
}