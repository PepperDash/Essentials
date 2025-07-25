using Crestron.SimplSharpPro.DeviceSupport;
using PepperDash.Core;
using PepperDash.Essentials.Core;
using PepperDash.Essentials.Core.SmartObjects;

namespace PepperDash.Essentials.Core
{
 /// <summary>
 /// Defines the contract for INumericKeypad
 /// </summary>
	public interface INumericKeypad:IKeyed
	{
		void Digit0(bool pressRelease);
		void Digit1(bool pressRelease);
		void Digit2(bool pressRelease);
		void Digit3(bool pressRelease);
		void Digit4(bool pressRelease);
		void Digit5(bool pressRelease);
		void Digit6(bool pressRelease);
		void Digit7(bool pressRelease);
		void Digit8(bool pressRelease);
		void Digit9(bool pressRelease);

		/// <summary>
		/// Used to hide/show the button and/or text on the left-hand keypad button
		/// </summary>
		bool HasKeypadAccessoryButton1 { get; }
		string KeypadAccessoryButton1Label { get; }
		void KeypadAccessoryButton1(bool pressRelease);

		bool HasKeypadAccessoryButton2 { get; }
		string KeypadAccessoryButton2Label { get; }
		void KeypadAccessoryButton2(bool pressRelease);
	}

	public interface ISetTopBoxNumericKeypad : INumericKeypad
	{
		void Dash(bool pressRelease);
		void KeypadEnter(bool pressRelease);
	}

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

  /// <summary>
  /// UnlinkButtons method
  /// </summary>
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