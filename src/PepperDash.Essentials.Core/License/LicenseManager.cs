using System.Collections.Generic;
using System.Linq;
using System.Text;
using Crestron.SimplSharp;
using PepperDash.Essentials.Core;


namespace PepperDash.Essentials.License
{
	public abstract class LicenseManager
	{
		public BoolFeedback LicenseIsValid { get; protected set; }
		public StringFeedback LicenseMessage { get; protected set; }
		public StringFeedback LicenseLog { get; protected set; }

		protected LicenseManager()
		{
			CrestronConsole.AddNewConsoleCommand(
				s => CrestronConsole.ConsoleCommandResponse(GetStatusString()), 
				"licensestatus", "shows license and related data", 
				ConsoleAccessLevelEnum.AccessOperator);
		}

		protected abstract string GetStatusString();
	}
}