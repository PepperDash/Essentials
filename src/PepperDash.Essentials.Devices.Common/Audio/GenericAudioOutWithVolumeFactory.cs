using System.Collections.Generic;
using PepperDash.Core;
using PepperDash.Essentials.Core;
using PepperDash.Essentials.Core.Config;

namespace PepperDash.Essentials.Devices.Common
{
    public class GenericAudioOutWithVolumeFactory : EssentialsDeviceFactory<GenericAudioOutWithVolume>
    {
        public GenericAudioOutWithVolumeFactory()
        {
            TypeNames = new List<string>() { "genericaudiooutwithvolume" };
        }

        public override EssentialsDevice BuildDevice(DeviceConfig dc)
        {
            Debug.Console(1, "Factory Attempting to create new GenericAudioOutWithVolumeFactory Device");
            var zone = dc.Properties.Value<uint>("zone");
            return new GenericAudioOutWithVolume(dc.Key, dc.Name,
                dc.Properties.Value<string>("volumeDeviceKey"), zone);
        }
    }
}