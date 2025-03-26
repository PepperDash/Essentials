using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Crestron.SimplSharp;
using Crestron.SimplSharp.Net.Http;
using Crestron.SimplSharp.Net.Https;

namespace PepperDash.Core.GenericRESTfulCommunications
{
	/// <summary>
	/// Generic RESTful communication class
	/// </summary>
	public class GenericRESTfulClient
	{
		/// <summary>
		/// Boolean event handler
		/// </summary>
		public event EventHandler<BoolChangeEventArgs> BoolChange;
		/// <summary>
		/// Ushort event handler
		/// </summary>
		public event EventHandler<UshrtChangeEventArgs> UshrtChange;
		/// <summary>
		/// String event handler
		/// </summary>
		public event EventHandler<StringChangeEventArgs> StringChange;

		/// <summary>
		/// Constructor
		/// </summary>
		public GenericRESTfulClient()
		{

		}

		/// <summary>
		/// Generic RESTful submit request
		/// </summary>
		/// <param name="url"></param>
		/// <param name="port"></param>
		/// <param name="requestType"></param>
		/// <param name="username"></param>
		/// <param name="password"></param>
        /// <param name="contentType"></param>
		public void SubmitRequest(string url, ushort port, ushort requestType, string contentType, string username, string password)
		{
			if (url.StartsWith("https:", StringComparison.OrdinalIgnoreCase))
			{
				SubmitRequestHttps(url, port, requestType, contentType, username, password);
			}
			else if (url.StartsWith("http:", StringComparison.OrdinalIgnoreCase))
			{
				SubmitRequestHttp(url, port, requestType, contentType, username, password);
			}
			else
			{
				OnStringChange(string.Format("Invalid URL {0}", url), 0, GenericRESTfulConstants.ErrorStringChange);
			}
		}

			/// <summary>
		/// Private HTTP submit request
		/// </summary>
		/// <param name="url"></param>
		/// <param name="port"></param>
		/// <param name="requestType"></param>
        /// <param name="contentType"></param>
		/// <param name="username"></param>
		/// <param name="password"></param>
		private void SubmitRequestHttp(string url, ushort port, ushort requestType, string contentType, string username, string password)
		{
			try
			{
				HttpClient client = new HttpClient();
				HttpClientRequest request = new HttpClientRequest();
				HttpClientResponse response;

				client.KeepAlive = false;
				
				if(port >= 1 || port <= 65535)
					client.Port = port;
				else
					client.Port = 80;

				var authorization = "";
				if (!string.IsNullOrEmpty(username))
					authorization = EncodeBase64(username, password);

				if (!string.IsNullOrEmpty(authorization))
					request.Header.SetHeaderValue("Authorization", authorization);

				if (!string.IsNullOrEmpty(contentType))
					request.Header.ContentType = contentType;

				request.Url.Parse(url);
				request.RequestType = (Crestron.SimplSharp.Net.Http.RequestType)requestType;
				
				response = client.Dispatch(request);

				CrestronConsole.PrintLine(string.Format("SubmitRequestHttp Response[{0}]: {1}", response.Code, response.ContentString.ToString()));				

				if (!string.IsNullOrEmpty(response.ContentString.ToString()))
					OnStringChange(response.ContentString.ToString(), 0, GenericRESTfulConstants.ResponseStringChange);

				if (response.Code > 0)
					OnUshrtChange((ushort)response.Code, 0, GenericRESTfulConstants.ResponseCodeChange);
			}
			catch (Exception e)
			{
				//var msg = string.Format("SubmitRequestHttp({0}, {1}, {2}) failed:{3}", url, port, requestType, e.Message);
				//CrestronConsole.PrintLine(msg);
				//ErrorLog.Error(msg);

				CrestronConsole.PrintLine(e.Message);
				OnStringChange(e.Message, 0, GenericRESTfulConstants.ErrorStringChange);				
			}
		}

