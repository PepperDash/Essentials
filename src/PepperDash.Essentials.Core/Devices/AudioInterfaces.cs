namespace PepperDash.Essentials.Core.Devices
{

 /// <summary>
 /// Enumeration of AudioChangeType values
 /// </summary>
	public enum AudioChangeType
	{
		Mute, Volume
	}

 /// <summary>
 /// Represents a AudioChangeEventArgs
 /// </summary>
	public class AudioChangeEventArgs
	{
  /// <summary>
  /// Gets or sets the ChangeType
  /// </summary>
		public AudioChangeType ChangeType { get; private set; }
  /// <summary>
  /// Gets or sets the AudioDevice
  /// </summary>
		public IBasicVolumeControls AudioDevice { get; private set; }

		public AudioChangeEventArgs(IBasicVolumeControls device, AudioChangeType changeType)
		{
			ChangeType = changeType;
			AudioDevice = device;
		}
	}
}