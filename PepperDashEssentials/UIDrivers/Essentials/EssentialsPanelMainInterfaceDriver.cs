using System;
using Crestron.SimplSharp;
using Crestron.SimplSharpPro.DeviceSupport;
using Crestron.SimplSharpPro.UI;
using PepperDash.Core;
using PepperDash.Essentials.Core;
using PepperDash.Essentials.Core.SmartObjects;

namespace PepperDash.Essentials
{
	/// <summary>
	/// 
	/// </summary>
	public class EssentialsPanelMainInterfaceDriver : PanelDriverBase, IHasScreenSaverController, IDisposable
	{
        CTimer InactivityTimer;

		/// <summary>
		/// Assign the appropriate A/V driver.
		/// Want to keep the AvDriver alive, because it may hold states
		/// </summary>
		public IAVDriver AvDriver { get; set;}

        public EssentialsHeaderDriver HeaderDriver { get; set; }

        public EssentialsEnvironmentDriver EnvironmentDriver { get; set; }

		public PanelDriverBase CurrentChildDriver { get; private set; }

        public ScreenSaverController ScreenSaverController { get; set; }

	    private readonly long _timeoutMs;

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

            _timeoutMs = Config.ScreenSaverTimeoutMin * 60 * 1000;

            var tsx52or60 = trilist as Tswx52ButtonVoiceControl;

            if (tsx52or60 != null)
            {
                tsx52or60.ExtenderTouchDetectionReservedSigs.Use();
                tsx52or60.ExtenderTouchDetectionReservedSigs.DeviceExtenderSigChange += ExtenderTouchDetectionReservedSigs_DeviceExtenderSigChange;
                tsx52or60.ExtenderTouchDetectionReservedSigs.Time.UShortValue = 1;
                ManageInactivityTimer();

            }
            else
            {
                var tswx70 = trilist as TswX70Base;
                if (tswx70 != null)
                {
                    tswx70.ExtenderTouchDetectionReservedSigs.Use();
                    tswx70.ExtenderTouchDetectionReservedSigs.DeviceExtenderSigChange += ExtenderTouchDetectionReservedSigs_DeviceExtenderSigChange;
                    tswx70.ExtenderTouchDetectionReservedSigs.Time.UShortValue = 1;
                    ManageInactivityTimer();
                }
            }
		}

        #region IDisposable Members

        public void Dispose()
        {
            var avDriver = AvDriver as PanelDriverBase;
            if (avDriver != null)
            {
                avDriver.Hide();
            }
            if (ScreenSaverController != null)
            {
                ScreenSaverController.Dispose();
            }
            if (HeaderDriver != null)
            {
                HeaderDriver.Hide();
            }
            if (EnvironmentDriver != null)
            {
                EnvironmentDriver.Hide();
            }
            if (CurrentChildDriver != null)
            {
                CurrentChildDriver.Hide();
            }
        }

        #endregion

        void ExtenderTouchDetectionReservedSigs_DeviceExtenderSigChange(Crestron.SimplSharpPro.DeviceExtender currentDeviceExtender, Crestron.SimplSharpPro.SigEventArgs args)
        {

            if (args.Sig.BoolValue)
            {
                ManageInactivityTimer();
            }
        }

	    private void ManageInactivityTimer()
	    {
            if (InactivityTimer != null)
            {
                InactivityTimer.Reset(_timeoutMs);
            }
            else
            {
                InactivityTimer = new CTimer((o) => InactivityTimerExpired(), _timeoutMs);
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

    public interface IHasScreenSaverController
    {
        ScreenSaverController ScreenSaverController { get; }
    }
}