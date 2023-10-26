using System.Collections.Generic;
using PepperDash.Core;
using PepperDash.Essentials.Core.Config;

namespace PepperDash.Essentials.Core
{
    public class GenericCommFactory : EssentialsDeviceFactory<GenericComm>
    {
        public GenericCommFactory()
        {
            TypeNames = new List<string>() { "genericComm" };
        }

        public override EssentialsDevice BuildDevice(DeviceConfig dc)
        {
            Debug.Console(1, "Factory Attempting to create new Generic Comm Device");
            return new GenericComm(dc);
        }
    }
}