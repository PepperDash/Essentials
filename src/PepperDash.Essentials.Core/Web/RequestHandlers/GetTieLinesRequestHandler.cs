using Crestron.SimplSharp.WebScripting;
using Newtonsoft.Json;
using PepperDash.Core.Web.RequestHandlers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PepperDash.Essentials.Core.Web.RequestHandlers
{
    public class GetTieLinesRequestHandler:WebApiBaseRequestHandler
    {
        public GetTieLinesRequestHandler() : base(true) { }

        protected override void HandleGet(HttpCwsContext context)
        {
            var tieLineString = JsonConvert.SerializeObject(TieLineCollection.Default.Select((tl) => new { 
                sourceKey = tl.SourcePort.ParentDevice.Key,
                sourcePort = tl.SourcePort.Key,
                destinationKey = tl.DestinationPort.ParentDevice.Key,
                destinationPort = tl.DestinationPort.Key
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
