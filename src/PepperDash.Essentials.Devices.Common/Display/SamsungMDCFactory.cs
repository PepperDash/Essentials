extern alias Full;
using System.Collections.Generic;
using Full::Newtonsoft.Json.Linq;
using PepperDash.Core;
using PepperDash.Essentials.Core;
using PepperDash.Essentials.Core.Config;

namespace PepperDash.Essentials.Devices.Displays
{
    public class SamsungMDCFactory : EssentialsDeviceFactory<SamsungMDC>
    {
        public SamsungMDCFactory()
        {
            TypeNames = new List<string>() { "samsungmdc" };
        }

        public override EssentialsDevice BuildDevice(DeviceConfig dc)
        {
            Debug.Console(1, "Factory Attempting to create new Generic Comm Device");
            var comm = CommFactory.CreateCommForDevice(dc);
            if (comm != null)
                return new SamsungMDC(dc.Key, dc.Name, comm, dc.Properties["id"].Value<string>());
            else
                return null;
        }
    }
}