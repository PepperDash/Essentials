using Crestron.SimplSharpPro;
using Crestron.SimplSharpPro.DeviceSupport;

using PepperDash.Essentials.Core;
using PepperDash.Essentials.Core.SmartObjects;

namespace PepperDash.Essentials.Core
{
	/// <summary>
	/// IColor interface
	/// </summary>
	public interface IColor
	{
		/// <summary>
		/// Red button
		/// </summary>
		/// <param name="pressRelease">indicates whether this is a press or release</param>
		void Red(bool pressRelease);

		/// <summary>
		/// Green button
		/// </summary>
		/// <param name="pressRelease">indicates whether this is a press or release</param>
		void Green(bool pressRelease);

		/// <summary>
		/// Yellow button
		/// </summary>
		/// <param name="pressRelease">indicates whether this is a press or release</param>
		void Yellow(bool pressRelease);

		/// <summary>
		/// Blue button
		/// </summary>
		/// <param name="pressRelease">indicates whether this is a press or release</param>
		void Blue(bool pressRelease);
	}

	/// <summary>
	/// IColorExtensions class
	/// </summary>
	public static class IColorExtensions
	{
		/// <summary>
		/// LinkButtons method
		/// </summary>
		/// <param name="dev">The IColor device</param>
		/// <param name="TriList">The BasicTriList to link</param>
		public static void LinkButtons(this IColor dev, BasicTriList TriList)
		{
			TriList.SetBoolSigAction(155, dev.Red);
			TriList.SetBoolSigAction(156, dev.Green);
			TriList.SetBoolSigAction(157, dev.Yellow);
			TriList.SetBoolSigAction(158, dev.Blue);
		}

		/// <summary>
		/// UnlinkButtons method
		/// </summary>
		/// <param name="dev">The IColor device</param>
		/// <param name="triList">The BasicTriList to unlink</param>
		public static void UnlinkButtons(this IColor dev, BasicTriList triList)
		{
			triList.ClearBoolSigAction(155);
			triList.ClearBoolSigAction(156);
			triList.ClearBoolSigAction(157);
			triList.ClearBoolSigAction(158);
		}
	}
}