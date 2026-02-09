using Crestron.SimplSharpPro;
using Crestron.SimplSharpPro.DeviceSupport;

using PepperDash.Essentials.Core;
using PepperDash.Essentials.Core.SmartObjects;

namespace PepperDash.Essentials.Core
{
	/// <summary>
	/// Defines the contract for IChannel
	/// </summary>
	public interface IChannel
	{
		/// <summary>
		/// Channel up
		/// </summary>
		/// <param name="pressRelease">indicates whether this is a press or release</param>
		/// 
		void ChannelUp(bool pressRelease);
		/// <summary>
		/// Channel down
		/// </summary>
		/// <param name="pressRelease">indicates whether this is a press or release</param>
		void ChannelDown(bool pressRelease);

		/// <summary>
		/// Last channel
		/// </summary>
		/// <param name="pressRelease">indicates whether this is a press or release</param>
		void LastChannel(bool pressRelease);

		/// <summary>
		/// Guide
		/// </summary>
		/// <param name="pressRelease">indicates whether this is a press or release</param>
		/// 
		void Guide(bool pressRelease);

		/// <summary>
		/// Info
		/// </summary>
		/// <param name="pressRelease">indicates whether this is a press or release</param>
		void Info(bool pressRelease);

		/// <summary>
		/// Exit
		/// </summary>
		/// <param name="pressRelease">indicates whether this is a press or release</param>
		void Exit(bool pressRelease);
	}

	/// <summary>
	/// IChannelExtensions class
	/// </summary>
	public static class IChannelExtensions
	{
		/// <summary>
		/// LinkButtons method
		/// </summary>
		public static void LinkButtons(this IChannel dev, BasicTriList triList)
		{
			triList.SetBoolSigAction(123, dev.ChannelUp);
			triList.SetBoolSigAction(124, dev.ChannelDown);
			triList.SetBoolSigAction(125, dev.LastChannel);
			triList.SetBoolSigAction(137, dev.Guide);
			triList.SetBoolSigAction(129, dev.Info);
			triList.SetBoolSigAction(134, dev.Exit);
		}

		/// <summary>
		/// UnlinkButtons method
		/// </summary>
		public static void UnlinkButtons(this IChannel dev, BasicTriList triList)
		{
			triList.ClearBoolSigAction(123);
			triList.ClearBoolSigAction(124);
			triList.ClearBoolSigAction(125);
			triList.ClearBoolSigAction(137);
			triList.ClearBoolSigAction(129);
			triList.ClearBoolSigAction(134);
		}
	}
}