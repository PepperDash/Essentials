using System;
using Crestron.SimplSharpPro.DeviceSupport;
using Crestron.SimplSharpPro.UI;
using PepperDash.Essentials.Core;
using PepperDash.Essentials.Core.SmartObjects;

namespace PepperDash.Essentials
{
	/// <summary>
	/// 
	/// </summary>
	public class EssentialsPanelMainInterfaceDriver : PanelDriverBase
	{
		/// <summary>
		/// Assign the appropriate A/V driver.
		/// Want to keep the AvDriver alive, because it may hold states
		/// </summary>
		public IAVDriver AvDriver { get; set; }

        public EssentialsHeaderDriver HeaderDriver { get; set; }

        public EssentialsEnvironmentDriver EnvironmentDriver { get; set; }

		public PanelDriverBase CurrentChildDriver { get; private set; }

		CrestronTouchpanelPropertiesConfig Config;

        /// <summary>
        /// The main interlock for popups
        /// </summary>
        public JoinedSigInterlock PopupInterlock { get; private set; }

		public EssentialsPanelMainInterfaceDriver(BasicTriListWithSmartObject trilist,
			CrestronTouchpanelPropertiesConfig config)
			: base(trilist)
		{
			Config = config;
		}

		public override void Show()
		{
			CurrentChildDriver = null;
			ShowSubDriver(AvDriver as PanelDriverBase);
			base.Show();
		}

		public override void Hide()
		{
			TriList.BooleanInput[UIBoolJoin.StartPageVisible].BoolValue = false;
			base.Hide();
		}

		void ShowSubDriver(PanelDriverBase driver)
		{
			CurrentChildDriver = driver;
			if (driver == null)
				return;
			this.Hide();
			driver.Show();
		}

		/// <summary>
		/// 
		/// </summary>
		public override void BackButtonPressed()
		{
			if(CurrentChildDriver != null)
				CurrentChildDriver.BackButtonPressed();
		}
	}
}