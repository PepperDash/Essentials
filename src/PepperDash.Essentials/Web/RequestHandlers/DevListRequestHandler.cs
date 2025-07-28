using System.Linq;
using Crestron.SimplSharp.WebScripting;
using Newtonsoft.Json;
using PepperDash.Essentials.Core.Devices;

namespace PepperDash.Essentials.Core.Web.RequestHandlers
{
 /// <summary>
 /// Represents a DevListRequestHandler
 /// </summary>
	public class DevListRequestHandler : WebApiBaseRequestHandler
	{
		/// <summary>
		/// Constructor
		/// </summary>
		/// <remarks>
		/// base(true) enables CORS support by default
		/// </remarks>
		public DevListRequestHandler()
			: base(true)
		{
		}

		/// <summary>
		/// Handles GET method requests
		/// </summary>
		/// <param name="context"></param>
		protected override void HandleGet(HttpCwsContext context)
		{
			var allDevices = DeviceManager.AllDevices;
			if (allDevices == null)
			{
				context.Response.StatusCode = 404;
				context.Response.StatusDescription = "Not Found";
				context.Response.End();

				return;
			}

			allDevices.Sort((a, b) => string.Compare(a.Key, b.Key, System.StringComparison.Ordinal));

			var deviceList = allDevices.Select(d => EssentialsWebApiHelpers.MapToDeviceListObject(d)).ToList();

			var js = JsonConvert.SerializeObject(deviceList, Formatting.Indented);			

			context.Response.StatusCode = 200;
			context.Response.StatusDescription = "OK";
			context.Response.ContentType = "application/json";
			context.Response.ContentEncoding = System.Text.Encoding.UTF8;
			context.Response.Write(js, false);
			context.Response.End();
		}
	}
}