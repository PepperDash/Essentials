using Crestron.SimplSharp.WebScripting;
using PepperDash.Core.Web.RequestHandlers;

namespace PepperDash.Essentials.Core.Web.RequestHandlers
{
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