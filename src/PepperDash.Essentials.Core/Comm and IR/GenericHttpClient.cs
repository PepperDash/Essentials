using Crestron.SimplSharp.Net.Http;
using PepperDash.Core;
using System;

namespace PepperDash.Essentials.Core
{
    [Obsolete("Please use the builtin HttpClient class instead: https://learn.microsoft.com/en-us/dotnet/fundamentals/networking/http/httpclient-guidelines")]
 /// <summary>
 /// Represents a GenericHttpClient
 /// </summary>
	public class GenericHttpClient : Device, IBasicCommunication
	{
		private readonly HttpClient Client;
		public event EventHandler<GenericHttpClientEventArgs> ResponseRecived;

		public GenericHttpClient(string key, string name, string hostname)
			: base(key, name)
		{
            Client = new HttpClient
            {
                HostName = hostname
            };


        }

		/// <summary>
		/// 
		/// </summary>
		/// <param name="path"></param>
  /// <summary>
  /// SendText method
  /// </summary>
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

  /// <summary>
  /// SendTextNoResponse method
  /// </summary>
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
                    ResponseRecived?.Invoke(this, new GenericHttpClientEventArgs(responseReceived.ContentString, (request as HttpClientRequest).Url.ToString(), error));
                }
			}

		}


		#region IBasicCommunication Members

  /// <summary>
  /// SendBytes method
  /// </summary>
		public void SendBytes(byte[] bytes)
		{
			throw new NotImplementedException();
		}



		#endregion

		#region ICommunicationReceiver Members

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

		public bool IsConnected
		{
			get { return true; }
		}

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
		public GenericHttpClientEventArgs(string response, string request, HTTP_CALLBACK_ERROR error)
		{
			ResponseText = response;
			RequestPath = request;
			Error = error;
		}
	}
}