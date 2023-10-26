using System.Collections.Generic;
using PepperDash.Core;
using PepperDash.Essentials.Core;
using PepperDash.Essentials.Core.Config;

namespace PepperDash.Essentials
{
    public class AmplifierFactory : EssentialsDeviceFactory<Amplifier>
    {
        public AmplifierFactory()
        {
            TypeNames = new List<string>() { "amplifier" };
        }

        public override EssentialsDevice BuildDevice(DeviceConfig dc)
        {
            Debug.Console(1, "Factory Attempting to create new Amplifier Device");
            return new Amplifier(dc.Key, dc.Name);
        }
    }
}