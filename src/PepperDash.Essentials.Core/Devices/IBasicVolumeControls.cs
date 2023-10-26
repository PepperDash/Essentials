using System.Linq;
using System.Text;
using Crestron.SimplSharp;

namespace PepperDash.Essentials.Core
{
	/// <summary>
	/// Defines minimal volume and mute control methods
	/// </summary>
    public interface IBasicVolumeControls 
	{
		void VolumeUp(bool pressRelease);
		void VolumeDown(bool pressRelease);
		void MuteToggle();
	}
}