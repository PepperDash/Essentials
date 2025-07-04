using Crestron.SimplSharp.WebScripting;
using Newtonsoft.Json;
using PepperDash.Core.Web.RequestHandlers;
using PepperDash.Essentials.Core.Config;

namespace PepperDash.Essentials.Core.Web.RequestHandlers;

/// <summary>
/// Handles requests to show the current config. 
/// This is used to display the current config via the web API. 
/// The config is returned as a JSON string. 
/// </summary>
public class ShowConfigRequestHandler : WebApiBaseRequestHandler
{
	/// <summary>
	/// Constructor
	/// </summary>
	/// <remarks>
	/// base(true) enables CORS support by default
	/// </remarks>
	public ShowConfigRequestHandler()
		: base(true)
	{
	}

	/// <summary>
	/// Handles GET method requests
	/// </summary>
	/// <param name="context"></param>
	protected override void HandleGet(HttpCwsContext context)
	{
		var config = JsonConvert.SerializeObject(ConfigReader.ConfigObject, Formatting.Indented);

		context.Response.StatusCode = 200;
		context.Response.StatusDescription = "OK";
		context.Response.ContentType = "application/json";
		context.Response.ContentEncoding = System.Text.Encoding.UTF8;
		context.Response.Write(config, false);
		context.Response.End();
	}
}