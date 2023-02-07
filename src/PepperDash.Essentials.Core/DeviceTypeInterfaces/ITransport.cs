using Crestron.SimplSharpPro.DeviceSupport;

namespace PepperDash.Essentials.Core
{
	/// <summary>
	/// 
	/// </summary>
	public interface ITransport
	{
		void Play(bool pressRelease);
		void Pause(bool pressRelease);
		void Rewind(bool pressRelease);
		void FFwd(bool pressRelease);
		void ChapMinus(bool pressRelease);
		void ChapPlus(bool pressRelease);
		void Stop(bool pressRelease);
		void Record(bool pressRelease);
	}

	/// <summary>
	/// 
	/// </summary>
	public static class ITransportExtensions
	{
		/// <summary>
		/// Attaches to trilist joins: Play:145, Pause:146, Stop:147, ChapPlus:148, ChapMinus:149, Rewind:150, Ffwd:151, Record:154
		/// 
		/// </summary>
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