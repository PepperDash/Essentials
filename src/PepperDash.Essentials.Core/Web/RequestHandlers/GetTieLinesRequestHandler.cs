using Crestron.SimplSharp.WebScripting;
using Newtonsoft.Json;
using PepperDash.Core.Web.RequestHandlers;
using System.Linq;
using System.Text;

namespace PepperDash.Essentials.Core.Web.RequestHandlers
{
    /// <summary>
    /// Represents a GetTieLinesRequestHandler
    /// </summary>
    public class GetTieLinesRequestHandler : WebApiBaseRequestHandler
    {
        /// <summary>
        /// Constructor
        /// </summary>
        public GetTieLinesRequestHandler() : base(true) { }

        /// <summary>
        /// Handles the GET request
        /// </summary>
        /// <param name="context"></param>
        protected override void HandleGet(HttpCwsContext context)
        {
            var tieLineString = JsonConvert.SerializeObject(TieLineCollection.Default.Select((tl) => new
            {
                sourceKey = tl.SourcePort.ParentDevice.Key,
                sourcePort = tl.SourcePort.Key,
                destinationKey = tl.DestinationPort.ParentDevice.Key,
                destinationPort = tl.DestinationPort.Key,
                type = tl.Type.ToString(),
            }));

            context.Response.StatusCode = 200;
            context.Response.StatusDescription = "OK";
            context.Response.ContentType = "application/json";
            context.Response.ContentEncoding = Encoding.UTF8;
            context.Response.Write(tieLineString, false);
            context.Response.End();

        }
    }
}
