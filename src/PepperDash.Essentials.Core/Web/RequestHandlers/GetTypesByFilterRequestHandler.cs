using System.Linq;
using Crestron.SimplSharp.WebScripting;
using Newtonsoft.Json;
using PepperDash.Core.Web.RequestHandlers;
using PepperDash.Essentials.Core.Web.Attributes;

namespace PepperDash.Essentials.Core.Web.RequestHandlers
{
	[HttpGet]
	[OpenApiOperation(
		Summary = "GetTypesByFilter",
		Description = "Retrieve device types filtered by a specific category",
		OperationId = "getDeviceTypesByFilter")]
	[OpenApiParameter("filter", Description = "The filter criteria for device types")]
	[OpenApiResponse(200, Description = "Successful response", ContentType = "application/json")]
	[OpenApiResponse(400, Description = "Bad Request")]
	[OpenApiResponse(404, Description = "Filtered device types not found")]
	public class GetTypesByFilterRequestHandler : WebApiBaseRequestHandler
	{
		/// <summary>
		/// Constructor
		/// </summary>
		/// <remarks>
		/// base(true) enables CORS support by default
		/// </remarks>
		public GetTypesByFilterRequestHandler()
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

			object filterObj;
			if (!routeData.Values.TryGetValue("filter", out filterObj))
			{
				context.Response.StatusCode = 400;
				context.Response.StatusDescription = "Bad Request";
				context.Response.End();

				return;
			}

			var deviceFactory = DeviceFactory.GetDeviceFactoryDictionary(filterObj.ToString());
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