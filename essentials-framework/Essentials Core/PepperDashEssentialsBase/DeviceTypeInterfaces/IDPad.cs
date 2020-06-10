using Crestron.SimplSharpPro;
using Crestron.SimplSharpPro.DeviceSupport;

using PepperDash.Essentials.Core;
using PepperDash.Essentials.Core.SmartObjects;

namespace PepperDash.Essentials.Core
{
	/// <summary>
	/// 
	/// </summary>
	public interface IDPad
	{
		void Up(bool pressRelease);
		void Down(bool pressRelease);
		void Left(bool pressRelease);
		void Right(bool pressRelease);
		void Select(bool pressRelease);
		void Menu(bool pressRelease);
		void Exit(bool pressRelease);
	}

	/// <summary>
	/// 
	/// </summary>
	public static class IDPadExtensions
	{
		public static void LinkButtons(this IDPad dev, BasicTriList triList)
		{
			triList.SetBoolSigAction(138, dev.Up);
			triList.SetBoolSigAction(139, dev.Down);
			triList.SetBoolSigAction(140, dev.Left);
			triList.SetBoolSigAction(141, dev.Right);
			triList.SetBoolSigAction(142, dev.Select);
			triList.SetBoolSigAction(130, dev.Menu);
			triList.SetBoolSigAction(134, dev.Exit);        
		}

		public static void UnlinkButtons(this IDPad dev, BasicTriList triList)
		{
			triList.ClearBoolSigAction(138);
			triList.ClearBoolSigAction(139);
			triList.ClearBoolSigAction(140);
			triList.ClearBoolSigAction(141);
			triList.ClearBoolSigAction(142);
			triList.ClearBoolSigAction(130);
			triList.ClearBoolSigAction(134);
		}
	}
}