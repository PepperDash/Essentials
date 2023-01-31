using System;
using System.Text;
using Crestron.SimplSharp.WebScripting;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using PepperDash.Core;
using PepperDash.Core.Web.RequestHandlers;

namespace PepperDash.Essentials.Core.Web.RequestHandlers
{
	public class SetDeviceStreamDebugRequestHandler : WebApiBaseRequestHandler
	{
		private const string Key = "SetDeviceStreamDebugRequestHandler";
		private const uint Trace = 0;
		private const uint Info = 0;
		private const uint Verbose = 0;

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
			context.Response.StatusCode = 501;
			context.Response.StatusDescription = "Not Implemented";
			context.Response.End();
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
			if (context.Request.ContentLength < 0) return;

			var bytes = new Byte[context.Request.ContentLength];
			context.Request.InputStream.Read(bytes, 0, context.Request.ContentLength);
			var data = Encoding.UTF8.GetString(bytes, 0, bytes.Length);
			Debug.Console(Info, "[{0}] Request data:\n{1}", Key.ToLower(), data);

			var o = new
			{
				DeviceKey = "",
				Type = "",
				Timeout = 15
			};

			var body = JsonConvert.DeserializeAnonymousType(data, o);

			if (string.IsNullOrEmpty(body.DeviceKey) || string.IsNullOrEmpty(body.Type) 
				|| !body.Type.ToLower().Contains("off")
				|| !body.Type.ToLower().Contains("tx")
				|| !body.Type.ToLower().Contains("rx")
				|| !body.Type.ToLower().Contains("both"))
			{
				context.Response.StatusCode = 400;
				context.Response.StatusDescription = "Bad Request";
				context.Response.End();

				return;
			}

			DeviceManager.SetDeviceStreamDebugging(string.Format("setdevicestreamdebug {0} {1} {2}", body.DeviceKey, body.Type, body.Timeout));

			context.Response.StatusCode = 200;
			context.Response.StatusDescription = "OK";
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