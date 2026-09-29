using System;
using System.Text;
using Crestron.SimplSharp.WebScripting;
using Newtonsoft.Json;
using PepperDash.Core;
using PepperDash.Core.Web.RequestHandlers;
using PepperDash.Essentials.Core.Web.ConsoleSession;
using Serilog.Events;

namespace PepperDash.Essentials.Core.Web.RequestHandlers
{
    /// <summary>
    /// Starts and stops the console session server that streams the processor console to the browser
    /// </summary>
    public class ConsoleSessionRequestHandler : WebApiBaseRequestHandler
    {
        /// <summary>
        /// Constructor
        /// </summary>
        public ConsoleSessionRequestHandler()
            : base(true)
        {
        }

        /// <summary>
        /// Starts the console session server if needed and returns its WSS URL
        /// </summary>
        /// <param name="context"></param>
        protected override void HandleGet(HttpCwsContext context)
        {
            try
            {
                ConsoleSessionServer.Start();

                var res = JsonConvert.SerializeObject(new { url = ConsoleSessionServer.GetUrl(GetRequestHost(context)) });

                context.Response.ContentType = "application/json";
                context.Response.ContentEncoding = Encoding.UTF8;
                context.Response.StatusCode = 200;
                context.Response.StatusDescription = "OK";
                context.Response.Write(res, false);
                context.Response.End();
            }
            catch (Exception e)
            {
                Debug.LogMessage(LogEventLevel.Error, "Unable to start console session server: {message}", e.Message);

                context.Response.StatusCode = 500;
                context.Response.StatusDescription = "Internal Server Error";
                context.Response.End();
            }
        }

        /// <summary>
        /// Host the browser used to reach this API, without the port. Null when it can't be determined.
        /// </summary>
        /// <remarks>
        /// Prefers the Host header, which is what the browser actually sent. Request.Url is the fallback, since
        /// the processor's web server may rewrite it. Loopback values are ignored; they are not reachable by the browser.
        /// </remarks>
        private static string GetRequestHost(HttpCwsContext context)
        {
            var header = context.Request.Headers?["Host"];

            if (!string.IsNullOrEmpty(header))
            {
                // "name" or "name:port"; IPv6 literals are bracketed, so only strip a colon after any ']'
                var portSeparator = header.LastIndexOf(':');
                var host = portSeparator > header.LastIndexOf(']') ? header.Substring(0, portSeparator) : header;

                if (!IsLoopback(host)) return host;
            }

            var urlHost = context.Request.Url?.Host;

            return IsLoopback(urlHost) ? null : urlHost;
        }

        private static bool IsLoopback(string host)
        {
            return string.IsNullOrEmpty(host)
                || host.Equals("localhost", StringComparison.OrdinalIgnoreCase)
                || host.StartsWith("127.")
                || host == "[::1]";
        }

        /// <summary>
        /// Stops the console session server and every SSH session it holds
        /// </summary>
        /// <param name="context"></param>
        protected override void HandlePost(HttpCwsContext context)
        {
            ConsoleSessionServer.Stop();

            context.Response.StatusCode = 200;
            context.Response.StatusDescription = "OK";
            context.Response.End();
        }
    }
}
