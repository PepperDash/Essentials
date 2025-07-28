using System.Linq;
using Crestron.SimplSharp.WebScripting;
using Newtonsoft.Json;
using PepperDash.Essentials.Core.Factory;

namespace PepperDash.Essentials.Core.Web.RequestHandlers
{
 /// <summary>
 /// Represents a GetTypesRequestHandler
 /// </summary>
	public class GetTypesRequestHandler : WebApiBaseRequestHandler
	{
		/// <summary>
		/// Constructor
		/// </summary>
		/// <remarks>
		/// base(true) enables CORS support by default
		/// </remarks>
		public GetTypesRequestHandler()
			: base(true)
		{
		}

		/// <summary>
		/// Handles GET method requests
		/// </summary>
		/// <param name="context"></param>
		protected override void HandleGet(HttpCwsContext context)
		{
			var routeData = context.Request.RouteData;
			if (routeData == null)
			{
				context.Response.StatusCode = 400;
				context.Response.StatusDescription = "Bad Request";
				context.Response.End();

				return;
			}

			var deviceFactory = DeviceFactory.GetDeviceFactoryDictionary(null);
			if (deviceFactory == null)
			{
				context.Response.StatusCode = 404;
				context.Response.StatusDescription = "Not Found";
				context.Response.End();

				return;
			}

			var deviceTypes = deviceFactory.Select(t => EssentialsWebApiHelpers.MapDeviceTypeToObject(t)).ToList();
			var js = JsonConvert.SerializeObject(deviceTypes, Formatting.Indented);

			context.Response.StatusCode = 200;
			context.Response.StatusDescription = "OK";
			context.Response.ContentType = "application/json";
			context.Response.ContentEncoding = System.Text.Encoding.UTF8;
			context.Response.Write(js, false);
			context.Response.End();
		}
	}
}