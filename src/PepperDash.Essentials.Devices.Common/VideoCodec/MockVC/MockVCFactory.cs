using System.Collections.Generic;
using PepperDash.Core;
using PepperDash.Essentials.Core;
using PepperDash.Essentials.Core.Config;

namespace PepperDash.Essentials.Devices.Common.VideoCodec
{
    public class MockVCFactory : EssentialsDeviceFactory<MockVC>
    {
        public MockVCFactory()
        {
            TypeNames = new List<string>() { "mockvc" };
        }

        public override EssentialsDevice BuildDevice(DeviceConfig dc)
        {
            Debug.Console(1, "Factory Attempting to create new MockVC Device");
            return new VideoCodec.MockVC(dc);
        }
    }
}