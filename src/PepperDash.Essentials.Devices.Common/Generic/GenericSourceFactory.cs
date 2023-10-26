using System.Collections.Generic;
using PepperDash.Core;
using PepperDash.Essentials.Core;
using PepperDash.Essentials.Core.Config;

namespace PepperDash.Essentials.Devices.Common
{
    public class GenericSourceFactory : EssentialsDeviceFactory<GenericSource>
    {
        public GenericSourceFactory()
        {
            TypeNames = new List<string>() { "genericsource" };
        }

        public override EssentialsDevice BuildDevice(DeviceConfig dc)
        {
            Debug.Console(1, "Factory Attempting to create new Generic Source Device");
            return new GenericSource(dc.Key, dc.Name);
        }
    }
}