		/// <summary>
		/// Private HTTPS submit request
		/// </summary>
		/// <param name="url"></param>
		/// <param name="port"></param>
		/// <param name="requestType"></param>
        /// <param name="contentType"></param>
		/// <param name="username"></param>
		/// <param name="password"></param>
		private void SubmitRequestHttps(string url, ushort port, ushort requestType, string contentType, string username, string password)
		{
			try
			{
				HttpsClient client = new HttpsClient();
				HttpsClientRequest request = new HttpsClientRequest();
				HttpsClientResponse response;

				client.KeepAlive = false;
				client.HostVerification = false;
				client.PeerVerification = false;				

				var authorization = "";
				if (!string.IsNullOrEmpty(username))
					authorization = EncodeBase64(username, password);

				if (!string.IsNullOrEmpty(authorization))
					request.Header.SetHeaderValue("Authorization", authorization);

				if (!string.IsNullOrEmpty(contentType))
					request.Header.ContentType = contentType;

				request.Url.Parse(url);
				request.RequestType = (Crestron.SimplSharp.Net.Https.RequestType)requestType;
				
				response = client.Dispatch(request);

				CrestronConsole.PrintLine(string.Format("SubmitRequestHttp Response[{0}]: {1}", response.Code, response.ContentString.ToString()));

				if(!string.IsNullOrEmpty(response.ContentString.ToString()))
					OnStringChange(response.ContentString.ToString(), 0, GenericRESTfulConstants.ResponseStringChange);

				if(response.Code > 0)
					OnUshrtChange((ushort)response.Code, 0, GenericRESTfulConstants.ResponseCodeChange);
				
			}
			catch (Exception e)
			{
				//var msg = string.Format("SubmitRequestHttps({0}, {1}, {2}, {3}, {4}) failed:{5}", url, port, requestType, username, password, e.Message);
				//CrestronConsole.PrintLine(msg);
				//ErrorLog.Error(msg);

				CrestronConsole.PrintLine(e.Message);
				OnStringChange(e.Message, 0, GenericRESTfulConstants.ErrorStringChange);
			}
		}

		/// <summary>
		/// Private method to encode username and password to Base64 string
		/// </summary>
		/// <param name="username"></param>
		/// <param name="password"></param>
		/// <returns>authorization</returns>
		private string EncodeBase64(string username, string password)
		{
			var authorization = "";

			try
			{
				if (!string.IsNullOrEmpty(username))
				{
					string base64String = System.Convert.ToBase64String(System.Text.Encoding.GetEncoding("ISO-8859-1").GetBytes(string.Format("{0}:{1}", username, password)));
					authorization = string.Format("Basic {0}", base64String);
				}
			}
			catch (Exception e)
			{
				var msg = string.Format("EncodeBase64({0}, {1}) failed:\r{2}", username, password, e);
				CrestronConsole.PrintLine(msg);
				ErrorLog.Error(msg);
				return "" ;
			}

			return authorization;
		}

		/// <summary>
		/// Protected method to handle boolean change events
		/// </summary>
		/// <param name="state"></param>
		/// <param name="index"></param>
		/// <param name="type"></param>
		protected void OnBoolChange(bool state, ushort index, ushort type)
		{
			var handler = BoolChange;
			if (handler != null)
			{
				var args = new BoolChangeEventArgs(state, type);
				args.Index = index;
				BoolChange(this, args);
			}
		}

		/// <summary>
		/// Protected mehtod to handle ushort change events
		/// </summary>
		/// <param name="value"></param>
		/// <param name="index"></param>
		/// <param name="type"></param>
		protected void OnUshrtChange(ushort value, ushort index, ushort type)
		{
			var handler = UshrtChange;
			if (handler != null)
			{
				var args = new UshrtChangeEventArgs(value, type);
				args.Index = index;
				UshrtChange(this, args);
			}
		}

		/// <summary>
		/// Protected method to handle string change events
		/// </summary>
		/// <param name="value"></param>
		/// <param name="index"></param>
		/// <param name="type"></param>
		protected void OnStringChange(string value, ushort index, ushort type)
		{
			var handler = StringChange;
			if (handler != null)
			{
				var args = new StringChangeEventArgs(value, type);
				args.Index = index;
				StringChange(this, args);
			}
		}
	}
}