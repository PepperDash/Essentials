using Crestron.SimplSharp.WebScripting;

namespace PepperDash.Essentials.Core
{
	/// <summary>
	/// Crestron CWS unknown request handler
	/// </summary>
	public class RequestHandlerUnknown : IHttpCwsHandler
	{
		
		public void ProcessRequest(HttpCwsContext context)
		{
			// TODO [ ] Modify unknown request handler 
			context.Response.StatusCode = 418;
			context.Response.ContentType = "application/json";
			context.Response.Write(string.Format("{0} {1}", context.Request.HttpMethod, context.Request.RawUrl), true);
		}
	}
}