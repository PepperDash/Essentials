using Crestron.SimplSharp;
using Crestron.SimplSharp.WebScripting;
using PepperDash.Core.Web.RequestHandlers;

namespace PepperDash.Essentials.Core.Web.RequestHandlers
{
 /// <summary>
 /// Represents a RestartProgramRequestHandler
 /// </summary>
	public class RestartProgramRequestHandler : WebApiBaseRequestHandler
	{
		/// <summary>
		/// Constructor
		/// </summary>
		/// <remarks>
		/// base(true) enables CORS support by default
		/// </remarks>		
		public RestartProgramRequestHandler()
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
			if(CrestronEnvironment.DevicePlatform == eDevicePlatform.Appliance)
				CrestronConsole.SendControlSystemCommand($"progres -p:{InitialParametersClass.ApplicationNumber}", ref message);
			
            context.Response.StatusCode = 200;
            context.Response.StatusDescription = "OK";
			context.Response.Write(message, false);
            context.Response.End();
        }
	}
}