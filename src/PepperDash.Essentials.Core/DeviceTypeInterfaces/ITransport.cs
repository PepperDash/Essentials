using Crestron.SimplSharpPro.DeviceSupport;

namespace PepperDash.Essentials.Core
{
 /// <summary>
 /// Defines the contract for ITransport
 /// </summary>
	public interface ITransport
	{
		/// <summary>
		/// Play button action
		/// </summary>
		/// <param name="pressRelease">determines if the button action is a press or release</param>
		void Play(bool pressRelease);

		/// <summary>
		/// Pause button action
		/// </summary>
		/// <param name="pressRelease">determines if the button action is a press or release</param>
		void Pause(bool pressRelease);

		/// <summary>
		/// Rewind button action
		/// </summary>
		/// <param name="pressRelease">determines if the button action is a press or release</param>
		void Rewind(bool pressRelease);

		/// <summary>
		/// Fast Forward button action
		/// </summary>
		/// <param name="pressRelease">determines if the button action is a press or release</param>
		void FFwd(bool pressRelease);

		/// <summary>
		/// Chapter Minus button action
		/// </summary>
		/// <param name="pressRelease">determines if the button action is a press or release</param>
		void ChapMinus(bool pressRelease);

		/// <summary>
		/// Chapter Plus button action
		/// </summary>
		/// <param name="pressRelease">determines if the button action is a press or release</param>
		void ChapPlus(bool pressRelease);

		/// <summary>
		/// Stop button action
		/// </summary>
		/// <param name="pressRelease">determines if the button action is a press or release</param>
		void Stop(bool pressRelease);

		/// <summary>
		/// Record button action
		/// </summary>
		/// <param name="pressRelease">determines if the button action is a press or release</param>
		void Record(bool pressRelease);
	}

	/// <summary>
	/// ITransportExtensions class
	/// </summary>
	public static class ITransportExtensions
	{
		/// <summary>
		/// Attaches to trilist joins: Play:145, Pause:146, Stop:147, ChapPlus:148, ChapMinus:149, Rewind:150, Ffwd:151, Record:154
		/// </summary>
		/// <param name="dev">The ITransport device</param>
		/// <param name="triList">The BasicTriList to link buttons to</param>
		public static void LinkButtons(this ITransport dev, BasicTriList triList)
		{
			triList.SetBoolSigAction(145, dev.Play);
			triList.SetBoolSigAction(146, dev.Pause);
			triList.SetBoolSigAction(147, dev.Stop);
			triList.SetBoolSigAction(148, dev.ChapPlus);
			triList.SetBoolSigAction(149, dev.ChapMinus);
			triList.SetBoolSigAction(150, dev.Rewind);
			triList.SetBoolSigAction(151, dev.FFwd);
			triList.SetBoolSigAction(154, dev.Record);
		}

		/// <summary>
		/// UnlinkButtons method
		/// </summary>
		/// <param name="dev">The ITransport device</param>
		/// <param name="triList">The BasicTriList to unlink buttons from</param>
		public static void UnlinkButtons(this ITransport dev, BasicTriList triList)
		{
			triList.ClearBoolSigAction(145);
			triList.ClearBoolSigAction(146);
			triList.ClearBoolSigAction(147);
			triList.ClearBoolSigAction(148);
			triList.ClearBoolSigAction(149);
			triList.ClearBoolSigAction(150);
			triList.ClearBoolSigAction(151);
			triList.ClearBoolSigAction(154);
		}
	}
}