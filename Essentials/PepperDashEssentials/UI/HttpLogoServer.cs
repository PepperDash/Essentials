using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
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
        HttpServer Server;

        /// <summary>
        /// 
        /// </summary>
        string FileDirectory;

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
				{ ".pdf", "application.pdf" },
				{ ".png", "image/png" },
                //{ ".txt", "text/plain" },
			};

            Server = new HttpServer();
            Server.Port = port;
            FileDirectory = directory;
            Server.OnHttpRequest += new OnHttpRequestHandler(Server_OnHttpRequest);
            Server.Open();

            CrestronEnvironment.ProgramStatusEventHandler += new ProgramStatusEventHandler(CrestronEnvironment_ProgramStatusEventHandler);
        }

        /// <summary>
        /// 
        /// </summary>
        void Server_OnHttpRequest(object sender, OnHttpRequestArgs args)
        {
            var path = args.Request.Path;
            if (File.Exists(FileDirectory + @"\" + path))
            {
                string filePath = path.Replace('/', '\\');
                string localPath = string.Format(@"{0}{1}", FileDirectory, filePath);
                if (File.Exists(localPath))
                {
                    args.Response.Header.ContentType = GetContentType(new FileInfo(localPath).Extension);
                    args.Response.ContentStream = new FileStream(localPath, FileMode.Open, FileAccess.Read);
                }
                else
                {
                    args.Response.ContentString = string.Format("Not found: '{0}'", filePath);
                    args.Response.Code = 404;
                }
            }
        }

        /// <summary>
        /// 
        /// </summary>
        void CrestronEnvironment_ProgramStatusEventHandler(eProgramStatusEventType programEventType)
        {
            if (programEventType == eProgramStatusEventType.Stopping)
                Server.Close();
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="extension"></param>
        /// <returns></returns>
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