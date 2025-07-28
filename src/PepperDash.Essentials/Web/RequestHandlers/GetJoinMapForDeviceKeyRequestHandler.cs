using Crestron.SimplSharp.WebScripting;
using Newtonsoft.Json;
using PepperDash.Essentials.Core.Bridges;
using PepperDash.Essentials.Core.Devices;
using PepperDash.Essentials.Core.JoinMaps;

namespace PepperDash.Essentials.Core.Web.RequestHandlers
{
 /// <summary>
 /// Represents a GetJoinMapForDeviceKeyRequestHandler
 /// </summary>
	public class GetJoinMapForDeviceKeyRequestHandler : WebApiBaseRequestHandler
	{
		/// <summary>
		/// Constructor
		/// </summary>
		/// <remarks>
		/// base(true) enables CORS support by default
		/// </remarks>
		public GetJoinMapForDeviceKeyRequestHandler()
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

			object deviceObj;
			if (!routeData.Values.TryGetValue("deviceKey", out deviceObj))
			{
				context.Response.StatusCode = 400;
				context.Response.StatusDescription = "Bad Request";
				context.Response.End();

				return;
			}

			var bridge = DeviceManager.GetDeviceForKey(bridgeObj.ToString()) as EiscApiAdvanced;
			if (bridge == null)
			{
				context.Response.StatusCode = 404;
				context.Response.StatusDescription = "Not Found";
				context.Response.End();

				return;
			}

			JoinMapBaseAdvanced deviceJoinMap;
			if (!bridge.JoinMaps.TryGetValue(deviceObj.ToString(), out deviceJoinMap))
			{
				context.Response.StatusCode = 500;
				context.Response.StatusDescription = "Internal Server Error";
				context.Response.End();

				return;
			}

			var joinMap = EssentialsWebApiHelpers.MapJoinToObject(deviceObj.ToString(), deviceJoinMap);
			var js = JsonConvert.SerializeObject(joinMap, Formatting.Indented, new JsonSerializerSettings
			{
				ReferenceLoopHandling = ReferenceLoopHandling.Ignore,
				NullValueHandling = NullValueHandling.Ignore,
				MissingMemberHandling = MissingMemberHandling.Ignore,
				DefaultValueHandling = DefaultValueHandling.Ignore,
				TypeNameHandling = TypeNameHandling.None
			});
			
			context.Response.StatusCode = 200;
			context.Response.StatusDescription = "OK";
			context.Response.ContentType = "application/json";
			context.Response.ContentEncoding = System.Text.Encoding.UTF8;
			context.Response.Write(js, false);
			context.Response.End();
		}
	}
}