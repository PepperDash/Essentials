using System;
using System.Collections.Generic;
using System.Linq;
using Crestron.SimplSharp;
using Crestron.SimplSharpPro;

using PepperDash.Core;
using PepperDash.Essentials.Core;
using PepperDash.Essentials.Core.Config;
using PepperDash.Essentials.Core.SmartObjects;
using PepperDash.Essentials.Core.PageManagers;

namespace PepperDash.Essentials
{
	/// <summary>
	/// 
	/// </summary>
    public class EssentialsPresentationPanelAvFunctionsDriver : PanelDriverBase
	{
        /// <summary>
        /// Smart Object 3200
        /// </summary>
        SubpageReferenceList SourcesSrl;

        /// <summary>
        /// For tracking feedback on last selected
        /// </summary>
        BoolInputSig LastSelectedSourceSig;
        
        /// <summary>
        ///  The source that has been selected and is awaiting assignment to a display
        /// </summary>
        SourceListItem PendingSource;

        bool IsSharingModeAdvanced;

		CrestronTouchpanelPropertiesConfig Config;

		public enum UiDisplayMode
		{
			PresentationMode, AudioSetup
		}

		/// <summary>
		/// Whether volume ramping from this panel will show the volume
		/// gauge popup.
		/// </summary>
		public bool ShowVolumeGauge { get; set; }

		/// <summary>
		/// The amount of time that the volume buttons stays on screen, in ms
		/// </summary>
		public uint VolumeButtonPopupTimeout
		{
			get { return VolumeButtonsPopupFeedback.TimeoutMs; }
			set { VolumeButtonsPopupFeedback.TimeoutMs = value; }
		}
		
		/// <summary>
		/// The amount of time that the volume gauge stays on screen, in ms
		/// </summary>
		public uint VolumeGaugePopupTimeout
		{
			get { return VolumeGaugeFeedback.TimeoutMs; }
			set { VolumeGaugeFeedback.TimeoutMs = value; }
		}

		/// <summary>
		/// 
		/// </summary>
		public uint PowerOffTimeout { get; set; }

		/// <summary>
		/// 
		/// </summary>
		public string DefaultRoomKey
		{
			get { return _DefaultRoomKey; }
			set
			{
				_DefaultRoomKey = value;
				CurrentRoom = DeviceManager.GetDeviceForKey(value) as EssentialsPresentationRoom;
			}	
		}
		string _DefaultRoomKey;

		/// <summary>
		/// 
		/// </summary>
		public EssentialsPresentationRoom CurrentRoom
		{
			get { return _CurrentRoom; }
			set
			{
				SetCurrentRoom(value);
			}
		}
        EssentialsPresentationRoom _CurrentRoom;

        /// <summary>
        /// For hitting feedback
        /// </summary>
        BoolInputSig ShareButtonSig;
        BoolInputSig EndMeetingButtonSig;

		/// <summary>
		/// Controls the extended period that the volume gauge shows on-screen,
		/// as triggered by Volume up/down operations
		/// </summary>
		BoolFeedbackPulseExtender VolumeGaugeFeedback;

		/// <summary>
		/// Controls the period that the volume buttons show on non-hard-button
		/// interfaces
		/// </summary>
		BoolFeedbackPulseExtender VolumeButtonsPopupFeedback;

        /// <summary>
        /// The parent driver for this
        /// </summary>
		PanelDriverBase Parent;

        ///// <summary>
        ///// Driver that manages advanced sharing features
        ///// </summary>
        //DualDisplaySimpleOrAdvancedRouting DualDisplayUiDriver;

        /// <summary>
        /// All children attached to this driver.  For hiding and showing as a group.
        /// </summary>
        List<PanelDriverBase> ChildDrivers = new List<PanelDriverBase>();

		List<BoolInputSig> CurrentDisplayModeSigsInUse = new List<BoolInputSig>();

        /// <summary>
        /// Smart Object 15022
        /// </summary>
        SubpageReferenceList ActivityFooterSrl;

		/// <summary>
		/// Tracks which audio page group the UI is in
		/// </summary>
		UiDisplayMode CurrentDisplayMode;

		/// <summary>
		/// The AV page mangagers that have been used, to keep them alive for later
		/// </summary>
		Dictionary<object, PageManager> PageManagers = new Dictionary<object, PageManager>();

		/// <summary>
		/// Current page manager running for a source
		/// </summary>
		PageManager CurrentSourcePageManager;

		/// <summary>
		/// Will auto-timeout a power off
		/// </summary>
		CTimer PowerOffTimer;

