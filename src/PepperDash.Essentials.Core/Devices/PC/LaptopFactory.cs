using System.Collections.Generic;
using PepperDash.Core;
using PepperDash.Essentials.Core.Config;

namespace PepperDash.Essentials.Core.Devices
{
    public class LaptopFactory : EssentialsDeviceFactory<Laptop>
    {
        public LaptopFactory()
        {
            TypeNames = new List<string>() { "laptop" };
        }

        public override EssentialsDevice BuildDevice(DeviceConfig dc)
        {
            Debug.Console(1, "Factory Attempting to create new Laptop Device");
            return new Core.Devices.Laptop(dc.Key, dc.Name);
        }
    }
}