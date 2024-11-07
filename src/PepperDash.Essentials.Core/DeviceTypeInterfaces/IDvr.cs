using Crestron.SimplSharpPro.DeviceSupport;
using PepperDash.Essentials.Core.Touchpanels;

namespace PepperDash.Essentials.Core.DeviceTypeInterfaces
{
	/// <summary>
	/// 
	/// </summary>
	public interface IDvr : IDPad
	{
		void DvrList(bool pressRelease);
		void Record(bool pressRelease);
	}

	/// <summary>
	/// 
	/// </summary>
	public static class IDvrExtensions
	{
		public static void LinkButtons(this IDvr dev, BasicTriList triList)
		{
			triList.SetBoolSigAction(136, dev.DvrList);
			triList.SetBoolSigAction(152, dev.Record);
		}

		public static void UnlinkButtons(this IDvr dev, BasicTriList triList)
		{
			triList.ClearBoolSigAction(136);
			triList.ClearBoolSigAction(152);
		}
	}
}