		/// <summary>
		/// Constructor
		/// </summary>
		public EssentialsPresentationPanelAvFunctionsDriver(PanelDriverBase parent, 
            CrestronTouchpanelPropertiesConfig config) 
			: base(parent.TriList)
		{
			Config = config;
			Parent = parent;

            ActivityFooterSrl = new SubpageReferenceList(TriList, 15022, 3, 3, 3);
            //SetupActivityFooterWhenRoomOff();

			ShowVolumeGauge = true;

			// One-second pulse extender for volume gauge
			VolumeGaugeFeedback = new BoolFeedbackPulseExtender(1500);
			VolumeGaugeFeedback.Feedback
				.LinkInputSig(TriList.BooleanInput[UIBoolJoin.VolumeGaugePopupVisible]);

			VolumeButtonsPopupFeedback = new BoolFeedbackPulseExtender(4000);
			VolumeButtonsPopupFeedback.Feedback
				.LinkInputSig(TriList.BooleanInput[UIBoolJoin.VolumeButtonPopupVisible]);

			PowerOffTimeout = 30000;

            SourcesSrl = new SubpageReferenceList(TriList, 3200, 3, 3, 3);

            TriList.StringInput[UIStringJoin.StartActivityText].StringValue =
                "Tap an activity to begin";

            // Sharing mode things
            TriList.SetSigFalseAction(UIBoolJoin.ToggleSharingModePress, ToggleSharingModePressed);

            TriList.SetSigFalseAction(UIBoolJoin.Display1AudioButtonPressAndFb, Display1AudioPress);
            TriList.SetSigFalseAction(UIBoolJoin.Display1ControlButtonPress, Display1ControlPress);
            TriList.SetSigTrueAction(UIBoolJoin.Display1SelectPressAndFb, Display1Press);

            TriList.SetSigFalseAction(UIBoolJoin.Display2AudioButtonPressAndFb, Display2AudioPress);
            TriList.SetSigFalseAction(UIBoolJoin.Display2ControlButtonPress, Display2ControlPress);
            TriList.SetSigTrueAction(UIBoolJoin.Display2SelectPressAndFb, Display2Press);
		}

		/// <summary>
		/// 
		/// </summary>
		public override void Show()
		{
			TriList.BooleanInput[UIBoolJoin.TopBarHabaneroVisible].BoolValue = true;
            TriList.BooleanInput[UIBoolJoin.ActivityFooterVisible].BoolValue = true;

			// Default to showing rooms/sources now.
			ShowMode(UiDisplayMode.PresentationMode);

			// Attach actions
			TriList.SetSigFalseAction(UIBoolJoin.VolumeButtonPopupPress, VolumeButtonsTogglePress);

            //Interlocked modals
            TriList.SetSigFalseAction(UIBoolJoin.InterlockedModalClosePress, HideCurrentInterlockedModal);
            TriList.SetSigFalseAction(UIBoolJoin.HelpPress, () =>
            {
                string message = null;
                var room = DeviceManager.GetDeviceForKey(Config.DefaultRoomKey)
                    as EssentialsPresentationRoom;
                if (room != null)
                    message = room.Config.HelpMessage;
                else
                    message = "Sorry, no help message available. No room connected.";
                TriList.StringInput[UIStringJoin.HelpMessage].StringValue = message;
                ShowInterlockedModal(UIBoolJoin.HelpPageVisible);
            });

            TriList.SetSigFalseAction(UIBoolJoin.RoomHeaderButtonPress, () =>
                ShowInterlockedModal(UIBoolJoin.RoomHeaderPageVisible));

#warning Add press and hold to gear button here
            TriList.SetSigFalseAction(UIBoolJoin.GearHeaderButtonPress, () =>
                ShowInterlockedModal(UIBoolJoin.VolumesPageVisible));

			// power-related functions
            // Note: some of these are not directly-related to the huddle space UI, but are held over
            // in case
			TriList.SetSigFalseAction(UIBoolJoin.ShowPowerOffPress, PowerButtonPressed);
			TriList.SetSigFalseAction(UIBoolJoin.PowerOffCancelPress, CancelPowerOff);
			TriList.SetSigFalseAction(UIBoolJoin.PowerOffConfirmPress, FinishPowerOff); 
			TriList.SetSigFalseAction(UIBoolJoin.PowerOffMorePress, () =>
				{
					CancelPowerOffTimer();
					TriList.BooleanInput[UIBoolJoin.PowerOffStep1Visible].BoolValue = false;
					TriList.BooleanInput[UIBoolJoin.PowerOffStep2Visible].BoolValue = true;
				});
			TriList.SetSigFalseAction(UIBoolJoin.AllRoomsOffPress, () =>
				{
					EssentialsHuddleSpaceRoom.AllRoomsOff();
					CancelPowerOff();
				});

            SetupActivityFooterWhenRoomOff();

			base.Show();
		}

        /// <summary>
        /// 
        /// </summary>
		public override void Hide()
		{
            var tl = TriList.BooleanInput;
            HideAndClearCurrentDisplayModeSigsInUse();
			tl[UIBoolJoin.TopBarHabaneroVisible].BoolValue = false;
            tl[UIBoolJoin.ActivityFooterVisible].BoolValue = false;
            tl[UIBoolJoin.StartPageVisible].BoolValue = false;
            tl[UIBoolJoin.TapToBeginVisible].BoolValue = false;
            tl[UIBoolJoin.ToggleSharingModeVisible].BoolValue = false;
            tl[UIBoolJoin.SourceStagingBarVisible].BoolValue = false;
            if (IsSharingModeAdvanced)
                tl[UIBoolJoin.DualDisplayPageVisible].BoolValue = false;
            else
                tl[UIBoolJoin.SelectASourceVisible].BoolValue = false;

			VolumeButtonsPopupFeedback.ClearNow();
			CancelPowerOff();

			base.Hide();
		}

