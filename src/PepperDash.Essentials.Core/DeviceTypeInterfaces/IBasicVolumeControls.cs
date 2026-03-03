using PepperDash.Core;

namespace PepperDash.Essentials.Core
{
	/// <summary>
	/// Defines minimal volume and mute control methods
	/// </summary>
	public interface IBasicVolumeControls : IKeyName
	{
		/// <summary>
		/// Increases the volume
		/// </summary>
		/// <param name="pressRelease">Indicates whether the volume change is a press and hold action</param>
		void VolumeUp(bool pressRelease);

		/// <summary>
		/// Decreases the volume
		/// </summary>
		/// <param name="pressRelease">Indicates whether the volume change is a press and hold action</param>
		void VolumeDown(bool pressRelease);

		/// <summary>
		/// Toggles the mute state
		/// </summary>
		void MuteToggle();
	}
}
