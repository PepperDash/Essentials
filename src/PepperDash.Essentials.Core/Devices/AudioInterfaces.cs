namespace PepperDash.Essentials.Core.Devices
{

	public enum AudioChangeType
	{
		Mute, Volume
	}

	public class AudioChangeEventArgs
	{
		public AudioChangeType ChangeType { get; private set; }
		public IBasicVolumeControls AudioDevice { get; private set; }

		public AudioChangeEventArgs(IBasicVolumeControls device, AudioChangeType changeType)
		{
			ChangeType = changeType;
			AudioDevice = device;
		}
	}
}