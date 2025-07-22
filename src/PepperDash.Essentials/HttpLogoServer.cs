using System;
using System.Collections.Generic;
using Crestron.SimplSharp;
using Crestron.SimplSharp.CrestronIO;
using Crestron.SimplSharp.Net.Http;

using PepperDash.Core;
using Serilog.Events;

namespace PepperDash.Essentials
{
    /// <summary>
    /// HTTP server for serving logo images and files
    /// </summary>
    public class HttpLogoServer
    {
        /// <summary>
        /// The HTTP server instance
        /// </summary>
        readonly HttpServer _server;

        /// <summary>
        /// The directory containing files to serve
        /// </summary>
        readonly string _fileDirectory;

        /// <summary>
        /// Dictionary mapping file extensions to content types
        /// </summary>
        public static Dictionary<string, string> ExtensionContentTypes;

        /// <summary>
        /// Initializes a new instance of the HttpLogoServer class
        /// </summary>
        /// <param name="port">Port number for the HTTP server</param>
        /// <param name="directory">Directory containing files to serve</param>
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
        /// Handles incoming HTTP requests and serves files from the configured directory
        /// </summary>
        /// <param name="sender">The HTTP server instance</param>
        /// <param name="args">HTTP request arguments</param>
        void Server_OnHttpRequest(object sender, OnHttpRequestArgs args)
        {
            var path = args.Request.Path;
            Debug.LogMessage(Serilog.Events.LogEventLevel.Verbose, "HTTP Request with path: '{requestPath:l}'", args.Request.Path);            

            try
            {
                if (File.Exists(_fileDirectory + path))
                {
                    var filePath = path.Replace('/', '\\');
                    var localPath = string.Format(@"{0}{1}", _fileDirectory, filePath);

                    Debug.LogMessage(LogEventLevel.Verbose, "HTTP Logo Server attempting to find file: '{localPath:l}'", localPath);
                    if (File.Exists(localPath))
                    {
                        args.Response.Header.ContentType = GetContentType(new FileInfo(localPath).Extension);
                        args.Response.ContentStream = new FileStream(localPath, FileMode.Open, FileAccess.Read);
                    }
                    else
                    {
                        Debug.LogMessage(LogEventLevel.Verbose, "HTTP Logo Server Cannot find file '{localPath:l}'", localPath);
                        args.Response.ContentString = string.Format("Not found: '{0}'", filePath);
                        args.Response.Code = 404;
                    }
                }
                else
                {
                    Debug.LogMessage(LogEventLevel.Verbose, "HTTP Logo Server: '{file:l}' does not exist", _fileDirectory + path);
                    args.Response.ContentString = string.Format("Not found: '{0}'", _fileDirectory + path);
                    args.Response.Code = 404;
                }
            }
            catch (Exception ex)
            {
                Debug.LogMessage(LogEventLevel.Error, "Exception getting file: {exception}", ex.Message, ex.StackTrace);
                Debug.LogMessage(LogEventLevel.Verbose, "Stack Trace: {stackTrace}", ex.StackTrace);

                args.Response.Code = 400;
                args.Response.ContentString = string.Format("invalid request");
            }
        }

        /// <summary>
        /// Handles program status events and closes the server when the program is stopping
        /// </summary>
        /// <param name="programEventType">The program status event type</param>
        void CrestronEnvironment_ProgramStatusEventHandler(eProgramStatusEventType programEventType)
        {
            if (programEventType == eProgramStatusEventType.Stopping)
                _server.Close();
        }

        /// <summary>
        /// Gets the content type for a file based on its extension
        /// </summary>
        /// <param name="extension">The file extension</param>
        /// <returns>The corresponding content type string</returns>
        public static string GetContentType(string extension)
        {
            var type = ExtensionContentTypes.ContainsKey(extension) ? ExtensionContentTypes[extension] : "text/plain";
            return type;
        }
    }
}