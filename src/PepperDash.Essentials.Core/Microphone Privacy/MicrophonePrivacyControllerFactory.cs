using System.Collections.Generic;
using PepperDash.Core;
using PepperDash.Essentials.Core.Config;

namespace PepperDash.Essentials.Core.Privacy
{
    public class MicrophonePrivacyControllerFactory : EssentialsDeviceFactory<MicrophonePrivacyController>
    {
        public MicrophonePrivacyControllerFactory()
        {
            TypeNames = new List<string>() { "microphoneprivacycontroller" };
        }

        public override EssentialsDevice BuildDevice(DeviceConfig dc)
        {
            Debug.Console(1, "Factory Attempting to create new MIcrophonePrivacyController Device");
            var props = Newtonsoft.Json.JsonConvert.DeserializeObject<Core.Privacy.MicrophonePrivacyControllerConfig>(dc.Properties.ToString());

            return new Core.Privacy.MicrophonePrivacyController(dc.Key, props);
        }
    }
}