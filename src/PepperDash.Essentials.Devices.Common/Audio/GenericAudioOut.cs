using System.Collections.Generic;

using PepperDash.Core;
using PepperDash.Core.Logging;
using PepperDash.Essentials.Core;
using PepperDash.Essentials.Core.Config;
using PepperDash.Essentials.Core.Routing;
using Serilog.Events;

namespace PepperDash.Essentials.Devices.Common;

/// <summary>
/// Represents and audio endpoint
/// </summary>
public class GenericAudioOut : EssentialsDevice, IRoutingSink
{
	/// <inheritdoc/>
	public RoutingInputPort CurrentInputPort => AnyAudioIn;


	/// <inheritdoc/> 
	public Dictionary<eRoutingSignalType, IRoutingSource> CurrentSources { get; private set; }

	/// <inheritdoc/>
	public Dictionary<eRoutingSignalType, string> CurrentSourceKeys { get; private set; }

	/// <inheritdoc />
	public event System.EventHandler<CurrentSourcesChangedEventArgs> CurrentSourcesChanged;


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

		CurrentSources = new Dictionary<eRoutingSignalType, IRoutingSource>
			{
				{ eRoutingSignalType.Audio, null },
			};

		CurrentSourceKeys = new Dictionary<eRoutingSignalType, string>
			{
				{ eRoutingSignalType.Audio, string.Empty },
			};
	}

		/// <inheritdoc />
	public virtual void SetCurrentSource(eRoutingSignalType signalType, IRoutingSource sourceDevice)
	{
		foreach (eRoutingSignalType type in System.Enum.GetValues(typeof(eRoutingSignalType)))
		{
			var flagValue = System.Convert.ToInt32(type);
			// Skip if flagValue is 0 or not a power of two (i.e., not a single-bit flag).
			// (flagValue & (flagValue - 1)) != 0 checks if more than one bit is set.
			if (flagValue == 0 || (flagValue & (flagValue - 1)) != 0)
			{
				this.LogDebug("Skipping {type}", type);
				continue;
			}

			this.LogDebug("setting {type}", type);

			var previousSource = CurrentSources[type];

			if (signalType.HasFlag(type))
			{
				UpdateCurrentSources(type, previousSource, sourceDevice);
			}
		}
	}

	private void UpdateCurrentSources(eRoutingSignalType signalType, IRoutingSource previousSource, IRoutingSource sourceDevice)
	{
		if (CurrentSources.ContainsKey(signalType))
		{
			CurrentSources[signalType] = sourceDevice;
		}
		else
		{
			CurrentSources.Add(signalType, sourceDevice);
		}

		// Update the current source key for the specified signal type
		if (CurrentSourceKeys.ContainsKey(signalType))
		{
			CurrentSourceKeys[signalType] = sourceDevice.Key;
		}
		else
		{
			CurrentSourceKeys.Add(signalType, sourceDevice.Key);
		}

		// Raise the CurrentSourcesChanged event
		CurrentSourcesChanged?.Invoke(this, new CurrentSourcesChangedEventArgs(signalType, previousSource, sourceDevice));
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

	/// <inheritdoc />
	public override EssentialsDevice BuildDevice(DeviceConfig dc)
	{
		Debug.LogMessage(LogEventLevel.Debug, "Factory Attempting to create new GenericAudioOutWithVolumeFactory Device");
		var zone = dc.Properties.Value<uint>("zone");
		return new GenericAudioOutWithVolume(dc.Key, dc.Name,
			dc.Properties.Value<string>("volumeDeviceKey"), zone);
	}
}