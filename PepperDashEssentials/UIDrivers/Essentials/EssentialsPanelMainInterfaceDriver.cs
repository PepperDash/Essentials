using System;
using Crestron.SimplSharp;
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
        CTimer InactivityTimer;

		/// <summary>
		/// Assign the appropriate A/V driver.
		/// Want to keep the AvDriver alive, because it may hold states
		/// </summary>
		public IAVDriver AvDriver { get; set; }

        public EssentialsHeaderDriver HeaderDriver { get; set; }

        public EssentialsEnvironmentDriver EnvironmentDriver { get; set; }

		public PanelDriverBase CurrentChildDriver { get; private set; }

        public ScreenSaverController ScreenSaverController { get; private set; } 

		CrestronTouchpanelPropertiesConfig Config;

        /// <summary>
        /// The main interlock for popups
        /// </summary>
        //public JoinedSigInterlock PopupInterlock { get; private set; }

		public EssentialsPanelMainInterfaceDriver(BasicTriListWithSmartObject trilist,
			CrestronTouchpanelPropertiesConfig config)
			: base(trilist)
		{
			Config = config;

            var tsx52or60 = trilist as Tswx52ButtonVoiceControl;

            if (tsx52or60 != null)
            {
                tsx52or60.ExtenderTouchDetectionReservedSigs.DeviceExtenderSigChange += ExtenderTouchDetectionReservedSigs_DeviceExtenderSigChange;
            }
            else
            {
                var tswx70 = trilist as TswX70Base;
                if (tswx70 != null)
                {
                    tswx70.ExtenderTouchDetectionReservedSigs.DeviceExtenderSigChange += ExtenderTouchDetectionReservedSigs_DeviceExtenderSigChange;
                }
            }
		}

        void ExtenderTouchDetectionReservedSigs_DeviceExtenderSigChange(Crestron.SimplSharpPro.DeviceExtender currentDeviceExtender, Crestron.SimplSharpPro.SigEventArgs args)
        {
            if (args.Sig.BoolValue)
            {
                if (InactivityTimer != null)
                {
                    InactivityTimer.Reset();
                }
                else
                {
                    InactivityTimer = new CTimer((o) => InactivityTimerExpired(), Config.ScreenSaverTimeoutMin * 60 * 1000);
                }
            }
        }

        void InactivityTimerExpired()
        {
            InactivityTimer.Stop();
            InactivityTimer.Dispose();
            InactivityTimer = null;

            ScreenSaverController.Show();
        }

		public override void Show()
		{
			CurrentChildDriver = null;
			ShowSubDriver(AvDriver as PanelDriverBase);
			base.Show();
		}

		public override void Hide()
		{
			TriList.BooleanInput[AvDriver.StartPageVisibleJoin].BoolValue = false;
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