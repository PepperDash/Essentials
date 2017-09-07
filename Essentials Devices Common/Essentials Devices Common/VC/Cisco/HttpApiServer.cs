using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Crestron.SimplSharp;
using Crestron.SimplSharp.Net.Http;

using PepperDash.Core;


namespace PepperDash.Essentials.Devices.VideoCodec.Cisco
{
    public class HttpApiServer
    {
        public static Dictionary<string, string> ExtensionContentTypes;

		public event EventHandler<OnHttpRequestArgs> ApiRequest;
		public Crestron.SimplSharp.Net.Http.HttpServer HttpServer { get; private set; }

		public string HtmlRoot { get; set; }


		/// <summary>
		/// SIMPL+ can only execute the default constructor. If you have variables that require initialization, please
		/// use an Initialize method
		/// </summary>
        public HttpApiServer()
		{
			ExtensionContentTypes = new Dictionary<string, string>
			{
				{ ".css", "text/css" },
				{ ".htm", "text/html" },
				{ ".html", "text/html" },
				{ ".jpg", "image/jpeg" },
				{ ".jpeg", "image/jpeg" },
				{ ".js", "application/javascript" },
				{ ".json", "application/json" },
                { ".xml", "text/xml" },
				{ ".map", "application/x-navimap" },
				{ ".pdf", "application.pdf" },
				{ ".png", "image/png" },
				{ ".txt", "text/plain" },
			};
			HtmlRoot = @"\HTML";
		}


		public void Start(int port)
		{
			// TEMP - this should be inserted by configuring class

			HttpServer = new Crestron.SimplSharp.Net.Http.HttpServer();
			HttpServer.ServerName = "Cisco API Server";
			HttpServer.KeepAlive = true;
			HttpServer.Port = port;
			HttpServer.OnHttpRequest += Server_Request;
			HttpServer.Open();

			CrestronEnvironment.ProgramStatusEventHandler += (a) =>
			{
				if (a == eProgramStatusEventType.Stopping)
				{
					HttpServer.Close();
					Debug.Console(1, "Shutting down HTTP, port {0}", HttpServer.Port);
				}
			};
		}

		void Server_Request(object sender, OnHttpRequestArgs args)
		{
			if (args.Request.Header.RequestType == "OPTIONS")
			{
				Debug.Console(2, "Asking for OPTIONS");
				args.Response.Header.SetHeaderValue("Access-Control-Allow-Origin", "*");
				args.Response.Header.SetHeaderValue("Access-Control-Allow-Methods", "GET, POST, PATCH, PUT, DELETE, OPTIONS");
				return;
			}

			string path = Uri.UnescapeDataString(args.Request.Path);
			var host = args.Request.DataConnection.RemoteEndPointAddress;
			string authToken;

			Debug.Console(2, "HTTP Request: {2}: Path='{0}' ?'{1}'", path, args.Request.QueryString, host);

			// ----------------------------------- ADD AUTH HERE
			if (path.StartsWith("/cisco/api"))
			{
				var handler = ApiRequest;
				if (ApiRequest != null)
					ApiRequest(this, args);
			}
			// Basic file handling
			else
			{
				// Default path
                //if (path.EndsWith("/")) path = path + "index.html";
                //if (string.IsNullOrEmpty(Path.GetExtension(path))) path = path + "/index.html";

                //string filePath = path.Replace('/', '\\');
                //string localPath = string.Format(@"{0}{1}", HtmlRoot, filePath);
                //if (File.Exists(localPath))
                //{
                //    args.Response.Header.ContentType = GetContentType(new FileInfo(localPath).Extension);
                //    args.Response.ContentStream = new FileStream(localPath, FileMode.Open, FileAccess.Read);
                //    //args.Response.CloseStream = true;
                //}
                //else
                //{
                //    args.Response.ContentString = string.Format("Not found: '{0}'", filePath);
                //    args.Response.Code = 404;
                //}
			}
		}

		public static string GetContentType(string extension)
		{
			string type;
			if (ExtensionContentTypes.ContainsKey(extension))
				type = ExtensionContentTypes[extension];
			else
				type = "text/plain";
			return type;
		}

	}

}