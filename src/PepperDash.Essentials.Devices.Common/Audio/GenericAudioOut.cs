using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Crestron.SimplSharp;

using PepperDash.Core;
using PepperDash.Essentials.Core;
using PepperDash.Essentials.Core.Config;

using PepperDash.Essentials.Core.Routing;

namespace PepperDash.Essentials.Devices.Common
{
	/// <summary>
	/// Represents and audio endpoint
	/// </summary>
	public class GenericAudioOut : EssentialsDevice, IRoutingSinkNoSwitching
	{
        public event SourceInfoChangeHandler CurrentSourceChange;

        public string CurrentSourceInfoKey { get; set; }
        public SourceListItem CurrentSourceInfo
        {
            get
            {
                return _CurrentSourceInfo;
            }
            set
            {
                if (value == _CurrentSourceInfo) return;

                var handler = CurrentSourceChange;

                if (handler != null)
                    handler(_CurrentSourceInfo, ChangeType.WillChange);

                _CurrentSourceInfo = value;

                if (handler != null)
                    handler(_CurrentSourceInfo, ChangeType.DidChange);
            }
        }
        SourceListItem _CurrentSourceInfo;

		public RoutingInputPort AnyAudioIn { get; private set; }

		public GenericAudioOut(string key, string name)
			: base(key, name)
		{
			AnyAudioIn = new RoutingInputPort(RoutingPortNames.AnyAudioIn, eRoutingSignalType.Audio, 
				eRoutingPortConnectionType.LineAudio, null, this);
		}

		#region IRoutingInputs Members

		public RoutingPortCollection<RoutingInputPort> InputPorts
		{
			get { return new RoutingPortCollection<RoutingInputPort> { AnyAudioIn }; }
		}

		#endregion
	}


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
			VolumeZone = zone;
		}

	}

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