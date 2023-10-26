using System.Collections.Generic;
using PepperDash.Core;
using PepperDash.Essentials.Core.Config;

namespace PepperDash.Essentials.Core
{
    public class ConsoleCommMockDeviceFactory : EssentialsDeviceFactory<ConsoleCommMockDevice>
    {
        public ConsoleCommMockDeviceFactory()
        {
            TypeNames = new List<string>() { "commmock" };
        }

        public override EssentialsDevice BuildDevice(DeviceConfig dc)
        {
            Debug.Console(1, "Factory Attempting to create new Comm Mock Device");
            var comm = CommFactory.CreateCommForDevice(dc);
            var props = Newtonsoft.Json.JsonConvert.DeserializeObject<ConsoleCommMockDevicePropertiesConfig>(
                dc.Properties.ToString());
            return new ConsoleCommMockDevice(dc.Key, dc.Name, props, comm);
        }
    }
}