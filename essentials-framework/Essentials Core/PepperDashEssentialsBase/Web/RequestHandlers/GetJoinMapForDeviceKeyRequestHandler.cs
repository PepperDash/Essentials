﻿using Crestron.SimplSharp.WebScripting;
using Newtonsoft.Json;
using PepperDash.Core.Web.RequestHandlers;
using PepperDash.Essentials.Core.Bridges;

namespace PepperDash.Essentials.Core.Web.RequestHandlers
{
	public class GetJoinMapForDeviceKeyRequestHandler : WebApiBaseRequestHandler
	{
		/// <summary>
		/// Handles CONNECT method requests
		/// </summary>
		/// <param name="context"></param>
		protected override void HandleConnect(HttpCwsContext context)
		{
			context.Response.StatusCode = 501;
			context.Response.StatusDescription = "Not Implemented";
			context.Response.End();
		}

		/// <summary>
		/// Handles DELETE method requests
		/// </summary>
		/// <param name="context"></param>
		protected override void HandleDelete(HttpCwsContext context)
		{
			context.Response.StatusCode = 501;
			context.Response.StatusDescription = "Not Implemented";
			context.Response.End();
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

		/// <summary>
		/// Handles HEAD method requests
		/// </summary>
		/// <param name="context"></param>
		protected override void HandleHead(HttpCwsContext context)
		{
			context.Response.StatusCode = 501;
			context.Response.StatusDescription = "Not Implemented";
			context.Response.End();
		}

		/// <summary>
		/// Handles OPTIONS method requests
		/// </summary>
		/// <param name="context"></param>
		protected override void HandleOptions(HttpCwsContext context)
		{
			context.Response.StatusCode = 501;
			context.Response.StatusDescription = "Not Implemented";
			context.Response.End();
		}

		/// <summary>
		/// Handles PATCH method requests
		/// </summary>
		/// <param name="context"></param>
		protected override void HandlePatch(HttpCwsContext context)
		{
			context.Response.StatusCode = 501;
			context.Response.StatusDescription = "Not Implemented";
			context.Response.End();
		}

		/// <summary>
		/// Handles POST method requests
		/// </summary>
		/// <param name="context"></param>
		protected override void HandlePost(HttpCwsContext context)
		{
			context.Response.StatusCode = 501;
			context.Response.StatusDescription = "Not Implemented";
			context.Response.End();
		}

		/// <summary>
		/// Handles PUT method requests
		/// </summary>
		/// <param name="context"></param>
		protected override void HandlePut(HttpCwsContext context)
		{
			context.Response.StatusCode = 501;
			context.Response.StatusDescription = "Not Implemented";
			context.Response.End();
		}

		/// <summary>
		/// Handles TRACE method requests
		/// </summary>
		/// <param name="context"></param>
		protected override void HandleTrace(HttpCwsContext context)
		{
			context.Response.StatusCode = 501;
			context.Response.StatusDescription = "Not Implemented";
			context.Response.End();
		}
	}
}