        /// <summary>
        /// 
        /// </summary>
        void ShowCurrentSharingMode()
        {
            var tlb = TriList.BooleanInput;
            tlb[UIBoolJoin.ToggleSharingModeVisible].BoolValue = true;
            tlb[UIBoolJoin.SourceStagingBarVisible].BoolValue = true;
            if (IsSharingModeAdvanced)
            {
                tlb[UIBoolJoin.DualDisplayPageVisible].BoolValue = true;
                TriList.StringInput[UIStringJoin.Display1TitleLabel].StringValue =
                    (CurrentRoom.Displays[1] as IKeyName).Name;
                TriList.StringInput[UIStringJoin.Display2TitleLabel].StringValue =
                   (CurrentRoom.Displays[2] as IKeyName).Name;
            }
            else
                tlb[UIBoolJoin.SelectASourceVisible].BoolValue = true;
        }

        /// <summary>
        /// 
        /// </summary>
        void HideCurrentSharingMode()
        {
            var tl = TriList.BooleanInput;
            tl[UIBoolJoin.ToggleSharingModeVisible].BoolValue = false;
            tl[UIBoolJoin.SourceStagingBarVisible].BoolValue = false;
            tl[UIBoolJoin.DualDisplayPageVisible].BoolValue = false;
            tl[UIBoolJoin.SelectASourceVisible].BoolValue = false;
        }

        /// <summary>
        /// Shows the various "modes" that this driver controls.  Presentation, Setup page
        /// </summary>
        /// <param name="mode"></param>
		public void ShowMode(UiDisplayMode mode)
		{
			//Clear whatever is showing now.
			HideAndClearCurrentDisplayModeSigsInUse();
			CurrentDisplayMode = mode;
			switch (mode)
			{
				case UiDisplayMode.PresentationMode:
                    // show start page or staging...
                    if (CurrentRoom.OnFeedback.BoolValue)
                    {
                        //TriList.BooleanInput[UIBoolJoin.StagingPageVisible].BoolValue = true;
                        TriList.BooleanInput[UIBoolJoin.TapToBeginVisible].BoolValue = false;
                        TriList.BooleanInput[UIBoolJoin.SelectASourceVisible].BoolValue = false;
                    }
                    else
                    {
                        TriList.BooleanInput[UIBoolJoin.StartPageVisible].BoolValue = true;
                        TriList.BooleanInput[UIBoolJoin.TapToBeginVisible].BoolValue = true;
                        TriList.BooleanInput[UIBoolJoin.SelectASourceVisible].BoolValue = false;
                    }
                    // Date/time
					if (Config.ShowDate && Config.ShowTime)
					{
						TriList.BooleanInput[UIBoolJoin.DateAndTimeVisible].BoolValue = true;
						TriList.BooleanInput[UIBoolJoin.DateOnlyVisible].BoolValue = false;
						TriList.BooleanInput[UIBoolJoin.TimeOnlyVisible].BoolValue = false;
					}
					else
					{
						TriList.BooleanInput[UIBoolJoin.DateAndTimeVisible].BoolValue = false;
						TriList.BooleanInput[UIBoolJoin.DateOnlyVisible].BoolValue = Config.ShowDate;
						TriList.BooleanInput[UIBoolJoin.TimeOnlyVisible].BoolValue = Config.ShowTime;
					}

                    //TriList.SetSigFalseAction(UIBoolJoin.ToggleSharingModePress, ToggleSharingModePressed);

					ShowCurrentDisplayModeSigsInUse();
					break;
			}
		}

