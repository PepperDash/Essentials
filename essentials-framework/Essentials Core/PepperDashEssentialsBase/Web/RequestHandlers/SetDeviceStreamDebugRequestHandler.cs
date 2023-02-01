﻿using System;
using Crestron.SimplSharp.WebScripting;
using Newtonsoft.Json;
using PepperDash.Core;
using PepperDash.Core.Web.RequestHandlers;

namespace PepperDash.Essentials.Core.Web.RequestHandlers
{
	public class SetDeviceStreamDebugRequestHandler : WebApiBaseRequestHandler
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
			context.Response.StatusCode = 501;
			context.Response.StatusDescription = "Not Implemented";
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
			if (context.Request.ContentLength < 0)
			{
				context.Response.StatusCode = 400;
				context.Response.StatusDescription = "Bad Request";
				context.Response.End();

				return;
			}

			var data = EssentialsWebApiHelpers.GetRequestBody(context.Request);
			if (data == null)
			{
				context.Response.StatusCode = 500;
				context.Response.StatusDescription = "Internal Server Error";
				context.Response.End();

				return;
			}

			var config = new SetDeviceStreamDebugConfig();
			var body = JsonConvert.DeserializeAnonymousType(data, config);
			if (body == null)
			{
				context.Response.StatusCode = 500;
				context.Response.StatusDescription = "Internal Server Error";
				context.Response.End();

				return;
			}

			if (string.IsNullOrEmpty(body.DeviceKey) || string.IsNullOrEmpty(body.Setting))
			{
				context.Response.StatusCode = 400;
				context.Response.StatusDescription = "Bad Request";
				context.Response.End();

				return;
			}

			var device = DeviceManager.GetDeviceForKey(body.DeviceKey) as IStreamDebugging;
			if (device == null)
			{
				context.Response.StatusCode = 404;
				context.Response.StatusDescription = "Not Found";
				context.Response.End();

				return;
			}
			
			eStreamDebuggingSetting debugSetting;
			try
			{
				debugSetting = (eStreamDebuggingSetting) Enum.Parse(typeof (eStreamDebuggingSetting), body.Setting, true);				
			}
			catch (Exception ex)
			{
				context.Response.StatusCode = 500;
				context.Response.StatusDescription = "Internal Server Error";
				context.Response.End();

				return;
			}

			try
			{
				var mins = Convert.ToUInt32(body.Timeout);
				if (mins > 0)
				{
					device.StreamDebugging.SetDebuggingWithSpecificTimeout(debugSetting, mins);
				}
				else
				{
					device.StreamDebugging.SetDebuggingWithDefaultTimeout(debugSetting);
				}

				context.Response.StatusCode = 200;
				context.Response.StatusDescription = "OK";
				context.Response.End();
			}
			catch (Exception ex)
			{
				context.Response.StatusCode = 500;
				context.Response.StatusDescription = "Internal Server Error";
				context.Response.End();
			}
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


	public class SetDeviceStreamDebugConfig
	{
		[JsonProperty("deviceKey", NullValueHandling = NullValueHandling.Include)]
		public string DeviceKey { get; set; }

		[JsonProperty("setting", NullValueHandling = NullValueHandling.Include)]
		public string Setting { get; set; }

		[JsonProperty("timeout")]
		public int Timeout { get; set; }

		public SetDeviceStreamDebugConfig()
		{
			DeviceKey = null;
			Setting = null;
			Timeout = 15;
		}
	}
}