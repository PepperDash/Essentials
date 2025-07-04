using Crestron.SimplSharp.WebScripting;
using PepperDash.Core.Web.RequestHandlers;

namespace PepperDash.Essentials.Core.Web.RequestHandlers;

/// <summary>
/// Handles requests to load the config. This is used when the config is loaded via the web API instead of at startup. This is typically used in conjunction with the SaveConfigRequestHandler to allow for saving and loading of the config via the web API.
/// </summary>
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
		if (cs != null)
			cs.GoWithLoad();

		context.Response.StatusCode = 200;
		context.Response.StatusDescription = "OK";
		context.Response.Write(message, false);
		context.Response.End();
	}
}