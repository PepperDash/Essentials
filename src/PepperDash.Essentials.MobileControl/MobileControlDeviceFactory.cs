using PepperDash.Core;
using PepperDash.Core.Logging;
using PepperDash.Essentials.Core;
using PepperDash.Essentials.Core.Config;
using Serilog.Events;
using System;
using System.Collections.Generic;
using System.Linq;


namespace PepperDash.Essentials
{
    public class MobileControlDeviceFactory : EssentialsDeviceFactory<MobileControlSystemController>
    {
        public MobileControlDeviceFactory()
        {
            TypeNames = new List<string> { "appserver", "mobilecontrol", "webserver" };
        }

        public override EssentialsDevice BuildDevice(DeviceConfig dc)
        {
            try
            {
                var props = dc.Properties.ToObject<MobileControlConfig>();
                return new MobileControlSystemController(dc.Key, dc.Name, props);
            }
            catch (Exception e)
            {
                Debug.LogMessage(e, "Error building Mobile Control System Controller");
                return null;
            }
        }
    }
}