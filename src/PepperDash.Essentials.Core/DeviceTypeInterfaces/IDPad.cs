using Crestron.SimplSharpPro;
using Crestron.SimplSharpPro.DeviceSupport;
using PepperDash.Core;
using PepperDash.Essentials.Core;
using PepperDash.Essentials.Core.SmartObjects;

namespace PepperDash.Essentials.Core
{
	/// <summary>
	/// 
	/// </summary>
	public interface IDPad : IKeyed
	{
		/// <summary>
		/// Up button press
		/// </summary>
		/// <param name="pressRelease">determines if the button is pressed or released</param>
		void Up(bool pressRelease);

		/// <summary>
		/// Down button press
		/// </summary>
		/// <param name="pressRelease">determines if the button is pressed or released</param>
		void Down(bool pressRelease);

		/// <summary>
		/// Left button press
		/// </summary>
		/// <param name="pressRelease">determines if the button is pressed or released</param>
		void Left(bool pressRelease);

		/// <summary>
		/// Right button press
		/// </summary>
		/// <param name="pressRelease">determines if the button is pressed or released</param>
		void Right(bool pressRelease);

		/// <summary>
		/// Select button press
		/// </summary>
		/// <param name="pressRelease">determines if the button is pressed or released</param>
		void Select(bool pressRelease);

		/// <summary>
		/// Menu button press
		/// </summary>
		/// <param name="pressRelease">determines if the button is pressed or released</param>
		void Menu(bool pressRelease);

		/// <summary>
		/// Exit button press
		/// </summary>
		/// <param name="pressRelease">determines if the button is pressed or released</param>
		void Exit(bool pressRelease);
	}

	/// <summary>
	/// IDPadExtensions class
	/// </summary>
	public static class IDPadExtensions
	{
		/// <summary>
		/// LinkButtons method
		/// </summary>
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

		/// <summary>
		/// UnlinkButtons method
		/// </summary>
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