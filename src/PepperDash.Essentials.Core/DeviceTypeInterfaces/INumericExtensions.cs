using Crestron.SimplSharpPro.DeviceSupport;
using PepperDash.Essentials.Core;
using PepperDash.Essentials.Core.SmartObjects;

namespace PepperDash.Essentials.Core
{
    /// <summary>
	/// 
	/// </summary>
	public static class INumericExtensions
	{
		/// <summary>
		/// Links to the smart object, and sets the misc button's labels on joins x and y
		/// </summary>
		public static void LinkButtons(this INumericKeypad dev, BasicTriList trilist)
		{
			trilist.SetBoolSigAction(110, dev.Digit0);
			trilist.SetBoolSigAction(111, dev.Digit1);
			trilist.SetBoolSigAction(112, dev.Digit2);
			trilist.SetBoolSigAction(113, dev.Digit3);
			trilist.SetBoolSigAction(114, dev.Digit4);
			trilist.SetBoolSigAction(115, dev.Digit5);
			trilist.SetBoolSigAction(116, dev.Digit6);
			trilist.SetBoolSigAction(117, dev.Digit7);
			trilist.SetBoolSigAction(118, dev.Digit8);
			trilist.SetBoolSigAction(119, dev.Digit9);
			trilist.SetBoolSigAction(120, dev.KeypadAccessoryButton1);
			trilist.SetBoolSigAction(121, dev.KeypadAccessoryButton2);
			trilist.StringInput[111].StringValue = dev.KeypadAccessoryButton1Label;
			trilist.StringInput[111].StringValue = dev.KeypadAccessoryButton2Label;
		}

		public static void UnlinkButtons(this INumericKeypad dev, BasicTriList trilist)
		{
			trilist.ClearBoolSigAction(110);
			trilist.ClearBoolSigAction(111);
			trilist.ClearBoolSigAction(112);
			trilist.ClearBoolSigAction(113);
			trilist.ClearBoolSigAction(114);
			trilist.ClearBoolSigAction(115);
			trilist.ClearBoolSigAction(116);
			trilist.ClearBoolSigAction(117);
			trilist.ClearBoolSigAction(118);
			trilist.ClearBoolSigAction(119);
			trilist.ClearBoolSigAction(120);
			trilist.ClearBoolSigAction(121);
		}
	}
}