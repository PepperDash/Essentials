using Crestron.SimplSharpPro;
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
		void Up(bool pressRelease);
		void Down(bool pressRelease);
		void Left(bool pressRelease);
		void Right(bool pressRelease);
		void Select(bool pressRelease);
		void Menu(bool pressRelease);
		void Exit(bool pressRelease);
	}
}