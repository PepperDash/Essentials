using Crestron.SimplSharp;
using Serilog.Core;
using Serilog.Events;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PepperDash.Core.Logging
{
    public class CrestronEnricher : ILogEventEnricher
    {
        static readonly string _appName;

        static CrestronEnricher()
        {
            switch (CrestronEnvironment.DevicePlatform)
            {
                case eDevicePlatform.Appliance:
                    _appName = $"App {InitialParametersClass.ApplicationNumber}";
                    break;
                case eDevicePlatform.Server:
                    _appName = $"{InitialParametersClass.RoomId}";
                    break;
            }
        }
            

        public void Enrich(LogEvent logEvent, ILogEventPropertyFactory propertyFactory)
        {
            var property = propertyFactory.CreateProperty("App", _appName);

            logEvent.AddOrUpdateProperty(property);
        }
    }
}
