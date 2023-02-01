using System.Linq;
using Crestron.SimplSharp.WebScripting;
using Newtonsoft.Json;
using PepperDash.Core.Web.RequestHandlers;

namespace PepperDash.Essentials.Core.Web.RequestHandlers
{
	public class ReportVersionsRequestHandler : WebApiBaseRequestHandler
	{
		private const string Key = "ReportVersionsRequestHandler";
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
			var assemblies = PluginLoader.LoadedAssemblies.Select(assembly => new
			{
				Name = assembly.Name,
				Version = assembly.Version
			}).Cast<object>().ToList();

			var js = JsonConvert.SerializeObject(assemblies, Formatting.Indented, new JsonSerializerSettings
			{
				ReferenceLoopHandling = ReferenceLoopHandling.Ignore,
				NullValueHandling = NullValueHandling.Ignore,
				MissingMemberHandling = MissingMemberHandling.Ignore,
				DefaultValueHandling = DefaultValueHandling.Ignore,
				TypeNameHandling = TypeNameHandling.None
			});
			//Debug.Console(Verbose, "[{0}] HandleGet: \x0d\x0a{1}", Key.ToLower(), js);

			context.Response.StatusCode = 200;
			context.Response.StatusDescription = "OK";
			context.Response.ContentType = "application/json";
			context.Response.ContentEncoding = System.Text.Encoding.UTF8;
			context.Response.Write(js, false);			
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