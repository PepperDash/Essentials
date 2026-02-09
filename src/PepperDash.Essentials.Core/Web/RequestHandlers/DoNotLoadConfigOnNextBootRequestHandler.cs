using Crestron.SimplSharp.WebScripting;
using Newtonsoft.Json;
using PepperDash.Core;
using PepperDash.Core.Web.RequestHandlers;

namespace PepperDash.Essentials.Core.Web.RequestHandlers
{
	/// <summary>
	/// Represents a DoNotLoadConfigOnNextBootRequestHandler
	/// </summary>
	public class DoNotLoadConfigOnNextBootRequestHandler : WebApiBaseRequestHandler
	{
		/// <summary>
		/// Constructor
		/// </summary>
		/// <remarks>
		/// base(true) enables CORS support by default
		/// </remarks>		
		public DoNotLoadConfigOnNextBootRequestHandler()
			: base(true)
		{
		}

		/// <summary>
		/// Handles GET method requests
		/// </summary>
		/// <param name="context"></param>
		protected override void HandleGet(HttpCwsContext context)
		{
			var data = new Data
			{
				DoNotLoadConfigOnNextBoot = Debug.DoNotLoadConfigOnNextBoot
            };

			var body = JsonConvert.SerializeObject(data, Formatting.Indented);

			context.Response.StatusCode = 200;
			context.Response.StatusDescription = "OK";
			context.Response.Write(body, false);
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

			var data = context.Request.GetRequestBody();
			if (string.IsNullOrEmpty(data))
			{
				context.Response.StatusCode = 400;
				context.Response.StatusDescription = "Bad Request";
				context.Response.End();

				return;
			}

			var d = new Data();
			var requestBody = JsonConvert.DeserializeAnonymousType(data, d);

			Debug.SetDoNotLoadConfigOnNextBoot(requestBody.DoNotLoadConfigOnNextBoot);

			var responseBody = JsonConvert.SerializeObject(d, Formatting.Indented);

			context.Response.StatusCode = 200;
			context.Response.StatusDescription = "OK";
			context.Response.Write(responseBody, false);
			context.Response.End();
		}
	}

	/// <summary>
	/// Represents a Data
	/// </summary>
	public class Data
	{
		/// <summary>
		/// Gets or sets the DoNotLoadConfigOnNextBoot
		/// </summary>
		[JsonProperty("doNotLoadConfigOnNextBoot", NullValueHandling = NullValueHandling.Ignore)]
		public bool DoNotLoadConfigOnNextBoot { get; set; }
	}
}