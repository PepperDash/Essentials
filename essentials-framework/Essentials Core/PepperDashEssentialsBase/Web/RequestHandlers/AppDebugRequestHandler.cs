using System;
using System.Text;
using Crestron.SimplSharp.WebScripting;
using Newtonsoft.Json;
using PepperDash.Core;
using PepperDash.Core.Web.RequestHandlers;

namespace PepperDash.Essentials.Core.Web.RequestHandlers
{
	public class AppDebugRequestHandler : WebApiBaseRequestHandler
	{
		private const string Key = "AppDebugRequestHandler";
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
			var o = new AppDebug();
			o.Level = Debug.Level;

			var body = JsonConvert.SerializeObject(o, Formatting.Indented);

			context.Response.StatusCode = 200;
			context.Response.StatusDescription = "OK";
			context.Response.Write(body, false);
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
			//Debug.Console(Info, "[{0}] Request data:\n{1}", Key.ToLower(), data);

			var o = new AppDebug();
			var requestBody = JsonConvert.DeserializeAnonymousType(data, o);
			
			Debug.SetDebugLevel(requestBody.Level);

			o.Level = Debug.Level;

			var responseBody = JsonConvert.SerializeObject(o, Formatting.Indented);

			context.Response.StatusCode = 200;
			context.Response.StatusDescription = "OK";
			context.Response.Write(responseBody, false);
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

	public class AppDebug
	{
		[JsonProperty("level", NullValueHandling = NullValueHandling.Ignore)]
		public int Level { get; set; } 
	}
}