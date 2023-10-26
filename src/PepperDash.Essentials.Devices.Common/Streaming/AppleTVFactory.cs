using System.Collections.Generic;
using PepperDash.Core;
using PepperDash.Essentials.Core;
using PepperDash.Essentials.Core.Config;

namespace PepperDash.Essentials.Devices.Common
{
    public class AppleTVFactory : EssentialsDeviceFactory<AppleTV>
    {
        public AppleTVFactory()
        {
            TypeNames = new List<string>() { "appletv" };
        }

        public override EssentialsDevice BuildDevice(DeviceConfig dc)
        {
            Debug.Console(1, "Factory Attempting to create new AppleTV Device");
            var irCont = IRPortHelper.GetIrOutputPortController(dc);
            return new AppleTV(dc.Key, dc.Name, irCont);
        }
    }
}