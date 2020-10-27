using System;
using System.Collections.Generic;
using Crestron.SimplSharp;
using Crestron.SimplSharp.CrestronIO;
using Crestron.SimplSharp.Net.Http;

using PepperDash.Core;

namespace PepperDash.Essentials
{
    public class HttpLogoServer
    {
        /// <summary>
        /// 
        /// </summary>
        readonly HttpServer _server;

        /// <summary>
        /// 
        /// </summary>
        readonly string _fileDirectory;

        /// <summary>
        /// 
        /// </summary>
        public static Dictionary<string, string> ExtensionContentTypes;

        /// <summary>
        /// 
        /// </summary>
        /// <param name="port"></param>
        /// <param name="directory"></param>
        public HttpLogoServer(int port, string directory)
        {
            ExtensionContentTypes = new Dictionary<string, string>
			{
                //{ ".css", "text/css" },
                //{ ".htm", "text/html" },
                //{ ".html", "text/html" },
				{ ".jpg", "image/jpeg" },
				{ ".jpeg", "image/jpeg" },
                //{ ".js", "application/javascript" },
                //{ ".json", "application/json" },
                //{ ".map", "application/x-navimap" },
				{ ".pdf", "application/pdf" },
				{ ".png", "image/png" },
                //{ ".txt", "text/plain" },
			};

            _server = new HttpServer {Port = port};
            _fileDirectory = directory;
            _server.OnHttpRequest += Server_OnHttpRequest;
            _server.Open();

            CrestronEnvironment.ProgramStatusEventHandler += CrestronEnvironment_ProgramStatusEventHandler;
        }

        /// <summary>
        /// 
        /// </summary>
        void Server_OnHttpRequest(object sender, OnHttpRequestArgs args)
        {
            var path = args.Request.Path;
            Debug.Console(2, "HTTP Request with path: '{0}'", args.Request.Path);

            try
            {
                if (File.Exists(_fileDirectory + path))
                {
                    var filePath = path.Replace('/', '\\');
                    var localPath = string.Format(@"{0}{1}", _fileDirectory, filePath);

                    Debug.Console(2, "HTTP Logo Server attempting to find file: '{0}'", localPath);
                    if (File.Exists(localPath))
                    {
                        args.Response.Header.ContentType = GetContentType(new FileInfo(localPath).Extension);
                        args.Response.ContentStream = new FileStream(localPath, FileMode.Open, FileAccess.Read);
                    }
                    else
                    {
                        Debug.Console(2, "HTTP Logo Server Cannot find file '{0}'", localPath);
                        args.Response.ContentString = string.Format("Not found: '{0}'", filePath);
                        args.Response.Code = 404;
                    }
                }
                else
                {
                    Debug.Console(2, "HTTP Logo Server: '{0}' does not exist", _fileDirectory + path);
                    args.Response.ContentString = string.Format("Not found: '{0}'", _fileDirectory + path);
                    args.Response.Code = 404;
                }
            }
            catch (Exception ex)
            {
                Debug.Console(0, Debug.ErrorLogLevel.Error, "Exception getting file: {0}", ex.Message);
                Debug.Console(0, Debug.ErrorLogLevel.Error, "Stack Trace: {0}", ex.StackTrace);

                args.Response.Code = 400;
                args.Response.ContentString = string.Format("invalid request");
            }
        }

        /// <summary>
        /// 
        /// </summary>
        void CrestronEnvironment_ProgramStatusEventHandler(eProgramStatusEventType programEventType)
        {
            if (programEventType == eProgramStatusEventType.Stopping)
                _server.Close();
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="extension"></param>
        /// <returns></returns>
        public static string GetContentType(string extension)
        {
            var type = ExtensionContentTypes.ContainsKey(extension) ? ExtensionContentTypes[extension] : "text/plain";
            return type;
        }
    }
}