using Crestron.SimplSharp.Net.Http;
using PepperDash.Core;
using System;

namespace PepperDash.Essentials.Core
{
    [Obsolete("Please use the builtin HttpClient class instead: https://learn.microsoft.com/en-us/dotnet/fundamentals/networking/http/httpclient-guidelines")]
	public class GenericHttpClient : Device, IBasicCommunication
	{
		public HttpClient Client;
		public event EventHandler<GenericHttpClientEventArgs> ResponseRecived;

		public GenericHttpClient(string key, string name, string hostname)
			: base(key, name)
		{
			Client = new HttpClient();
			Client.HostName = hostname;
			
			
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="path"></param>
		public void SendText(string path)
		{
			HttpClientRequest request = new HttpClientRequest();
			string url = string.Format("http://{0}/{1}", Client.HostName, path);
			request.Url = new UrlParser(url);
			HttpClient.DISPATCHASYNC_ERROR error = Client.DispatchAsyncEx(request, Response, request); 
		}
		public void SendText(string format, params object[] items)
		{
			HttpClientRequest request = new HttpClientRequest();
			string url = string.Format("http://{0}/{1}", Client.HostName, string.Format(format, items));
			request.Url = new UrlParser(url);
			HttpClient.DISPATCHASYNC_ERROR error = Client.DispatchAsyncEx(request, Response, request);
		}

		public void SendTextNoResponse(string format, params object[] items)
		{
			HttpClientRequest request = new HttpClientRequest();
			string url = string.Format("http://{0}/{1}", Client.HostName, string.Format(format, items));
			request.Url = new UrlParser(url);
			Client.Dispatch(request);
		}

		private void Response(HttpClientResponse response, HTTP_CALLBACK_ERROR error, object request)
		{
			if (error == HTTP_CALLBACK_ERROR.COMPLETED)
			{
				var responseReceived = response;

				if (responseReceived.ContentString.Length > 0)
				{
					if (ResponseRecived != null)
						ResponseRecived(this, new GenericHttpClientEventArgs(responseReceived.ContentString, (request as HttpClientRequest).Url.ToString(), error));
				}
			}

		}


		#region IBasicCommunication Members

		public void SendBytes(byte[] bytes)
		{
			throw new NotImplementedException();
		}



		#endregion

		#region ICommunicationReceiver Members

		public event EventHandler<GenericCommMethodReceiveBytesArgs> BytesReceived;

		public void Connect()
		{
			throw new NotImplementedException();
		}

		public void Disconnect()
		{
			throw new NotImplementedException();
		}

		public bool IsConnected
		{
			get { return true; }
		}

		public event EventHandler<GenericCommMethodReceiveTextArgs> TextReceived;

		#endregion
	}
	public class GenericHttpClientEventArgs : EventArgs
	{
		public string ResponseText { get; private set; }
		public string RequestPath { get; private set; }
		public HTTP_CALLBACK_ERROR Error { get; set; }
		public GenericHttpClientEventArgs(string response, string request, HTTP_CALLBACK_ERROR error)
		{
			ResponseText = response;
			RequestPath = request;
			Error = error;
		}
	}
}