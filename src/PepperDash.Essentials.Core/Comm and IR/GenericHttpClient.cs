using Crestron.SimplSharp.Net.Http;
using PepperDash.Core;
using System;

namespace PepperDash.Essentials.Core
{
	
	/// <summary>
	/// Represents a GenericHttpClient
	/// </summary>
    [Obsolete("Please use the builtin HttpClient class instead: https://learn.microsoft.com/en-us/dotnet/fundamentals/networking/http/httpclient-guidelines")]
	public class GenericHttpClient : Device, IBasicCommunication
	{
		private readonly HttpClient Client;

		/// <summary>
		/// Event raised when response is received
		/// </summary>
		public event EventHandler<GenericHttpClientEventArgs> ResponseRecived;

		/// <summary>
		/// Constructor
		/// </summary>
		/// <param name="key">key of the device</param>
		/// <param name="name">name of the device</param>
		/// <param name="hostname">hostname for the HTTP client</param>
		public GenericHttpClient(string key, string name, string hostname)
			: base(key, name)
		{
            Client = new HttpClient
            {
                HostName = hostname
            };


        }

		/// <summary>
		/// SendText method
		/// </summary>
		/// <param name="path">the path to send the request to</param>
		public void SendText(string path)
		{
			HttpClientRequest request = new HttpClientRequest();
			string url = string.Format("http://{0}/{1}", Client.HostName, path);
			request.Url = new UrlParser(url);
			HttpClient.DISPATCHASYNC_ERROR error = Client.DispatchAsyncEx(request, Response, request); 
		}

		/// <summary>
		/// SendText method
		/// </summary>
		/// <param name="format">format for the items</param>
		/// <param name="items">items to format</param>
		public void SendText(string format, params object[] items)
		{
			HttpClientRequest request = new HttpClientRequest();
			string url = string.Format("http://{0}/{1}", Client.HostName, string.Format(format, items));
			request.Url = new UrlParser(url);
			HttpClient.DISPATCHASYNC_ERROR error = Client.DispatchAsyncEx(request, Response, request);
		}

		/// <summary>
		/// SendTextNoResponse method
		/// </summary>
		/// <param name="format">format for the items</param>
		/// <param name="items">items to format</param>
		public void SendTextNoResponse(string format, params object[] items)
		{
			HttpClientRequest request = new HttpClientRequest();
			string url = string.Format("http://{0}/{1}", Client.HostName, string.Format(format, items));
			request.Url = new UrlParser(url);
			Client.Dispatch(request);
		}

		/// <summary>
		/// Response method
		/// </summary>
		/// <param name="response">response received from the HTTP client</param>
		/// <param name="error">error status of the HTTP callback</param>
		/// <param name="request">original HTTP client request</param>
		private void Response(HttpClientResponse response, HTTP_CALLBACK_ERROR error, object request)
		{
			if (error == HTTP_CALLBACK_ERROR.COMPLETED)
			{
				var responseReceived = response;

				if (responseReceived.ContentString.Length > 0)
				{
                    ResponseRecived?.Invoke(this, new GenericHttpClientEventArgs(responseReceived.ContentString, (request as HttpClientRequest).Url.ToString(), error));
                }
			}

		}


		#region IBasicCommunication Members

		/// <summary>
		/// SendBytes method
		/// </summary>
		/// <param name="bytes">bytes to send</param>
		public void SendBytes(byte[] bytes)
		{
			throw new NotImplementedException();
		}



		#endregion

		#region ICommunicationReceiver Members

		/// <summary>
		/// BytesReceived event
		/// </summary>
		public event EventHandler<GenericCommMethodReceiveBytesArgs> BytesReceived;

		/// <summary>
		/// Connect method
		/// </summary>
		public void Connect()
		{
			throw new NotImplementedException();
		}

		/// <summary>
		/// Disconnect method
		/// </summary>
		public void Disconnect()
		{
			throw new NotImplementedException();
		}

		/// <summary>
		/// IsConnected property
		/// </summary>
		public bool IsConnected
		{
			get { return true; }
		}

		/// <summary>
		/// TextReceived event
		/// </summary>
		public event EventHandler<GenericCommMethodReceiveTextArgs> TextReceived;

		#endregion
	}

	/// <summary>
	/// Represents a GenericHttpClientEventArgs
	/// </summary>
	public class GenericHttpClientEventArgs : EventArgs
	{
		/// <summary>
		/// Gets or sets the ResponseText
		/// </summary>
		public string ResponseText { get; private set; }

		/// <summary>
		/// Gets or sets the RequestPath
		/// </summary>
		public string RequestPath { get; private set; }

		/// <summary>
		/// Gets or sets the Error
		/// </summary>
		public HTTP_CALLBACK_ERROR Error { get; set; }

		/// <summary>
		/// Constructor
		/// </summary>
		/// <param name="response">response text</param>
		/// <param name="request">request path</param>
		/// <param name="error">error status</param>
		public GenericHttpClientEventArgs(string response, string request, HTTP_CALLBACK_ERROR error)
		{
			ResponseText = response;
			RequestPath = request;
			Error = error;
		}
	}
}