using Crestron.SimplSharp;
using Crestron.SimplSharp.WebScripting;
using Newtonsoft.Json;
using PepperDash.Core;
using PepperDash.Core.Web.RequestHandlers;
using PepperDash.Essentials.Core.Web.Attributes;

namespace PepperDash.Essentials.Core.Web.RequestHandlers
{
	[HttpPost]
	[OpenApiOperation(
		Summary = "Load Config",
		Description = "Load configuration",
		OperationId = "loadConfig")]
	[OpenApiResponse(200, Description = "Configuration load initiated successfully")]
	public class LoadConfigRequestHandler : WebApiBaseRequestHandler
	{
		/// <summary>
		/// Constructor
		/// </summary>
		/// <remarks>
		/// base(true) enables CORS support by default
		/// </remarks>		
		public LoadConfigRequestHandler()
			: base(true)
		{
		}
		
		/// <summary>
		/// Handles POST method requests
		/// </summary>
		/// <param name="context"></param>
		protected override void HandlePost(HttpCwsContext context)
		{
			var message = "";
			var cs = Global.ControlSystem as ILoadConfig;
			if(cs != null)
                cs.GoWithLoad();
            
            context.Response.StatusCode = 200;
            context.Response.StatusDescription = "OK";
			context.Response.Write(message, false);
            context.Response.End();
        }
	}
}