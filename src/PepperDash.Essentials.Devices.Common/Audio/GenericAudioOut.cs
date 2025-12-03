using System.Collections.Generic;

using PepperDash.Core;
using PepperDash.Essentials.Core;
using PepperDash.Essentials.Core.Config;

using Serilog.Events;

namespace PepperDash.Essentials.Devices.Common
{
	/// <summary>
	/// Represents and audio endpoint
	/// </summary>
	public class GenericAudioOut : EssentialsDevice, IRoutingSink
	{
		/// <summary>
		/// Gets the current input port
		/// </summary>
		public RoutingInputPort CurrentInputPort => AnyAudioIn;

		/// <summary>
		/// Event fired when the current source changes
		/// </summary>
		public event SourceInfoChangeHandler CurrentSourceChange;

		/// <summary>
		/// Gets or sets the current source info key
		/// </summary>
		public string CurrentSourceInfoKey { get; set; }
		/// <summary>
		/// Gets or sets the current source info
		/// </summary>
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

		/// <summary>
		/// Gets or sets the AnyAudioIn
		/// </summary>
		public RoutingInputPort AnyAudioIn { get; private set; }

		/// <summary>
		/// Constructor for GenericAudioOut
		/// </summary>
		/// <param name="key">Device key</param>
		/// <param name="name">Device name</param>
		public GenericAudioOut(string key, string name)
			: base(key, name)
		{
			AnyAudioIn = new RoutingInputPort(RoutingPortNames.AnyAudioIn, eRoutingSignalType.Audio,
				eRoutingPortConnectionType.LineAudio, null, this);
		}

		#region IRoutingInputs Members

		/// <summary>
		/// Gets the collection of input ports
		/// </summary>
		public RoutingPortCollection<RoutingInputPort> InputPorts
		{
			get { return new RoutingPortCollection<RoutingInputPort> { AnyAudioIn }; }
		}

		#endregion
	}


	/// <summary>
	/// Represents a GenericAudioOutWithVolume
	/// </summary>
	public class GenericAudioOutWithVolume : GenericAudioOut, IHasVolumeDevice
	{
		/// <summary>
		/// Gets the volume device key
		/// </summary>
		public string VolumeDeviceKey { get; private set; }
		/// <summary>
		/// Gets the volume zone
		/// </summary>
		public uint VolumeZone { get; private set; }

		/// <summary>
		/// Gets the volume device
		/// </summary>
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

	/// <summary>
	/// Factory for creating GenericAudioOutWithVolume devices
	/// </summary>
	public class GenericAudioOutWithVolumeFactory : EssentialsDeviceFactory<GenericAudioOutWithVolume>
	{
		/// <summary>
		/// Constructor for GenericAudioOutWithVolumeFactory
		/// </summary>
		public GenericAudioOutWithVolumeFactory()
		{
			TypeNames = new List<string>() { "genericaudiooutwithvolume" };
		}

		/// <summary>
		/// BuildDevice method
		/// </summary>
		/// <inheritdoc />
		public override EssentialsDevice BuildDevice(DeviceConfig dc)
		{
			Debug.LogMessage(LogEventLevel.Debug, "Factory Attempting to create new GenericAudioOutWithVolumeFactory Device");
			var zone = dc.Properties.Value<uint>("zone");
			return new GenericAudioOutWithVolume(dc.Key, dc.Name,
					dc.Properties.Value<string>("volumeDeviceKey"), zone);
		}
	}

}