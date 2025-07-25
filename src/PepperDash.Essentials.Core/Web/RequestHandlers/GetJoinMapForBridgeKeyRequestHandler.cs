using System.Linq;
using Crestron.SimplSharp.WebScripting;
using Newtonsoft.Json;
using PepperDash.Core.Web.RequestHandlers;
using PepperDash.Essentials.Core.Bridges;

namespace PepperDash.Essentials.Core.Web.RequestHandlers
{
 /// <summary>
 /// Represents a GetJoinMapForBridgeKeyRequestHandler
 /// </summary>
	public class GetJoinMapForBridgeKeyRequestHandler : WebApiBaseRequestHandler
	{
		/// <summary>
		/// Constructor
		/// </summary>
		/// <remarks>
		/// base(true) enables CORS support by default
		/// </remarks>
		public GetJoinMapForBridgeKeyRequestHandler()
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

			object bridgeObj;			
			if (!routeData.Values.TryGetValue("bridgeKey", out bridgeObj))
			{
				context.Response.StatusCode = 400;
				context.Response.StatusDescription = "Bad Request";
				context.Response.End();

				return;
			}

			var bridge = DeviceManager.GetDeviceForKey(bridgeObj.ToString()) as EiscApiAdvanced;
			if (bridge == null)
			{
				context.Response.StatusCode = 400;
				context.Response.StatusDescription = "Bad Request";
				context.Response.End();

				return;
			}

			var joinMap = bridge.JoinMaps.Select(j => EssentialsWebApiHelpers.MapJoinToObject(j)).ToList();			
			if (joinMap == null)
			{
				context.Response.StatusCode = 404;
				context.Response.StatusDescription = "Not Found";
				context.Response.End();

				return;
			}

			var js = JsonConvert.SerializeObject(joinMap, Formatting.Indented);

			context.Response.StatusCode = 200;
			context.Response.StatusDescription = "OK";
			context.Response.ContentType = "application/json";
			context.Response.ContentEncoding = System.Text.Encoding.UTF8;
			context.Response.Write(js, false);
			context.Response.End();
		}		
	}
}