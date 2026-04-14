
using Crestron.SimplSharp.WebScripting;
using System.Collections.Generic;
using System.Text;
using PepperDash.Essentials.Core.DeviceTypeInterfaces;
using PepperDash.Core.Web.RequestHandlers;
using System.Linq;

namespace PepperDash.Essentials.Core.Web.RequestHandlers;

/// <summary>
/// Request handler for retrieving initialization exceptions from the control system.
/// </summary>
public class GetInitializationExceptionsRequestHandler : WebApiBaseRequestHandler
{

    /// <summary>
    /// Initializes a new instance of the <see cref="GetInitializationExceptionsRequestHandler"/> class.
    /// </summary>
    public GetInitializationExceptionsRequestHandler() : base(true) { }

    /// <inheritdoc />
    protected override void HandleGet(HttpCwsContext context)
    {
        var exceptions = (Global.ControlSystem as IInitializationExceptions)?.InitializationExceptions ?? new List<System.Exception>();

        var response = Newtonsoft.Json.JsonConvert.SerializeObject(new
        {
            Exceptions = exceptions.Select(e => new
            {
                Message = e.Message,
                StackTrace = e.StackTrace,
                Type = e.GetType().FullName,
                InnerException = e.InnerException != null ? new
                {
                    Message = e.InnerException.Message,
                    StackTrace = e.InnerException.StackTrace,
                    Type = e.InnerException.GetType().FullName
                } : null
            }).ToList()
        });

        context.Response.StatusCode = 200;
        context.Response.StatusDescription = "OK";
        context.Response.ContentType = "application/json";
        context.Response.ContentEncoding = Encoding.UTF8;
        context.Response.Write(response, false);
        context.Response.End();
    }
}