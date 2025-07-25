using Crestron.SimplSharp.WebScripting;
using PepperDash.Core.Web.RequestHandlers;
using PepperDash.Essentials.Core.Web.Attributes;

namespace PepperDash.Essentials.Core.Web.RequestHandlers
{
	[HttpPost]
	[OpenApiOperation(
		Summary = "DisableAllStreamDebug",
		Description = "Disable stream debugging for all devices",
		OperationId = "disableAllStreamDebug")]
	[OpenApiResponse(200, Description = "Successful response")]
	public class DisableAllStreamDebugRequestHandler : WebApiBaseRequestHandler
	{
		/// <summary>
		/// Constructor
		/// </summary>
		/// <remarks>
		/// base(true) enables CORS support by default
		/// </remarks>
		public DisableAllStreamDebugRequestHandler()
			: base(true)
		{
		}

		/// <summary>
		/// Handles POST method requests
		/// </summary>
		/// <param name="context"></param>
		protected override void HandlePost(HttpCwsContext context)
		{
			DeviceManager.DisableAllDeviceStreamDebugging();

			context.Response.StatusCode = 200;
			context.Response.StatusDescription = "OK";
			context.Response.End();
		}
	}
}