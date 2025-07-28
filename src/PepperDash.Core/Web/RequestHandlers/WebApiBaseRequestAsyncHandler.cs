using Crestron.SimplSharp.WebScripting;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace PepperDash.Core.Web.RequestHandlers
{
    public abstract class WebApiBaseRequestAsyncHandler:IHttpCwsHandler
    {
        private readonly Dictionary<string, Func<HttpCwsContext, Task>> _handlers;
        protected readonly bool EnableCors;

        /// <summary>
        /// Constructor
        /// </summary>
        protected WebApiBaseRequestAsyncHandler(bool enableCors)
        {
            EnableCors = enableCors;

            _handlers = new Dictionary<string, Func<HttpCwsContext, Task>>
            {
                {"CONNECT", HandleConnect},
                {"DELETE", HandleDelete},
                {"GET", HandleGet},
                {"HEAD", HandleHead},
                {"OPTIONS", HandleOptions},
                {"PATCH", HandlePatch},
                {"POST", HandlePost},
                {"PUT", HandlePut},
                {"TRACE", HandleTrace}
            };
        }

        /// <summary>
        /// Constructor
        /// </summary>
        protected WebApiBaseRequestAsyncHandler()
            : this(false)
        {
        }

        /// <summary>
        /// Handles CONNECT method requests
        /// </summary>
        /// <param name="context"></param>
        protected virtual async Task HandleConnect(HttpCwsContext context)
        {
            context.Response.StatusCode = 501;
            context.Response.StatusDescription = "Not Implemented";
            context.Response.End();            
        }

        /// <summary>
        /// Handles DELETE method requests
        /// </summary>
        /// <param name="context"></param>
        protected virtual async Task HandleDelete(HttpCwsContext context)
        {
            context.Response.StatusCode = 501;
            context.Response.StatusDescription = "Not Implemented";
            context.Response.End();
        }

        /// <summary>
        /// Handles GET method requests
        /// </summary>
        /// <param name="context"></param>
        protected virtual async Task HandleGet(HttpCwsContext context)
        {
            context.Response.StatusCode = 501;
            context.Response.StatusDescription = "Not Implemented";
            context.Response.End();
        }

        /// <summary>
        /// Handles HEAD method requests
        /// </summary>
        /// <param name="context"></param>
        protected virtual async Task HandleHead(HttpCwsContext context)
        {
            context.Response.StatusCode = 501;
            context.Response.StatusDescription = "Not Implemented";
            context.Response.End();
        }

        /// <summary>
        /// Handles OPTIONS method requests
        /// </summary>
        /// <param name="context"></param>
        protected virtual async Task HandleOptions(HttpCwsContext context)
        {
            context.Response.StatusCode = 501;
            context.Response.StatusDescription = "Not Implemented";
            context.Response.End();
        }

        /// <summary>
        /// Handles PATCH method requests
        /// </summary>
        /// <param name="context"></param>
        protected virtual async Task HandlePatch(HttpCwsContext context)
        {
            context.Response.StatusCode = 501;
            context.Response.StatusDescription = "Not Implemented";
            context.Response.End();
        }

        /// <summary>
        /// Handles POST method requests
        /// </summary>
        /// <param name="context"></param>
        protected virtual async Task HandlePost(HttpCwsContext context)
        {
            context.Response.StatusCode = 501;
            context.Response.StatusDescription = "Not Implemented";
            context.Response.End();
        }

        /// <summary>
        /// Handles PUT method requests
        /// </summary>
        /// <param name="context"></param>
        protected virtual async Task HandlePut(HttpCwsContext context)
        {
            context.Response.StatusCode = 501;
            context.Response.StatusDescription = "Not Implemented";
            context.Response.End();
        }

        /// <summary>
        /// Handles TRACE method requests
        /// </summary>
        /// <param name="context"></param>
        protected virtual async Task HandleTrace(HttpCwsContext context)
        {
            context.Response.StatusCode = 501;
            context.Response.StatusDescription = "Not Implemented";
            context.Response.End();
        }

        /// <summary>
        /// Process request
        /// </summary>
        /// <param name="context"></param>
        /// <summary>
        /// ProcessRequest method
        /// </summary>
        public void ProcessRequest(HttpCwsContext context)
        {
            if (!_handlers.TryGetValue(context.Request.HttpMethod, out Func<HttpCwsContext, Task> handler))
            {
                return;
            }

            if (EnableCors)
            {
                context.Response.Headers.Add("Access-Control-Allow-Origin", "*");
                context.Response.Headers.Add("Access-Control-Allow-Methods", "POST, GET, OPTIONS");
            }

            var handlerTask = handler(context);

            handlerTask.GetAwaiter().GetResult();
        }
    }
}
