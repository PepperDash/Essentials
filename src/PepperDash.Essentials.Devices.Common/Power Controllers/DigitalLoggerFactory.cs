extern alias Full;
using System.Collections.Generic;
using Full::Newtonsoft.Json;
using PepperDash.Core;
using PepperDash.Essentials.Core;
using PepperDash.Essentials.Core.Config;

namespace PepperDash.Essentials.Devices.Common
{
    public class DigitalLoggerFactory : EssentialsDeviceFactory<DigitalLogger>
    {
        public DigitalLoggerFactory()
        {
            TypeNames = new List<string>() { "digitallogger" };
        }

        public override EssentialsDevice BuildDevice(DeviceConfig dc)
        {
            Debug.Console(1, "Factory Attempting to create new DigitalLogger Device");
            var props = JsonConvert.DeserializeObject<DigitalLoggerPropertiesConfig>(
                dc.Properties.ToString());
            return new DigitalLogger(dc.Key, dc.Name, props);
        }
    }
}