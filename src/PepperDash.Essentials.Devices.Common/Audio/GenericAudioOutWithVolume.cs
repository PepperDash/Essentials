using PepperDash.Essentials.Core;

namespace PepperDash.Essentials.Devices.Common
{
    /// <summary>
    /// Allows a zone-device's audio control to be attached to the endpoint, for easy routing and
    /// control switching.  Will also set the zone name on attached devices, like SWAMP or other
    /// hardware with names built in.
    /// </summary>
    public class GenericAudioOutWithVolume : GenericAudioOut, IHasVolumeDevice
    {
        public string VolumeDeviceKey { get; private set; }
        public uint VolumeZone { get; private set; }

        public IBasicVolumeControls VolumeDevice
        { 
            get
            {
                var dev = DeviceManager.GetDeviceForKey(VolumeDeviceKey);
                if (dev is IAudioZones)
                    return (dev as IAudioZones).Zone[VolumeZone];
                else return dev as IBasicVolumeControls;
            } 
        }

        /// <summary>
        /// Constructor - adds the name to the attached audio device, if appropriate.
        /// </summary>
        /// <param name="key"></param>
        /// <param name="name"></param>
        /// <param name="audioDevice"></param>
        /// <param name="zone"></param>
        public GenericAudioOutWithVolume(string key, string name, string audioDevice, uint zone)
            : base(key, name)
        {
            VolumeDeviceKey = audioDevice;
            VolumeZone      = zone;
        }

    }
}