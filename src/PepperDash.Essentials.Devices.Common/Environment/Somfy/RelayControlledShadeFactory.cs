using System.Collections.Generic;
using PepperDash.Core;
using PepperDash.Essentials.Core;
using PepperDash.Essentials.Core.Config;

namespace PepperDash.Essentials.Devices.Common.Environment.Somfy
{
    public class RelayControlledShadeFactory : EssentialsDeviceFactory<RelayControlledShade>
    {
        public RelayControlledShadeFactory()
        {
            TypeNames = new List<string>() { "relaycontrolledshade" };
        }

        public override EssentialsDevice BuildDevice(DeviceConfig dc)
        {
            Debug.Console(1, "Factory Attempting to create new Generic Comm Device");
            var props = Newtonsoft.Json.JsonConvert.DeserializeObject<Environment.Somfy.RelayControlledShadeConfigProperties>(dc.Properties.ToString());

            return new Environment.Somfy.RelayControlledShade(dc.Key, dc.Name, props);
        }
    }
}