        /// <summary>
        /// 
        /// </summary>
        void SetupSourceList()
        {
            // get the source list config and set up the source list
            var config = ConfigReader.ConfigObject.SourceLists;
            if (config.ContainsKey(CurrentRoom.SourceListKey))
            {
                var srcList = config[CurrentRoom.SourceListKey]
                    .Values.ToList().OrderBy(s => s.Order);
                // Setup sources list			
                uint i = 1; // counter for UI list
                foreach (var srcConfig in srcList)
                {
                    if (!srcConfig.IncludeInSourceList) // Skip sources marked this way
                        continue;

                    var sourceKey = srcConfig.SourceKey;
                    var actualSource = DeviceManager.GetDeviceForKey(sourceKey) as Device;
                    if (actualSource == null)
                    {
                        Debug.Console(0, "Cannot assign missing source '{0}' to source UI list",
                            srcConfig.SourceKey);
                        continue;
                    }
                    var localSrcItem = srcConfig; // lambda scope below
                    var localIndex = i;
                    SourcesSrl.GetBoolFeedbackSig(i, 1).UserObject = new Action<bool>(b =>
                    {
                        if (b) return;
                        if (LastSelectedSourceSig != null)
                            LastSelectedSourceSig.BoolValue = false;
                        LastSelectedSourceSig = SourcesSrl.BoolInputSig(localIndex, 1);
                        LastSelectedSourceSig.BoolValue = true;
                        if (IsSharingModeAdvanced)
                        {
                            PendingSource = localSrcItem;
                        }
                        else
                        {
                            CurrentRoom.RouteSourceToAllDestinations(localSrcItem);
                        }
                    });
                    SourcesSrl.StringInputSig(i, 1).StringValue = srcConfig.PreferredName;
                    i++;
                }
                var count = (ushort)(i-1);
                SourcesSrl.Count = count;
                TriList.BooleanInput[UIBoolJoin.StagingPageAdditionalArrowsVisible].BoolValue =
                    count >= Config.SourcesOverflowCount;

                _CurrentRoom.CurrentDisplay1SourceChange += _CurrentRoom_CurrentDisplay1SourceChange;
                _CurrentRoom.CurrentDisplay2SourceChange += _CurrentRoom_CurrentDisplay2SourceChange;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        void ToggleSharingModePressed()
        {
            if (CurrentSourcePageManager != null)
                CurrentSourcePageManager.Hide();
            HideCurrentSharingMode();
            IsSharingModeAdvanced = !IsSharingModeAdvanced;
            TriList.BooleanInput[UIBoolJoin.ToggleSharingModePress].BoolValue = IsSharingModeAdvanced;
            ShowCurrentSharingMode();
        }

        ///// <summary>
        ///// 
        ///// </summary>
        //void EnableAppropriateDisplayButtons()
        //{
        //    if (LastSelectedSourceSig != null)
        //        LastSelectedSourceSig.BoolValue = false;
        //}

        public void Display1Press()
        {
            CurrentRoom.SourceToDisplay1(PendingSource);
        }

        public void Display1AudioPress()
        {

        }


        public void Display1ControlPress()
        {
            var uiDev = CurrentRoom.Display1SourceInfo.SourceDevice as IUiDisplayInfo;
            ShowSource(uiDev);
        }

        public void Display2Press()
        {
            CurrentRoom.SourceToDisplay2(PendingSource);
        }

        public void Display2AudioPress()
        {

        }

        public void Display2ControlPress()
        {
            var uiDev = CurrentRoom.Display2SourceInfo.SourceDevice as IUiDisplayInfo;
            ShowSource(uiDev);
        }

        /// <summary>
        /// When the room is off, set the footer SRL
        /// </summary>
        void SetupActivityFooterWhenRoomOff()
        {
            ActivityFooterSrl.Clear();
            ActivityFooterSrl.AddItem(new SubpageReferenceListActivityItem(1, ActivityFooterSrl, 0, 
                b => { if (!b) ShareButtonPressed(); }));
            // only show phone call when there's a dialer present
            ActivityFooterSrl.AddItem(new SubpageReferenceListActivityItem(2, ActivityFooterSrl, 1,
                b => { }));
            ActivityFooterSrl.Count = (ushort)(CurrentRoom.HasAudioDialer ? 2 : 1);
            TriList.UShortInput[UIUshortJoin.PresentationListCaretMode].UShortValue =
                (ushort)(CurrentRoom.HasAudioDialer ? 1 : 0);
        }

        /// <summary>
        /// Sets up the footer SRL for when the room is on
        /// </summary>
        void SetupActivityFooterWhenRoomOn()
        {
            ActivityFooterSrl.Clear();
            ActivityFooterSrl.AddItem(new SubpageReferenceListActivityItem(1, ActivityFooterSrl,
                0, null));
            if (CurrentRoom.HasAudioDialer)
            {
                ActivityFooterSrl.AddItem(new SubpageReferenceListActivityItem(2, ActivityFooterSrl,
                    1, b => { }));
                ActivityFooterSrl.AddItem(new SubpageReferenceListActivityItem(3, ActivityFooterSrl,
                    3, b => { if (!b) PowerButtonPressed(); }));
                ActivityFooterSrl.Count = 3;
                TriList.UShortInput[UIUshortJoin.PresentationListCaretMode].UShortValue = 2;
                EndMeetingButtonSig = ActivityFooterSrl.BoolInputSig(3, 1);
            }
            else
            {
                ActivityFooterSrl.AddItem(new SubpageReferenceListActivityItem(2, ActivityFooterSrl,
                    3, b => { if (!b) PowerButtonPressed(); }));
                ActivityFooterSrl.Count = 2;
                TriList.UShortInput[UIUshortJoin.PresentationListCaretMode].UShortValue = 1;
                EndMeetingButtonSig = ActivityFooterSrl.BoolInputSig(2, 1);
            }
        }

        /// <summary>
        /// Attached to activity list share button
        /// </summary>
        void ShareButtonPressed()
        {
            ShareButtonSig = ActivityFooterSrl.BoolInputSig(1, 1);
            if (!_CurrentRoom.OnFeedback.BoolValue)
            {
                ShareButtonSig.BoolValue = true;
                TriList.BooleanInput[UIBoolJoin.StartPageVisible].BoolValue = false;
                ShowCurrentSharingMode();
            }
        }

        uint CurrentInterlockedModalJoin;

        void ShowInterlockedModal(uint join)
        {
            if (CurrentInterlockedModalJoin == join)
                HideCurrentInterlockedModal();
            else
            {
                TriList.BooleanInput[UIBoolJoin.HelpPageVisible].BoolValue = join == UIBoolJoin.HelpPageVisible;
                TriList.BooleanInput[UIBoolJoin.RoomHeaderPageVisible].BoolValue = join == UIBoolJoin.RoomHeaderPageVisible;
                TriList.BooleanInput[UIBoolJoin.VolumesPageVisible].BoolValue = join == UIBoolJoin.VolumesPageVisible;
                CurrentInterlockedModalJoin = join;
            }
        }

        void HideCurrentInterlockedModal()
        {
            TriList.BooleanInput[CurrentInterlockedModalJoin].BoolValue = false;
            CurrentInterlockedModalJoin = 0;
        }

		/// <summary>
		/// Shows all sigs that are in CurrentDisplayModeSigsInUse
		/// </summary>
		void ShowCurrentDisplayModeSigsInUse()
		{
			foreach (var sig in CurrentDisplayModeSigsInUse)
				sig.BoolValue = true;
		}

		/// <summary>
		/// Hides all CurrentDisplayModeSigsInUse sigs and clears the array
		/// </summary>
		void HideAndClearCurrentDisplayModeSigsInUse()
		{
			foreach (var sig in CurrentDisplayModeSigsInUse)
				sig.BoolValue = false;
			CurrentDisplayModeSigsInUse.Clear();
		}

		/// <summary>
		/// Send the UI back depending on location, not used in huddle UI
		/// </summary>
		public override void BackButtonPressed()
		{
			switch (CurrentDisplayMode)
			{
				case UiDisplayMode.PresentationMode:
					//CancelReturnToSourceTimer();
					BackToHome();
					break;
			}
		}

		/// <summary>
		/// 
		/// </summary>
		void BackToHome()
		{
			Hide();
			Parent.Show();
		}

		/// <summary>
		/// Loads the appropriate Sigs into CurrentDisplayModeSigsInUse and shows them
		/// </summary>
		void ShowCurrentSource()
		{
			if (CurrentRoom.CurrentSingleSourceInfo == null)
				return;
			var uiDev = CurrentRoom.CurrentSingleSourceInfo.SourceDevice as IUiDisplayInfo;
			ShowSource(uiDev);
		}

        void ShowSource(IUiDisplayInfo uiDev)
        {
            PageManager pm = null;
			// If we need a page manager, get an appropriate one
			if (uiDev != null)
			{
                TriList.BooleanInput[UIBoolJoin.SelectASourceVisible].BoolValue = false;
                if (IsSharingModeAdvanced)
                {
                    TriList.BooleanInput[UIBoolJoin.SourceBackgroundOverlayVisible].BoolValue = true;
                    TriList.SetSigFalseAction(UIBoolJoin.SourceBackgroundOverlayClosePress, new Action(() =>
                    {
                        TriList.BooleanInput[UIBoolJoin.SourceBackgroundOverlayVisible].BoolValue = false;
                        if (CurrentSourcePageManager != null)
                            CurrentSourcePageManager.Hide();
                    }));
                }


				// Got an existing page manager, get it
				if (PageManagers.ContainsKey(uiDev))
					pm = PageManagers[uiDev];
				// Otherwise make an apporiate one
				else if (uiDev is ISetTopBoxControls)
					pm = new SetTopBoxThreePanelPageManager(uiDev as ISetTopBoxControls, TriList);
				else if (uiDev is IDiscPlayerControls)
					pm = new DiscPlayerMediumPageManager(uiDev as IDiscPlayerControls, TriList);
				else
					pm = new DefaultPageManager(uiDev, TriList);
				PageManagers[uiDev] = pm;
				CurrentSourcePageManager = pm;
				pm.Show();
			}
        }

		/// <summary>
		/// 
		/// </summary>
		public void PowerButtonPressed()
		{
			if (!CurrentRoom.OnFeedback.BoolValue) 
                return;
            EndMeetingButtonSig.BoolValue = true;
            ShareButtonSig.BoolValue = false;
            // Timeout or button 1 press will shut down
            var modal = new ModalDialog(TriList);
			uint time = 60000;
            uint seconds = time / 1000;
            var message = string.Format("Meeting will end in {0} seconds", seconds);
            modal.PresentModalDialog(2, "End Meeting", "Power", message,
                "End Meeting Now", "Cancel", true, true,
				but => 
                {
                    EndMeetingButtonSig.BoolValue = false;
                    if (but != 2)
                    {
                        CurrentRoom.RouteSourceToAllDestinations(null);
                    }
                    else
                        ShareButtonSig.BoolValue = true; // restore Share fb
                });
		}

		void CancelPowerOffTimer()
		{
			if (PowerOffTimer != null)
			{
				PowerOffTimer.Stop();
				PowerOffTimer = null;
			}
		}

		/// <summary>
		/// Runs the power off function on the current room
		/// </summary>
		public void FinishPowerOff()
		{
			if (CurrentRoom == null)
				return;
			CurrentRoom.RunRouteAction("roomOff");
			CancelPowerOff();
		}

		/// <summary>
		/// Hides power off pages and stops timer
		/// </summary>
		void CancelPowerOff()
		{
			CancelPowerOffTimer();
			TriList.BooleanInput[UIBoolJoin.PowerOffStep1Visible].BoolValue = false;
			TriList.BooleanInput[UIBoolJoin.PowerOffStep2Visible].BoolValue = false;
		}

		/// <summary>
		/// 
		/// </summary>
		void VolumeButtonsTogglePress()
		{
			if (VolumeButtonsPopupFeedback.BoolValue)
				VolumeButtonsPopupFeedback.ClearNow();
			else
			{
				// Trigger the popup
				VolumeButtonsPopupFeedback.BoolValue = true;
				VolumeButtonsPopupFeedback.BoolValue = false;
			}
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="state"></param>
		public void VolumeUpPress(bool state)
		{
			// extend timeouts
			if (ShowVolumeGauge)
				VolumeGaugeFeedback.BoolValue = state;		
			VolumeButtonsPopupFeedback.BoolValue = state;
			if (CurrentRoom.CurrentVolumeControls != null)
				CurrentRoom.CurrentVolumeControls.VolumeUp(state);
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="state"></param>
		public void VolumeDownPress(bool state)
		{
			// extend timeouts
			if (ShowVolumeGauge)
				VolumeGaugeFeedback.BoolValue = state;
			VolumeButtonsPopupFeedback.BoolValue = state; 
			if (CurrentRoom.CurrentVolumeControls != null)
				CurrentRoom.CurrentVolumeControls.VolumeDown(state);
		}

		/// <summary>
		/// Helper for property setter. Sets the panel to the given room, latching up all functionality
		/// </summary>
        void SetCurrentRoom(EssentialsPresentationRoom room)
		{
			if (_CurrentRoom == room) return;
			if (_CurrentRoom != null)
			{
				// Disconnect current room
                _CurrentRoom.OnFeedback.OutputChange -= _CurrentRoom_OnFeedback_OutputChange;
				_CurrentRoom.CurrentVolumeDeviceChange -= this._CurrentRoom_CurrentAudioDeviceChange;
				ClearAudioDeviceConnections();
				_CurrentRoom.CurrentSingleSourceChange -= this._CurrentRoom_SourceInfoChange;
				DisconnectSource(_CurrentRoom.CurrentSingleSourceInfo);
			}
			_CurrentRoom = room;

			if (_CurrentRoom != null)
			{
                if (IsSharingModeAdvanced)
                {} // add stuff here
                else
                    SetupSourceList();
				TriList.StringInput[UIStringJoin.CurrentRoomName].StringValue = _CurrentRoom.Name;

                // Link up all the change events from the room
                _CurrentRoom.OnFeedback.OutputChange += _CurrentRoom_OnFeedback_OutputChange;
				_CurrentRoom.CurrentVolumeDeviceChange += _CurrentRoom_CurrentAudioDeviceChange;
				RefreshAudioDeviceConnections();
				_CurrentRoom.CurrentSingleSourceChange += _CurrentRoom_SourceInfoChange;
				RefreshSourceInfo();
			}
			else
			{
				// Clear sigs that need to be
				TriList.StringInput[UIStringJoin.CurrentRoomName].StringValue = "Select a room";
			}
		}

		/// <summary>
		/// Hides source for provided source info
		/// </summary>
		/// <param name="previousInfo"></param>
		void DisconnectSource(SourceListItem previousInfo)
		{
			if (previousInfo == null) return;

			// Hide whatever is showing
			if (IsVisible)
			{
				if (CurrentSourcePageManager != null)
				{
					CurrentSourcePageManager.Hide();
					CurrentSourcePageManager = null;
				}
			}

			if (previousInfo == null) return;
			var previousDev = previousInfo.SourceDevice;

			// device type interfaces
			if (previousDev is ISetTopBoxControls)
				(previousDev as ISetTopBoxControls).UnlinkButtons(TriList);
			// common interfaces
			if (previousDev is IChannel)
				(previousDev as IChannel).UnlinkButtons(TriList);
			if (previousDev is IColor)
				(previousDev as IColor).UnlinkButtons(TriList);
			if (previousDev is IDPad)
				(previousDev as IDPad).UnlinkButtons(TriList);
			if (previousDev is IDvr)
				(previousDev as IDvr).UnlinkButtons(TriList);
			if (previousDev is INumericKeypad)
				(previousDev as INumericKeypad).UnlinkButtons(TriList);
			if (previousDev is IPower)
				(previousDev as IPower).UnlinkButtons(TriList);
			if (previousDev is ITransport)
				(previousDev as ITransport).UnlinkButtons(TriList);
			//if (previousDev is IRadio)
			//    (previousDev as IRadio).UnlinkButtons(this);
		}

		/// <summary>
		/// Refreshes and shows the room's current source
		/// </summary>
		void RefreshSourceInfo()
		{
			var routeInfo = CurrentRoom.CurrentSingleSourceInfo;
			// This will show off popup too
			if (this.IsVisible)
				ShowCurrentSource();

			if (routeInfo == null)// || !CurrentRoom.OnFeedback.BoolValue)
			{
				// Check for power off and insert "Room is off"
				TriList.StringInput[UIStringJoin.CurrentSourceName].StringValue = "Room is off";
				TriList.StringInput[UIStringJoin.CurrentSourceIcon].StringValue = "Power";
				this.Hide();
				Parent.Show();
				return;
			}
			else if (CurrentRoom.CurrentSingleSourceInfo != null)
			{
				TriList.StringInput[UIStringJoin.CurrentSourceName].StringValue = routeInfo.PreferredName;
				TriList.StringInput[UIStringJoin.CurrentSourceIcon].StringValue = routeInfo.Icon; // defaults to "blank"
			}
			else
			{
				TriList.StringInput[UIStringJoin.CurrentSourceName].StringValue = "---";
				TriList.StringInput[UIStringJoin.CurrentSourceIcon].StringValue = "Blank";
			}
			
			// Connect controls
			if (routeInfo.SourceDevice != null)
				ConnectControlDeviceMethods(routeInfo.SourceDevice);
		}

		/// <summary>
		/// Attach the source to the buttons and things
		/// </summary>
		void ConnectControlDeviceMethods(Device dev)
		{
			if(dev is ISetTopBoxControls)
				(dev as ISetTopBoxControls).LinkButtons(TriList);
			if (dev is IChannel)
				(dev as IChannel).LinkButtons(TriList);
			if (dev is IColor)
				(dev as IColor).LinkButtons(TriList);
			if (dev is IDPad)
				(dev as IDPad).LinkButtons(TriList);
			if (dev is IDvr)
				(dev as IDvr).LinkButtons(TriList);
			if (dev is INumericKeypad)
				(dev as INumericKeypad).LinkButtons(TriList);
			if (dev is IPower)
				(dev as IPower).LinkButtons(TriList);
			if (dev is ITransport)
				(dev as ITransport).LinkButtons(TriList);
			//if (dev is IRadio)
			//    (dev as IRadio).LinkButtons(this); // +++++++++++++ Make part of this into page manager

			//if (dev is ICustomFunctions)
			//{
			//    var custBridge = (dev as ICustomFunctions).GetCustomBridge();
			//    custBridge.Link(this.Remote);
		}

		/// <summary>
		/// Detaches the buttons and feedback from the room's current audio device
		/// </summary>
		void ClearAudioDeviceConnections()
		{
			TriList.ClearBoolSigAction(UIBoolJoin.VolumeUpPress);
			TriList.ClearBoolSigAction(UIBoolJoin.VolumeDownPress);
            TriList.ClearBoolSigAction(UIBoolJoin.Volume1ProgramMutePressAndFB);

			var fDev = CurrentRoom.CurrentVolumeControls as IBasicVolumeWithFeedback;
			if (fDev != null)
			{
                TriList.ClearUShortSigAction(UIUshortJoin.VolumeSlider1Value);
				fDev.VolumeLevelFeedback.UnlinkInputSig(
                    TriList.UShortInput[UIUshortJoin.VolumeSlider1Value]);
			}
		}
	
		/// <summary>
		/// Attaches the buttons and feedback to the room's current audio device
		/// </summary>
		void RefreshAudioDeviceConnections()
		{
			var dev = CurrentRoom.CurrentVolumeControls;
			if (dev != null) // connect buttons
			{
				TriList.SetBoolSigAction(UIBoolJoin.VolumeUpPress, VolumeUpPress);
				TriList.SetBoolSigAction(UIBoolJoin.VolumeDownPress, VolumeDownPress);
                TriList.SetSigFalseAction(UIBoolJoin.Volume1ProgramMutePressAndFB, dev.MuteToggle);
			}

			var fbDev = dev as IBasicVolumeWithFeedback;
			if (fbDev == null) // this should catch both IBasicVolume and IBasicVolumeWithFeeback
                TriList.UShortInput[UIUshortJoin.VolumeSlider1Value].UShortValue = 0;
			else
			{
				// slider
                TriList.SetUShortSigAction(UIUshortJoin.VolumeSlider1Value, fbDev.SetVolume);
				// feedbacks
                fbDev.MuteFeedback.LinkInputSig(TriList.BooleanInput[UIBoolJoin.Volume1ProgramMutePressAndFB]);
				fbDev.VolumeLevelFeedback.LinkInputSig(
                    TriList.UShortInput[UIUshortJoin.VolumeSlider1Value]);
			}
		}

		/// <summary>
		/// Handler for when the room's volume control device changes
		/// </summary>
		void _CurrentRoom_CurrentAudioDeviceChange(object sender, VolumeDeviceChangeEventArgs args)
		{
			if (args.Type == ChangeType.WillChange)
				ClearAudioDeviceConnections();
			else // did change
				RefreshAudioDeviceConnections();
		}

        /// <summary>
        /// For room on/off changes
        /// </summary>
        void _CurrentRoom_OnFeedback_OutputChange(object sender, EventArgs e)
        {
            var value = _CurrentRoom.OnFeedback.BoolValue;
            TriList.BooleanInput[UIBoolJoin.RoomIsOn].BoolValue = value;
            if (value)
            {
                SetupActivityFooterWhenRoomOn();
                TriList.BooleanInput[UIBoolJoin.StartPageVisible].BoolValue = false;
            }
            else
            {
                HideCurrentSharingMode();
                SetupActivityFooterWhenRoomOff();
                TriList.BooleanInput[UIBoolJoin.StartPageVisible].BoolValue = true;
                if (LastSelectedSourceSig != null)
                {
                    LastSelectedSourceSig.BoolValue = false;
                    LastSelectedSourceSig = null;
                }
                PendingSource = null;
            }

            if (_CurrentRoom.HasAudioDialer)
            {
                TriList.BooleanInput[UIBoolJoin.VolumeDualMute1Visible].BoolValue = value;
                TriList.BooleanInput[UIBoolJoin.VolumeSingleMute1Visible].BoolValue = false;
            }
            else
            {
                TriList.BooleanInput[UIBoolJoin.VolumeDualMute1Visible].BoolValue = false;
                TriList.BooleanInput[UIBoolJoin.VolumeSingleMute1Visible].BoolValue = value;
            }
        }

		/// <summary>
		/// Handles source change
		/// </summary>
		void _CurrentRoom_SourceInfoChange(EssentialsRoomBase room, 
			SourceListItem info, ChangeType change)
		{
			if (change == ChangeType.WillChange)
				DisconnectSource(info);
			else
				RefreshSourceInfo();
		}

        /// <summary>
        /// 
        /// </summary>
        void _CurrentRoom_CurrentDisplay1SourceChange(EssentialsRoomBase room, SourceListItem info, ChangeType type)
        {
            if (type == ChangeType.DidChange)
            {
                var isSource = info != null;
                TriList.BooleanInput[UIBoolJoin.Display1SelectPressAndFb].BoolValue = isSource;
                TriList.StringInput[UIStringJoin.Display1SourceLabel].StringValue =
                    isSource ? info.PreferredName : "";
                if (!isSource) // return if no source
                {
                    TriList.BooleanInput[UIBoolJoin.Display1AudioButtonEnable].BoolValue = false;
                    TriList.BooleanInput[UIBoolJoin.Display1ControlButtonEnable].BoolValue = false;
                    return;
                }
                // enable audio and control buttons
                var devConfig = ConfigReader.ConfigObject.Devices.FirstOrDefault(d => d.Key == info.SourceKey);
                TriList.BooleanInput[UIBoolJoin.Display1AudioButtonEnable].BoolValue =
                    ConfigPropertiesHelpers.GetHasAudio(devConfig);
                TriList.BooleanInput[UIBoolJoin.Display1ControlButtonEnable].BoolValue =
                    ConfigPropertiesHelpers.GetHasControls(devConfig);
            }
        }

        /// <summary>
        /// 
        /// </summary>
        void _CurrentRoom_CurrentDisplay2SourceChange(EssentialsRoomBase room, SourceListItem info, ChangeType type)
        {
            if (type == ChangeType.DidChange)
            {
                var isSource = info != null;
                TriList.BooleanInput[UIBoolJoin.Display2SelectPressAndFb].BoolValue = isSource;
                TriList.StringInput[UIStringJoin.Display2SourceLabel].StringValue =
                    isSource ? info.PreferredName : "";
                if (!isSource)
                {
                    TriList.BooleanInput[UIBoolJoin.Display2AudioButtonEnable].BoolValue = false;
                    TriList.BooleanInput[UIBoolJoin.Display2ControlButtonEnable].BoolValue = false;
                    return;
                }
                // enable audio and control buttons
                var devConfig = ConfigReader.ConfigObject.Devices.FirstOrDefault(d => d.Key == info.SourceKey);
                TriList.BooleanInput[UIBoolJoin.Display2AudioButtonEnable].BoolValue =
                    ConfigPropertiesHelpers.GetHasAudio(devConfig);
                TriList.BooleanInput[UIBoolJoin.Display2ControlButtonEnable].BoolValue =
                    ConfigPropertiesHelpers.GetHasControls(devConfig);
            }
        }

	}
}