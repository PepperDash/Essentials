using Crestron.SimplSharpPro;
using Crestron.SimplSharpPro.DeviceSupport;

using PepperDash.Essentials.Core;
using PepperDash.Essentials.Core.SmartObjects;

namespace PepperDash.Essentials.Core;

	/// <summary>
	/// 
	/// </summary>
	public interface IColor
	{
		void Red(bool pressRelease);
		void Green(bool pressRelease);
		void Yellow(bool pressRelease);
		void Blue(bool pressRelease);
	}

	/// <summary>
	/// 
	/// </summary>
	public static class IColorExtensions
	{
		public static void LinkButtons(this IColor dev, BasicTriList TriList)
		{
			TriList.SetBoolSigAction(155, dev.Red);
			TriList.SetBoolSigAction(156, dev.Green);
			TriList.SetBoolSigAction(157, dev.Yellow);
			TriList.SetBoolSigAction(158, dev.Blue);
		}

		public static void UnlinkButtons(this IColor dev, BasicTriList triList)
		{
			triList.ClearBoolSigAction(155);
			triList.ClearBoolSigAction(156);
			triList.ClearBoolSigAction(157);
			triList.ClearBoolSigAction(158);
		}
	}