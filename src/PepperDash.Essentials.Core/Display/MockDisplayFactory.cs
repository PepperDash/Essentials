using System.Collections.Generic;
using PepperDash.Core;
using PepperDash.Essentials.Core.Config;

namespace PepperDash.Essentials.Core
{
    public class MockDisplayFactory : EssentialsDeviceFactory<MockDisplay>
    {
        public MockDisplayFactory()
        {
            TypeNames = new List<string>() { "mockdisplay" };
        }

        public override EssentialsDevice BuildDevice(DeviceConfig dc)
        {
            Debug.Console(1, "Factory Attempting to create new Mock Display Device");
            return new MockDisplay(dc.Key, dc.Name);
        }
    }
}