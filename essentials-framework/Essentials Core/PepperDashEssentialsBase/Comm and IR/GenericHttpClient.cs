using System;
using System.Collections.Generic;
using System.ComponentModel;
using Crestron.SimplSharp.Net.Http;
using Crestron.SimplSharp.Ssh;
using Crestron.SimplSharpPro.EthernetCommunication;
using PepperDash.Core;

namespace PepperDash.Essentials.Core
{
	public class GenericHttpClient : Device, IBasicCommunication
	{
		public HttpClient Client;
		public event EventHandler<GenericHttpClientEventArgs> ResponseRecived;

		public GenericHttpClient(string key, string name, string hostname)
			: base(key, name)
		{
			Client = new HttpClient {HostName = hostname};
		}

	    public GenericHttpClient(string key, string name, string hostname, GenericHttpClientConnectionOptions options)
	        : base(key, name)
	    {
	        Client = new HttpClient
	        {
	            HostName = hostname,
	            Accept = options.Accept,
	            KeepAlive = options.KeepAlive,
	            Password = options.Password,
	            Timeout = options.Timeout,
	            TimeoutEnabled = options.TimeoutEnabled,
	            UserAgent = options.UserAgent,
	            UserName = options.UserName,
	            Version = options.Version
	        };
	        if (options.Port > 0) Client.Port = options.Port;
	    }

	    /// <summary>
		/// Send a HTTP Get Request to a client
		/// </summary>
		/// <param name="path">Path to request node</param>
        public void SendText(string path)
        {
            var url = string.Format("http://{0}/{1}", Client.HostName, path);
            var request = new HttpClientRequest()
            {
                Url = new UrlParser(url)
            };
            var error = Client.DispatchAsyncEx(request, Response, request);
        }

        /// <summary>
        /// Send a HTTP Get Request to a client using a formatted string
        /// </summary>
        /// <param name="format">Path</param>
        /// <param name="items">Parameters for Path String Formatting</param>
        public void SendText(string format, params object[] items)
        {
            var url = string.Format("http://{0}/{1}", Client.HostName, string.Format(format, items));
            var request = new HttpClientRequest()
            {
                Url = new UrlParser(url)
            };
            var error = Client.DispatchAsyncEx(request, Response, request);
        }

        /// <summary>
        /// Send a unidirectional HTTP Get Request to a client using a formatted string
        /// </summary>
        /// <param name="format">Path</param>
        /// <param name="items">Parameters for Path String Formatting</param>
        public void SendTextNoResponse(string format, params object[] items)
        {
            var url = string.Format("http://{0}/{1}", Client.HostName, string.Format(format, items));
            var request = new HttpClientRequest()
            {
                Url = new UrlParser(url)
            };
            Client.Dispatch(request);
        }

        /// <summary>
        /// Send an HTTP Request of a specific request type
        /// </summary>
        /// <param name="requestType">HTTP Request Type</param>
        /// <param name="path">Path to request node</param>
        public void SendText(RequestType requestType, string path)
        {
            var url = string.Format("http://{0}/{1}", Client.HostName, path);
            var request = new HttpClientRequest()
            {
                Url = new UrlParser(url),
                RequestType = requestType
            };
            var error = Client.DispatchAsyncEx(request, Response, request);
        }

        /// <summary>
        /// Send an HTTP Request of a specific request type using a formatted string
        /// </summary>
        /// <param name="requestType">HTTP Request Type</param>
        /// <param name="format">Path</param>
        /// <param name="items">Parameters for Path String Formatting</param>
        public void SendText(RequestType requestType, string format, params object[] items)
        {
            var url = string.Format("http://{0}/{1}", Client.HostName, string.Format(format, items));
            var request = new HttpClientRequest()
            {
                Url = new UrlParser(url),
                RequestType = requestType
            };
            var error = Client.DispatchAsyncEx(request, Response, request);
        }

