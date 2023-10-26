extern alias Full;
using System.Collections.Generic;
using Full::Newtonsoft.Json.Linq;
using PepperDash.Core;
using PepperDash.Essentials.Core;
using PepperDash.Essentials.Core.Config;

namespace PepperDash.Essentials.Devices.Common
{
    public class IRBlurayBaseFactory : EssentialsDeviceFactory<IRBlurayBase>
    {
        public IRBlurayBaseFactory()
        {
            TypeNames = new List<string>() { "discplayer", "bluray" };
        }

        public override EssentialsDevice BuildDevice(DeviceConfig dc)
        {
            Debug.Console(1, "Factory Attempting to create new IRBlurayPlayer Device");

            if (dc.Properties["control"]["method"].Value<string>() == "ir")
            {
                var irCont = IRPortHelper.GetIrOutputPortController(dc);
                return new IRBlurayBase(dc.Key, dc.Name, irCont);
            }
            else if (dc.Properties["control"]["method"].Value<string>() == "com")
            {
                Debug.Console(0, "[{0}] COM Device type not implemented YET!", dc.Key);
            }

            return null;
        }
    }
}