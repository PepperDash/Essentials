using Crestron.SimplSharpPro;
using PepperDash.Essentials.Core;
using PepperDash.Essentials.Core.SmartObjects;

namespace PepperDash.Essentials.Core
{
	/// <summary>
	/// 
	/// </summary>
	public interface IChannel
	{
		void ChannelUp(bool pressRelease);
		void ChannelDown(bool pressRelease);
		void LastChannel(bool pressRelease);
		void Guide(bool pressRelease);
		void Info(bool pressRelease);
		void Exit(bool pressRelease);
	}
}