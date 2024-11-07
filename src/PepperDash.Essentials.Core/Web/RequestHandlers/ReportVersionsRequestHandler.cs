using System.Linq;
using Crestron.SimplSharp.WebScripting;
using Newtonsoft.Json;
using PepperDash.Core.Web.RequestHandlers;
using PepperDash.Essentials.Core.Plugins;

namespace PepperDash.Essentials.Core.Web.RequestHandlers
{
	public class ReportVersionsRequestHandler : WebApiBaseRequestHandler
	{
		/// <summary>
		/// Constructor
		/// </summary>
		/// <remarks>
		/// base(true) enables CORS support by default
		/// </remarks>
		public ReportVersionsRequestHandler()
			: base(true)
		{
		}

		/// <summary>
		/// Handles GET method requests
		/// </summary>
		/// <param name="context"></param>
		protected override void HandleGet(HttpCwsContext context)
		{
			var loadAssemblies = PluginLoader.LoadedAssemblies;
			if (loadAssemblies == null)
			{
				context.Response.StatusCode = 500;
				context.Response.StatusDescription = "Internal Server Error";
				context.Response.End();

				return;
			}

			var assemblies = loadAssemblies.Select(a => EssentialsWebApiHelpers.MapToAssemblyObject(a)).ToList();

			var js = JsonConvert.SerializeObject(assemblies, Formatting.Indented);
			
			context.Response.StatusCode = 200;
			context.Response.StatusDescription = "OK";
			context.Response.ContentType = "application/json";
			context.Response.ContentEncoding = System.Text.Encoding.UTF8;
			context.Response.Write(js, false);			
			context.Response.End();
		}
	}
}