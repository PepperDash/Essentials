using System.Collections.Generic;
using PepperDash.Core;
using PepperDash.Essentials.Core;
using PepperDash.Essentials.Core.Config;

namespace PepperDash.Essentials.Devices.Common.AudioCodec
{
    public class MockACFactory : EssentialsDeviceFactory<MockAC>
    {
        public MockACFactory()
        {
            TypeNames = new List<string>() { "mockac" };
        }

        public override EssentialsDevice BuildDevice(DeviceConfig dc)
        {
            Debug.Console(1, "Factory Attempting to create new MockAc Device");
            var props = Newtonsoft.Json.JsonConvert.DeserializeObject<AudioCodec.MockAcPropertiesConfig>(dc.Properties.ToString());
            return new AudioCodec.MockAC(dc.Key, dc.Name, props);
        }
    }
}