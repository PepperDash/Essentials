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
		public PanelDriverBase AvDriver { get; set; }

        SingleSubpageModalDriver HelpDriver;

		public PanelDriverBase CurrentChildDriver { get; private set; }

		CrestronTouchpanelPropertiesConfig Config;

        SubpageReferenceList ActivityFooterList;

		public EssentialsPanelMainInterfaceDriver(BasicTriListWithSmartObject trilist,
			CrestronTouchpanelPropertiesConfig config)
			: base(trilist)
		{
			Config = config;
			trilist.SetSigFalseAction(UIBoolJoin.StartPagePress, () => ShowSubDriver(AvDriver));

			// Need this?
			trilist.SetSigFalseAction(UIBoolJoin.ShowPanelSetupPress, () =>
				ShowSubDriver(new SingleSubpageModalAndBackDriver(this, UIBoolJoin.PanelSetupVisible)));

            ActivityFooterList = new SubpageReferenceList(trilist, 1234, 3, 3, 3);
		}

		public override void Show()
		{
			CurrentChildDriver = null;


            //if (Config.UsesSplashPage)
            //    TriList.BooleanInput[UIBoolJoin.StartPageVisible].BoolValue = true;
            //else
			ShowSubDriver(AvDriver);

			TriList.SetSigFalseAction(UIBoolJoin.HelpPress, () =>
			{
                //var modal = new ModalDialog(TriList);
                //var room = DeviceManager.GetDeviceForKey(Config.DefaultRoomKey) 
                //    as EssentialsHuddleSpaceRoom;
                //string message = "Sorry, no help message available. No room connected.";
                //if(room != null)
                //    message = room.Config.HelpMessage;
                //modal.PresentModalTimerDialog(1, "Help", "Help", message,
                //    "Done", null, 0, false, null);

                string message = null;
                var room = DeviceManager.GetDeviceForKey(Config.DefaultRoomKey)
                    as EssentialsHuddleSpaceRoom;
                if (room != null)
                    message = room.Config.HelpMessage;
                else
                    message = "Sorry, no help message available. No room connected.";

                if (HelpDriver == null)
                    HelpDriver = new SingleSubpageModalDriver(this, UIBoolJoin.HelpPageVisible, UIBoolJoin.HelpClosePress);
                if (HelpDriver.IsVisible)
                    HelpDriver.Hide();
                else
                    HelpDriver.Show();

			});

            TriList.SetSigFalseAction(UIBoolJoin.RoomHeaderButtonPress, () =>
                {
                    
                });

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