        /// <summary>
        /// Send a unidirectional HTTP Request of a specific request type using a formatted string
        /// </summary>
        /// <param name="requestType">HTTP Request Type</param>
        /// <param name="format">Path</param>
        /// <param name="items">Parameters for Path String Formatting</param>
        public void SendTextNoResponse(RequestType requestType, string format, params object[] items)
        {
            var url = string.Format("http://{0}/{1}", Client.HostName, string.Format(format, items));
            var request = new HttpClientRequest()
            {
                Url = new UrlParser(url)
            };
            Client.Dispatch(request);
        }

		private void Response(HttpClientResponse response, HTTP_CALLBACK_ERROR error, object request)
		{
		    if (error != HTTP_CALLBACK_ERROR.COMPLETED) return;
		    var responseReceived = response;

		    if (responseReceived.ContentString.Length <= 0) return;
		    if (ResponseRecived == null) return;
		    var httpClientRequest = request as HttpClientRequest;
		    if (httpClientRequest != null)
		        ResponseRecived(this, new GenericHttpClientEventArgs(responseReceived.ContentString, httpClientRequest.Url.ToString(), error));
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

    /// <summary>
    /// Objedct to set parameters for HTTP Requests
    /// </summary>
    public class GenericHttpClientConnectionOptions
    {
        /// <summary>
        ///     Gets or sets content types that are acceptable for the response. The default
        ///     value is "text/html, image/gif, image/jpeg, image/png, */*".
        /// </summary>
        [DefaultValue("text/html, image/gif, image/jpeg, image/png")]
        public string Accept { get; set; }

        /// <summary>
        ///     Controls whether to use HTTP Keep-Alive to keep the connection alive between
        ///     requests. If enabled (true) , once a request is made and a connection is
        ///     established, this connection is kept open and used for future requests. If
        ///     disabled, the connection is closed, and a new connection is created for future
        ///     requests.
        /// </summary>
        [DefaultValue(true)]
        public bool KeepAlive { get; set; }

        /// <summary>
        ///     This property controls whether the request operation will do an automatic
        ///     timeout checking. If timeout handling is turned on (i.e. this property is
        ///     set to true) and a request takes longer than Timeout, it will be terminated.
        /// </summary>
        [DefaultValue(true)]
        public bool TimeoutEnabled { get; set; }

        /// <summary>
        ///     Gets or sets the maximum amount of time (in seconds) that a client will wait
        ///     for a server response within a single request. The default value is 60 seconds
        ///     (1 minute).  The timeout handling can be activated via the TimeoutEnabled
        ///     property.
        /// </summary>
        [DefaultValue(60)]
        public int Timeout { get; set; }

        /// <summary>
        ///     Gets or sets the version identifier of the UserAgent. Can be used to mimic
        ///     particular browsers like Internet Explorer 6.0
        /// </summary>
        [DefaultValue("1.1")]
        public string Version { get; set; }

        /// <summary>
        ///     Gets or sets the identifier of the software being used to retrieve data via
        ///     the URL. Some custom HTTP servers check this HTTP header to provide content
        ///     optimized for particular HTTP browsers.
        /// </summary>
        [DefaultValue("Crestron SimplSharp HTTP Client")]
        public string UserAgent { get; set; }

        /// <summary>
        ///    Name that will be inserted into the Authorization HTTP header in the request
        ///    to the server.
        /// </summary>
        public string UserName { get; set; }

        /// <summary>
        ///     Password that will be inserted into the Authorization HTTP header in the
        ///     request to the server.
        /// </summary>
        public string Password { get; set; }

        /// <summary>
        ///     The server Port that you intend the client to connect to.  If you do not
        ///     assign a port number on this property, the port number in the parsed URL
        ///     will be used. If a port number is assigned in the parsed URL, it will take
        ///     precedence over this property.
        /// </summary>
        /// <remarks>
        ///     If you do not assign a port number on this property, the port number in the
        ///     parsed URL will be used.
        /// </remarks>
        /// 
        public int Port { get; set; }
    }
}