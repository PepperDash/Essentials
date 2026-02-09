using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Crestron.SimplSharp;
using Crestron.SimplSharp.CrestronDataStore;

using PepperDash.Essentials.Core;

using PepperDash.Core;
using Serilog.Events;


namespace PepperDash.Essentials.License
{
	/// <summary>
	/// Abstract base class for License Managers
	/// </summary>
	public abstract class LicenseManager
	{
		/// <summary>
		/// Gets or sets the LicenseIsValid
		/// </summary>
		public BoolFeedback LicenseIsValid { get; protected set; }

		/// <summary>
		/// Gets or sets the LicenseMessage
		/// </summary>
		public StringFeedback LicenseMessage { get; protected set; }

		/// <summary>
		/// Gets or sets the LicenseLog
		/// </summary>
		public StringFeedback LicenseLog { get; protected set; }

		/// <summary>
		/// Constructor
		/// </summary>
		protected LicenseManager()
		{
			CrestronConsole.AddNewConsoleCommand(
				s => CrestronConsole.ConsoleCommandResponse(GetStatusString()), 
				"licensestatus", "shows license and related data", 
				ConsoleAccessLevelEnum.AccessOperator);
		}

		/// <summary>
		/// Gets the status string for console command
		/// </summary>
		protected abstract string GetStatusString();
	}

	/// <summary>
	/// Represents a MockEssentialsLicenseManager
	/// </summary>
	public class MockEssentialsLicenseManager : LicenseManager
	{
		/// <summary>
		/// Returns the singleton mock license manager for this app
		/// </summary>
		public static MockEssentialsLicenseManager Manager
		{
			get
			{
				if (_Manager == null)
					_Manager = new MockEssentialsLicenseManager();
				return _Manager;
			}
		}
		static MockEssentialsLicenseManager _Manager;

		bool IsValid;

		MockEssentialsLicenseManager() : base()
		{
			LicenseIsValid = new BoolFeedback("LicenseIsValid",
				() => { return IsValid; });
			CrestronConsole.AddNewConsoleCommand(
				s => SetFromConsole(s.Equals("true", StringComparison.OrdinalIgnoreCase)), 
				"mocklicense", "true or false for testing", ConsoleAccessLevelEnum.AccessOperator);

			bool valid;
			var err = CrestronDataStoreStatic.GetGlobalBoolValue("MockLicense", out valid);
			if (err == CrestronDataStore.CDS_ERROR.CDS_SUCCESS)
				SetIsValid(valid);
			else if (err == CrestronDataStore.CDS_ERROR.CDS_RECORD_NOT_FOUND)
				CrestronDataStoreStatic.SetGlobalBoolValue("MockLicense", false);
			else
				CrestronConsole.PrintLine("Error restoring Mock License setting: {0}", err);
		}

		void SetIsValid(bool isValid)
		{
			IsValid = isValid;
			CrestronDataStoreStatic.SetGlobalBoolValue("MockLicense", isValid);
			Debug.LogMessage(LogEventLevel.Information, "Mock License is{0} valid", IsValid ? "" : " not");
			LicenseIsValid.FireUpdate();
		}

		void SetFromConsole(bool isValid)
		{
			SetIsValid(isValid);
		}

		/// <summary>
		/// Gets the status string for console command
		/// </summary>
		/// <returns>license status valid or invalid</returns>
		protected override string GetStatusString()
		{
			return string.Format("License Status: {0}", IsValid ? "Valid" : "Not Valid");
		}
	}
}