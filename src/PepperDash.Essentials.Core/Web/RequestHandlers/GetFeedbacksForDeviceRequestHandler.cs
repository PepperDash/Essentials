using System.Linq;
using Crestron.SimplSharp.WebScripting;
using Newtonsoft.Json;
using PepperDash.Core.Web.RequestHandlers;
using PepperDash.Essentials.Core.Web.Attributes;

namespace PepperDash.Essentials.Core.Web.RequestHandlers
{
	[HttpGet]
	[OpenApiOperation(
		Summary = "GetFeedbacksForDeviceKey",
		Description = "Get feedback values from a specific device",
		OperationId = "getDeviceFeedbacks")]
	[OpenApiParameter("deviceKey", Description = "The key of the device to get feedbacks from")]
	[OpenApiResponse(200, Description = "Device feedback values")]
	[OpenApiResponse(400, Description = "Bad Request")]
	[OpenApiResponse(404, Description = "Device not found")]
	public class GetFeedbacksForDeviceRequestHandler : WebApiBaseRequestHandler
	{
		/// <summary>
		/// Constructor
		/// </summary>
		/// <remarks>
		/// base(true) enables CORS support by default
		/// </remarks>
		public GetFeedbacksForDeviceRequestHandler()
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

			object deviceObj;
			if (!routeData.Values.TryGetValue("deviceKey", out deviceObj))
			{
				context.Response.StatusCode = 400;
				context.Response.StatusDescription = "Bad Request";
				context.Response.End();

				return;
			}


			var device = DeviceManager.GetDeviceForKey(deviceObj.ToString()) as IHasFeedback;
			if (device == null)
			{
				context.Response.StatusCode = 404;
				context.Response.StatusDescription = "Not Found";
				context.Response.End();

				return;
			}

			var boolFeedback =
				from feedback in device.Feedbacks.OfType<BoolFeedback>()
				where !string.IsNullOrEmpty(feedback.Key)
				select new
				{
					FeedbackKey = feedback.Key,
					Value = feedback.BoolValue
				};

			var intFeedback =
				from feedback in device.Feedbacks.OfType<IntFeedback>()
				where !string.IsNullOrEmpty(feedback.Key)
				select new
				{
					FeedbackKey = feedback.Key,
					Value = feedback.IntValue
				};

			var stringFeedback = 
				from feedback in device.Feedbacks.OfType<StringFeedback>()
				where !string.IsNullOrEmpty(feedback.Key)
				select new
				{
					FeedbackKey = feedback.Key,
					Value = feedback.StringValue ?? string.Empty
				};

			var responseObj = new
			{
				BoolValues = boolFeedback,
				IntValues = intFeedback,
				SerialValues = stringFeedback
			};

			var js = JsonConvert.SerializeObject(responseObj, Formatting.Indented);

			context.Response.StatusCode = 200;
			context.Response.StatusDescription = "OK";
			context.Response.ContentType = "application/json";
			context.Response.ContentEncoding = System.Text.Encoding.UTF8;
			context.Response.Write(js, false);
			context.Response.End();
		}
	}
}