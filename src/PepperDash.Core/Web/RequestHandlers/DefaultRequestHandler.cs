using Crestron.SimplSharp.WebScripting;

namespace PepperDash.Core.Web.RequestHandlers
{
	/// <summary>
	/// Web API default request handler
	/// </summary>
 /// <summary>
 /// Represents a DefaultRequestHandler
 /// </summary>
	public class DefaultRequestHandler : WebApiBaseRequestHandler
	{
		/// <summary>
		/// Constructor
		/// </summary>
		public DefaultRequestHandler()
			: base(true)
		{ }
	}
}