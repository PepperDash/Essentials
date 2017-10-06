using System;
using System.Linq;
using System.Collections.Generic;
using Crestron.SimplSharp;
using Crestron.SimplSharpPro;
using Crestron.SimplSharpPro.UI;

using PepperDash.Core;
using PepperDash.Essentials.Core;
using PepperDash.Essentials.Core.SmartObjects;
using PepperDash.Essentials.Core.PageManagers;
using PepperDash.Essentials.Room.Config;
using PepperDash.Essentials.Devices.Common.VideoCodec;

namespace PepperDash.Essentials
{
    /// <summary>
    /// 
    /// </summary>
    public class EssentialsHuddleVtc1PanelAvFunctionsDriver : PanelDriverBase, IAVDriver
    {
        CrestronTouchpanelPropertiesConfig Config;

        public enum UiDisplayMode
        {
            Presentation, AudioSetup, Call, Start
        }

        /// <summary>
        /// Whether volume ramping from this panel will show the volume
        /// gauge popup.
        /// </summary>
        public bool ShowVolumeGauge { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public uint PowerOffTimeout { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public string DefaultRoomKey { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public EssentialsHuddleVtc1Room CurrentRoom
        {
            get { return _CurrentRoom; }
            set
            {
                SetCurrentRoom(value);
            }
        }
        EssentialsHuddleVtc1Room _CurrentRoom;

        /// <summary>
        /// For hitting feedbacks
        /// </summary>
        BoolInputSig CallButtonSig;
        BoolInputSig ShareButtonSig;
        BoolInputSig EndMeetingButtonSig;

		public HeaderListButton HeaderCallButton { get; private set; }
		public HeaderListButton HeaderGearButton { get; private set; }

        /// <summary>
        /// The parent driver for this
        /// </summary>
        PanelDriverBase Parent;

        /// <summary>
        /// All children attached to this driver.  For hiding and showing as a group.
        /// </summary>
        List<PanelDriverBase> ChildDrivers = new List<PanelDriverBase>();

        List<BoolInputSig> CurrentDisplayModeSigsInUse = new List<BoolInputSig>();

        //// Important smart objects

        /// <summary>
        /// Smart Object 3200
        /// </summary>
        SubpageReferenceList SourceStagingSrl;

        /// <summary>
        /// Smart Object 15022
        /// </summary>
        SubpageReferenceList ActivityFooterSrl;

		/// <summary>
		/// 
		/// </summary>
		SubpageReferenceList MeetingsSrl;

		/// <summary>
		/// The list of buttons on the header. Managed with visibility only
		/// </summary>
		SmartObjectHeaderButtonList HeaderButtonsList;

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
        /// 
        /// </summary>
        ModalDialog PowerDownModal;

        /// <summary>
        /// 
        /// </summary>
        ModalDialog WarmingCoolingModal;

        /// <summary>
        /// Represents
        /// </summary>
        public JoinedSigInterlock PopupInterlock { get; private set; }

        /// <summary>
        /// Interlock for various source, camera, call control bars. The bar above the activity footer.  This is also 
        /// used to show start page
        /// </summary>
        JoinedSigInterlock StagingBarInterlock;

        /// <summary>
        /// Interlocks the various call-related subpages
        /// </summary>
        JoinedSigInterlock CallPagesInterlock;

        /// <summary>
        /// The Video codec driver
        /// </summary>
        PepperDash.Essentials.UIDrivers.VC.EssentialsVideoCodecUiDriver VCDriver;

        /// <summary>
        /// The driver for the tech page. Lazy getter for memory usage
        /// </summary>
        PepperDash.Essentials.UIDrivers.EssentialsHuddleTechPageDriver TechDriver
        {
            get
            {
                if (_TechDriver == null)
                    _TechDriver = new PepperDash.Essentials.UIDrivers.EssentialsHuddleTechPageDriver(TriList, this, CurrentRoom.Config.Tech);
                return _TechDriver;
            }
        }
        PepperDash.Essentials.UIDrivers.EssentialsHuddleTechPageDriver _TechDriver;

        /// <summary>
        /// Controls timeout of notification ribbon timer
        /// </summary>
        CTimer RibbonTimer;

        /// <summary>
        /// The keyboard
        /// </summary>
        public PepperDash.Essentials.Core.Touchpanels.Keyboards.HabaneroKeyboardController Keyboard { get; private set; }

        /// <summary>
        /// The mode showing. Presentation or call.
        /// </summary>
        UiDisplayMode CurrentMode = UiDisplayMode.Start;

		CTimer NextMeetingTimer;



        /// <summary>
        /// Constructor
        /// </summary>
        public EssentialsHuddleVtc1PanelAvFunctionsDriver(PanelDriverBase parent, CrestronTouchpanelPropertiesConfig config)
            : base(parent.TriList)
        {
            Config = config;
            Parent = parent;

            PopupInterlock = new JoinedSigInterlock(TriList);
            StagingBarInterlock = new JoinedSigInterlock(TriList);
            CallPagesInterlock = new JoinedSigInterlock(TriList);

            SourceStagingSrl = new SubpageReferenceList(TriList, UISmartObjectJoin.SourceStagingSRL, 3, 3, 3);

            ActivityFooterSrl = new SubpageReferenceList(TriList, UISmartObjectJoin.ActivityFooterSRL, 3, 3, 3);
            CallButtonSig = ActivityFooterSrl.BoolInputSig(2, 1);
            ShareButtonSig = ActivityFooterSrl.BoolInputSig(1, 1);
            EndMeetingButtonSig = ActivityFooterSrl.BoolInputSig(3, 1);

			MeetingsSrl = new SubpageReferenceList(TriList, UISmartObjectJoin.MeetingListSRL, 3, 3, 5);


			// buttons are added in SetCurrentRoom
			HeaderButtonsList = new SmartObjectHeaderButtonList(TriList.SmartObjects[UISmartObjectJoin.HeaderButtonList]);

            SetupActivityFooterWhenRoomOff();

            ShowVolumeGauge = true;
            Keyboard = new PepperDash.Essentials.Core.Touchpanels.Keyboards.HabaneroKeyboardController(TriList);
        }

        /// <summary>
        /// Add a video codec driver to this
        /// </summary>
        /// <param name="vcd"></param>
        public void SetVideoCodecDriver(PepperDash.Essentials.UIDrivers.VC.EssentialsVideoCodecUiDriver vcd)
        {
            VCDriver = vcd;
        }

        /// <summary>
        /// 
        /// </summary>
        public override void Show()
        {
            if (CurrentRoom == null)
            {
                Debug.Console(1, "ERROR: AVUIFunctionsDriver, Cannot show. No room assigned");
                return;
            }

            var roomConf = CurrentRoom.Config;

            if (Config.HeaderStyle == UiHeaderStyle.Habanero)
            {
                TriList.SetString(UIStringJoin.CurrentRoomName, CurrentRoom.Name);
                TriList.SetSigFalseAction(UIBoolJoin.HeaderRoomButtonPress, () =>
                    PopupInterlock.ShowInterlockedWithToggle(UIBoolJoin.RoomHeaderPageVisible));
            }
            else if (Config.HeaderStyle == UiHeaderStyle.Verbose)
            {
                // room name on join 1, concat phone and sip on join 2, no button method
                TriList.SetString(UIStringJoin.CurrentRoomName, CurrentRoom.Name);
                var addr = roomConf.Addresses;
                if (addr == null) // protect from missing values by using default empties
                    addr = new EssentialsRoomAddressPropertiesConfig();
                // empty string when either missing, pipe when both showing
                TriList.SetString(UIStringJoin.RoomAddressPipeText, 
                    (string.IsNullOrEmpty(addr.PhoneNumber.Trim())
                    || string.IsNullOrEmpty(addr.SipAddress.Trim())) ? "" : " | ");
                TriList.SetString(UIStringJoin.RoomPhoneText, addr.PhoneNumber);
                TriList.SetString(UIStringJoin.RoomSipText, addr.SipAddress);
            }

            TriList.SetBool(UIBoolJoin.DateAndTimeVisible, Config.ShowDate && Config.ShowTime);
            TriList.SetBool(UIBoolJoin.DateOnlyVisible, Config.ShowDate && !Config.ShowTime);
            TriList.SetBool(UIBoolJoin.TimeOnlyVisible, !Config.ShowDate && Config.ShowTime);

			TriList.SetBool(UIBoolJoin.TopBarHabaneroDynamicVisible, true);

            TriList.SetBool(UIBoolJoin.ActivityFooterVisible, true);

            // Privacy mute button
            TriList.SetSigFalseAction(UIBoolJoin.Volume1SpeechMutePressAndFB, CurrentRoom.PrivacyModeToggle);
            CurrentRoom.PrivacyModeIsOnFeedback.LinkInputSig(TriList.BooleanInput[UIBoolJoin.Volume1SpeechMutePressAndFB]);

            // Default to showing rooms/sources now.
            if (CurrentRoom.OnFeedback.BoolValue)
            {
                TriList.SetBool(UIBoolJoin.TapToBeginVisible, false);
            }
            else
            {
                TriList.SetBool(UIBoolJoin.StartPageVisible, true);
                TriList.SetBool(UIBoolJoin.TapToBeginVisible, true);
            }
            ShowCurrentDisplayModeSigsInUse();

            // *** Header Buttons ***
            
            // Generic "close" button for popup modals
            TriList.SetSigFalseAction(UIBoolJoin.InterlockedModalClosePress, PopupInterlock.HideAndClear);

            // Volume related things
            TriList.SetSigFalseAction(UIBoolJoin.VolumeDefaultPress, () => CurrentRoom.SetDefaultLevels());
            TriList.SetString(UIStringJoin.AdvancedVolumeSlider1Text, "Room");
            
            if (TriList is CrestronApp)
                TriList.BooleanInput[UIBoolJoin.GearButtonVisible].BoolValue = false;
            else
                TriList.BooleanInput[UIBoolJoin.GearButtonVisible].BoolValue = true;

            // power-related functions
            // Note: some of these are not directly-related to the huddle space UI, but are held over
            // in case
            TriList.SetSigFalseAction(UIBoolJoin.ShowPowerOffPress, PowerButtonPressed);

            TriList.SetSigFalseAction(UIBoolJoin.DisplayPowerTogglePress, () =>
            {
                if (CurrentRoom != null && CurrentRoom.DefaultDisplay is IPower)
                    (CurrentRoom.DefaultDisplay as IPower).PowerToggle();
            });

			SetupNextMeetingTimer();

            base.Show();
        }

        /// <summary>
        /// 
        /// </summary>
        void ShowLogo()
        {
            if (CurrentRoom.LogoUrl == null)
            {
                TriList.SetBool(UIBoolJoin.LogoDefaultVisible, true);
                TriList.SetBool(UIBoolJoin.LogoUrlVisible, false);
            }
            else
            {
                TriList.SetBool(UIBoolJoin.LogoDefaultVisible, false);
                TriList.SetBool(UIBoolJoin.LogoUrlVisible, true);
                TriList.SetString(UIStringJoin.LogoUrl, _CurrentRoom.LogoUrl);
            }
        }

        /// <summary>
        /// 
        /// </summary>
        void HideLogo()
        {
            TriList.SetBool(UIBoolJoin.LogoDefaultVisible, false);
            TriList.SetBool(UIBoolJoin.LogoUrlVisible, false);
        }

        /// <summary>
        /// 
        /// </summary>
        public override void Hide()
        {
            HideAndClearCurrentDisplayModeSigsInUse();
			TriList.SetBool(UIBoolJoin.TopBarHabaneroDynamicVisible, false);
            TriList.BooleanInput[UIBoolJoin.ActivityFooterVisible].BoolValue = false;
            TriList.BooleanInput[UIBoolJoin.StartPageVisible].BoolValue = false;
            TriList.BooleanInput[UIBoolJoin.TapToBeginVisible].BoolValue = false;
            TriList.BooleanInput[UIBoolJoin.SelectASourceVisible].BoolValue = false;
			if (NextMeetingTimer != null)
				NextMeetingTimer.Stop();
			HideNextMeetingPopup();
            base.Hide();
        }

        /// <summary>
        /// Reveals a message on the notification ribbon until cleared
        /// </summary>
        /// <param name="message">Text to display</param>
        /// <param name="timeout">Time in ms to display. 0 to keep on screen</param>
        public void ShowNotificationRibbon(string message, int timeout)
        {
            TriList.SetString(UIStringJoin.NotificationRibbonText, message);
            TriList.SetBool(UIBoolJoin.NotificationRibbonVisible, true);
            if (timeout > 0)
            {
                if (RibbonTimer != null)
                    RibbonTimer.Stop();
                RibbonTimer = new CTimer(o => {
                    TriList.SetBool(UIBoolJoin.NotificationRibbonVisible, false);
                    RibbonTimer = null;
                }, timeout);
            }
        }

        /// <summary>
        /// Hides the notification ribbon
        /// </summary>
        public void HideNotificationRibbon()
        {
            TriList.SetBool(UIBoolJoin.NotificationRibbonVisible, false);
            if (RibbonTimer != null)
            {
                RibbonTimer.Stop();
                RibbonTimer = null;
            }
        }

		void SetupNextMeetingTimer()
		{
			var ss = CurrentRoom.ScheduleSource;
			if (ss != null)
			{
				NextMeetingTimer = new CTimer(o =>
				{
					if (CurrentRoom.OnFeedback.BoolValue)
						return;
					// Every 60 seconds, check meetings list for the closest, joinable meeting
					var meetings = ss.CodecSchedule.Meetings;
					var meeting = meetings.Aggregate((m1, m2) => m1.StartTime < m2.StartTime ? m1 : m2);
					if (meeting != null && meeting.Joinable)
					{
						TriList.SetString(UIStringJoin.NextMeetingRibbonStartText, meeting.StartTime.ToShortTimeString());
						TriList.SetString(UIStringJoin.NextMeetingRibbonEndText, meeting.EndTime.ToShortTimeString());
						TriList.SetString(UIStringJoin.NextMeetingRibbonTitleText, meeting.Title);
						TriList.SetString(UIStringJoin.NextMettingRibbonNameText, meeting.Organizer);
						TriList.SetString(UIStringJoin.NextMeetingRibbonButtonLabel, "Join");
						TriList.SetSigFalseAction(UIBoolJoin.NextMeetingRibbonJoinPress, () =>
							{
								HideNextMeetingPopup();
								RoomOnAndDialMeeting(meeting.ConferenceNumberToDial);
							});
						TriList.SetString(UIStringJoin.NextMeetingSecondaryButtonLabel, "Show Schedule");
						TriList.SetSigFalseAction(UIBoolJoin.CalendarHeaderButtonPress, () =>
							{
								HideNextMeetingPopup();
								CalendarPress();
							});
						if (meetings.Count > 1)
						{
							TriList.SetString(UIStringJoin.NextMeetingFollowingMeetingText, 
								meetings[1].StartTime.ToShortTimeString());
						}

						ShowNextMeetingPopup();
					}
				}, null, 0, 60000);
			}
		}

		/// <summary>
		/// 
		/// </summary>
		void ShowNextMeetingPopup()
		{
			TriList.SetSigFalseAction(UIBoolJoin.NextMeetingRibbonClosePress, HideNextMeetingPopup);
			TriList.SetBool(UIBoolJoin.NextMeetingRibbonVisible, true);
		}
		
		/// <summary>
		/// 
		/// </summary>
		void HideNextMeetingPopup()
		{
			TriList.SetBool(UIBoolJoin.NextMeetingRibbonVisible, false);
		}

		/// <summary>
		/// Calendar should only be visible when it's supposed to
		/// </summary>
		void CalendarPress()
		{
			RefreshMeetingsList();
			PopupInterlock.ShowInterlockedWithToggle(UIBoolJoin.MeetingsListVisible);
		}

		/// <summary>
		/// Dials a meeting after turning on room (if necessary)
		/// </summary>
		void RoomOnAndDialMeeting(string number)
		{
			Action dialAction = () => 
				{
					var d = CurrentRoom.ScheduleSource as VideoCodecBase;
					if (d != null)
						d.Dial(number);
				};
			if (CurrentRoom.OnFeedback.BoolValue)
				dialAction();
			else
			{
				// Rig a one-time handler to catch when the room is warmed and then dial call
				EventHandler<EventArgs> oneTimeHandler = null;
				oneTimeHandler = (o, a) =>
					{
						if (!CurrentRoom.IsWarmingUpFeedback.BoolValue)
						{
							CurrentRoom.IsWarmingUpFeedback.OutputChange -= oneTimeHandler;
							dialAction();
						}
					};
				CurrentRoom.IsWarmingUpFeedback.OutputChange += oneTimeHandler;
				ActivityCallButtonPressed();
			}
		}

        /// <summary>
        /// Reveals the tech page and puts away anything that's in the way.
        /// </summary>
        void ShowTech()
        {
            PopupInterlock.HideAndClear();
            TechDriver.Show();
        }

        /// <summary>
        /// When the room is off, set the footer SRL
        /// </summary>
        void SetupActivityFooterWhenRoomOff()
        {
            ActivityFooterSrl.Clear();
            ActivityFooterSrl.AddItem(new SubpageReferenceListActivityItem(1, ActivityFooterSrl, 0,
                b => { if (!b) ActivityShareButtonPressed(); }));
            ActivityFooterSrl.AddItem(new SubpageReferenceListActivityItem(2, ActivityFooterSrl, 3,
                b => { if (!b) ActivityCallButtonPressed(); }));
            ActivityFooterSrl.Count = 2;
            TriList.SetUshort(UIUshortJoin.PresentationStagingCaretMode, 1); // right one slot
            TriList.SetUshort(UIUshortJoin.CallStagingCaretMode, 5); // left one slot
        }

        /// <summary>
        /// Sets up the footer SRL for when the room is on
        /// </summary>
        void SetupActivityFooterWhenRoomOn()
        {
            ActivityFooterSrl.Clear();
            ActivityFooterSrl.AddItem(new SubpageReferenceListActivityItem(1, ActivityFooterSrl, 0, 
                b => { if (!b) ActivityShareButtonPressed(); }));
            ActivityFooterSrl.AddItem(new SubpageReferenceListActivityItem(2, ActivityFooterSrl, 3,
                b => { if (!b) ActivityCallButtonPressed(); }));
            ActivityFooterSrl.AddItem(new SubpageReferenceListActivityItem(3, ActivityFooterSrl, 4, 
                b => { if (!b) PowerButtonPressed(); }));
            ActivityFooterSrl.Count = 3;
            TriList.SetUshort(UIUshortJoin.PresentationStagingCaretMode, 2); // center
            TriList.SetUshort(UIUshortJoin.CallStagingCaretMode, 0); // left -2
        }

        /// <summary>
        /// Single point call for setting the feedbacks on the activity buttons
        /// </summary>
        void SetActivityFooterFeedbacks()
        {
            CallButtonSig.BoolValue = CurrentMode == UiDisplayMode.Call 
                && CurrentRoom.ShutdownType == eShutdownType.None;
            ShareButtonSig.BoolValue = CurrentMode == UiDisplayMode.Presentation 
                && CurrentRoom.ShutdownType == eShutdownType.None;
            EndMeetingButtonSig.BoolValue = CurrentRoom.ShutdownType != eShutdownType.None;
        }

        /// <summary>
        /// 
        /// </summary>
        void ActivityCallButtonPressed()
        {
            if (VCDriver.IsVisible)
                return;
            HideLogo();
            TriList.SetBool(UIBoolJoin.StartPageVisible, false);
            TriList.SetBool(UIBoolJoin.SourceStagingBarVisible, false);
            TriList.SetBool(UIBoolJoin.SelectASourceVisible, false);
            if (CurrentSourcePageManager != null)
                CurrentSourcePageManager.Hide();
            if (!CurrentRoom.OnFeedback.BoolValue)
            {
                CurrentRoom.RunDefaultCallRoute();
            }
            CurrentMode = UiDisplayMode.Call;
            SetActivityFooterFeedbacks();
            VCDriver.Show();
        }

        /// <summary>
        /// Attached to activity list share button
        /// </summary>
        void ActivityShareButtonPressed()
        {
            if (VCDriver.IsVisible)
                VCDriver.Hide();
            TriList.SetBool(UIBoolJoin.StartPageVisible, false);
            TriList.SetBool(UIBoolJoin.CallStagingBarVisible, false);
            TriList.SetBool(UIBoolJoin.SourceStagingBarVisible, true);
            // Run default source when room is off and share is pressed
            if (!CurrentRoom.OnFeedback.BoolValue)
            { 
                // If there's no default, show UI elements
                if(!CurrentRoom.RunDefaultPresentRoute())
                    TriList.SetBool(UIBoolJoin.SelectASourceVisible, true);
            }
            else // room is on show what's active or select a source if nothing is yet active
            {
                if(CurrentRoom.CurrentSourceInfo == null || CurrentRoom.CurrentSourceInfoKey == CurrentRoom.DefaultCodecRouteString)
                    TriList.SetBool(UIBoolJoin.SelectASourceVisible, true);
                else if (CurrentSourcePageManager != null)
                    CurrentSourcePageManager.Show();
            }
            CurrentMode = UiDisplayMode.Presentation;
            SetActivityFooterFeedbacks();
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
        /// Loads the appropriate Sigs into CurrentDisplayModeSigsInUse and shows them
        /// </summary>
        void ShowCurrentSource()
        {
            if (CurrentRoom.CurrentSourceInfo == null)
                return;

            var uiDev = CurrentRoom.CurrentSourceInfo.SourceDevice as IUiDisplayInfo;
            PageManager pm = null;
            // If we need a page manager, get an appropriate one
            if (uiDev != null)
            {
                TriList.BooleanInput[UIBoolJoin.SelectASourceVisible].BoolValue = false;
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
        /// Called from button presses on source, where We can assume we want
        /// to change to the proper screen.
        /// </summary>
        /// <param name="key">The key name of the route to run</param>
        void UiSelectSource(string key)
        {
            // Run the route and when it calls back, show the source
            CurrentRoom.RunRouteAction(key, null);
        }

        /// <summary>
        /// 
        /// </summary>
        public void PowerButtonPressed()
        {
            if (!CurrentRoom.OnFeedback.BoolValue
                || CurrentRoom.ShutdownPromptTimer.IsRunningFeedback.BoolValue)
                return;

            CurrentRoom.StartShutdown(eShutdownType.Manual);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void ShutdownPromptTimer_HasStarted(object sender, EventArgs e)
        {
            // Do we need to check where the UI is? No?
            var timer = CurrentRoom.ShutdownPromptTimer;
            SetActivityFooterFeedbacks();

            if (CurrentRoom.ShutdownType == eShutdownType.Manual)
            {
                PowerDownModal = new ModalDialog(TriList);
                var message = string.Format("Meeting will end in {0} seconds", CurrentRoom.ShutdownPromptSeconds);

                // Attach timer things to modal
                CurrentRoom.ShutdownPromptTimer.TimeRemainingFeedback.OutputChange += ShutdownPromptTimer_TimeRemainingFeedback_OutputChange;
                CurrentRoom.ShutdownPromptTimer.PercentFeedback.OutputChange += ShutdownPromptTimer_PercentFeedback_OutputChange;

                // respond to offs by cancelling dialog
                var onFb = CurrentRoom.OnFeedback;
                EventHandler<EventArgs> offHandler = null;
                offHandler = (o, a) =>
                {
                    if (!onFb.BoolValue)
                    {
                        PowerDownModal.HideDialog();
                        SetActivityFooterFeedbacks();
                        onFb.OutputChange -= offHandler;
                    }
                };
                onFb.OutputChange += offHandler;

                PowerDownModal.PresentModalDialog(2, "End Meeting", "Power", message, "Cancel", "End Meeting Now", true, true,
                    but =>
                    {
                        if (but != 2) // any button except for End cancels
                            timer.Cancel();
                        else
                            timer.Finish();
                    });
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void ShutdownPromptTimer_HasFinished(object sender, EventArgs e)
        {
            SetActivityFooterFeedbacks();
            CurrentRoom.ShutdownPromptTimer.TimeRemainingFeedback.OutputChange -= ShutdownPromptTimer_TimeRemainingFeedback_OutputChange;
            CurrentRoom.ShutdownPromptTimer.PercentFeedback.OutputChange -= ShutdownPromptTimer_PercentFeedback_OutputChange;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void ShutdownPromptTimer_WasCancelled(object sender, EventArgs e)
        {
            if (PowerDownModal != null)
                PowerDownModal.HideDialog();
            SetActivityFooterFeedbacks();

            CurrentRoom.ShutdownPromptTimer.TimeRemainingFeedback.OutputChange += ShutdownPromptTimer_TimeRemainingFeedback_OutputChange;
            CurrentRoom.ShutdownPromptTimer.PercentFeedback.OutputChange -= ShutdownPromptTimer_PercentFeedback_OutputChange;
        }

        /// <summary>
        /// Event handler for countdown timer on power off modal
        /// </summary>
        void ShutdownPromptTimer_TimeRemainingFeedback_OutputChange(object sender, EventArgs e)
        {

            var message = string.Format("Meeting will end in {0} seconds", (sender as StringFeedback).StringValue);
            TriList.StringInput[ModalDialog.MessageTextJoin].StringValue = message;
        }

        /// <summary>
        /// Event handler for percentage on power off countdown
        /// </summary>
        void ShutdownPromptTimer_PercentFeedback_OutputChange(object sender, EventArgs e)
        {
            var value = (ushort)((sender as IntFeedback).UShortValue * 65535 / 100);
            TriList.UShortInput[ModalDialog.TimerGaugeJoin].UShortValue = value;
        }

        /// <summary>
        /// 
        /// </summary>
        void CancelPowerOffTimer()
        {
            if (PowerOffTimer != null)
            {
                PowerOffTimer.Stop();
                PowerOffTimer = null;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="state"></param>
        public void VolumeUpPress(bool state)
        {
            if (CurrentRoom.CurrentVolumeControls != null)
                CurrentRoom.CurrentVolumeControls.VolumeUp(state);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="state"></param>
        public void VolumeDownPress(bool state)
        {
            if (CurrentRoom.CurrentVolumeControls != null)
                CurrentRoom.CurrentVolumeControls.VolumeDown(state);
        }

        /// <summary>
        /// Helper for property setter. Sets the panel to the given room, latching up all functionality
        /// </summary>
        void SetCurrentRoom(EssentialsHuddleVtc1Room room)
        {
            if (_CurrentRoom == room) return;
            // Disconnect current (probably never called)
            if (_CurrentRoom != null)
            {
                // Disconnect current room
                _CurrentRoom.CurrentVolumeDeviceChange -= this.CurrentRoom_CurrentAudioDeviceChange;
                ClearAudioDeviceConnections();
                _CurrentRoom.CurrentSingleSourceChange -= this.CurrentRoom_SourceInfoChange;
                DisconnectSource(_CurrentRoom.CurrentSourceInfo);
                _CurrentRoom.ShutdownPromptTimer.HasStarted -= ShutdownPromptTimer_HasStarted;
                _CurrentRoom.ShutdownPromptTimer.HasFinished -= ShutdownPromptTimer_HasFinished;
                _CurrentRoom.ShutdownPromptTimer.WasCancelled -= ShutdownPromptTimer_WasCancelled;

                _CurrentRoom.OnFeedback.OutputChange += CurrentRoom_OnFeedback_OutputChange;
                _CurrentRoom.IsWarmingUpFeedback.OutputChange -= CurrentRoom_IsWarmingFeedback_OutputChange;
                _CurrentRoom.IsCoolingDownFeedback.OutputChange -= CurrentRoom_IsCoolingDownFeedback_OutputChange;
            }

            _CurrentRoom = room;

            if (_CurrentRoom != null)
            {
                // get the source list config and set up the source list
                var config = ConfigReader.ConfigObject.SourceLists;
                if (config.ContainsKey(_CurrentRoom.SourceListKey))
                {
                    var srcList = config[_CurrentRoom.SourceListKey];
                    // Setup sources list			
                    uint i = 1; // counter for UI list
                    foreach (var kvp in srcList)
                    {
                        var srcConfig = kvp.Value;
                        if (!srcConfig.IncludeInSourceList) // Skip sources marked this way
                            continue;

                        //var actualSource = DeviceManager.GetDeviceForKey(srcConfig.SourceKey) as Device;
                        //if (actualSource == null)
                        //{
                        //    Debug.Console(1, "Cannot assign missing source '{0}' to source UI list",
                        //        srcConfig.SourceKey);
                        //    continue;
                        //}
                        var routeKey = kvp.Key;
                        var item = new SubpageReferenceListSourceItem(i++, SourceStagingSrl, srcConfig,
                            b => { if (!b) UiSelectSource(routeKey); });
                        SourceStagingSrl.AddItem(item); // add to the SRL
                        item.RegisterForSourceChange(_CurrentRoom);
                    }
                    SourceStagingSrl.Count = (ushort)(i - 1);
                }
                // Name and logo
                TriList.StringInput[UIStringJoin.CurrentRoomName].StringValue = _CurrentRoom.Name;
                ShowLogo();

                // Shutdown timer
                _CurrentRoom.ShutdownPromptTimer.HasStarted += ShutdownPromptTimer_HasStarted;
                _CurrentRoom.ShutdownPromptTimer.HasFinished += ShutdownPromptTimer_HasFinished;
                _CurrentRoom.ShutdownPromptTimer.WasCancelled += ShutdownPromptTimer_WasCancelled;

				// Link up all the change events from the room
                _CurrentRoom.OnFeedback.OutputChange += CurrentRoom_OnFeedback_OutputChange;
                CurrentRoom_SyncOnFeedback();
                _CurrentRoom.IsWarmingUpFeedback.OutputChange += CurrentRoom_IsWarmingFeedback_OutputChange;
                _CurrentRoom.IsCoolingDownFeedback.OutputChange += CurrentRoom_IsCoolingDownFeedback_OutputChange;

                _CurrentRoom.CurrentVolumeDeviceChange += CurrentRoom_CurrentAudioDeviceChange;
                RefreshAudioDeviceConnections();
                _CurrentRoom.CurrentSingleSourceChange += CurrentRoom_SourceInfoChange;
                RefreshSourceInfo();

				SetupHeaderButtons();
            }
            else
            {
                // Clear sigs that need to be
                TriList.StringInput[UIStringJoin.CurrentRoomName].StringValue = "Select a room";
            }
        }

		/// <summary>
		/// 
		/// </summary>
		void SetupHeaderButtons()
		{
			TriList.SetBool(UIBoolJoin.TopBarHabaneroDynamicVisible, true);

			var roomConf = CurrentRoom.Config;
			//
			HeaderGearButton = new HeaderListButton(HeaderButtonsList, 5);
			HeaderGearButton.SetIcon(HeaderListButton.Gear);
			HeaderGearButton.OutputSig.SetSigHeldAction(2000,
				ShowTech,
				() =>
				{
					if (CurrentRoom.OnFeedback.BoolValue)
						PopupInterlock.ShowInterlockedWithToggle(UIBoolJoin.VolumesPageVisible);
					else
						PopupInterlock.ShowInterlockedWithToggle(UIBoolJoin.VolumesPagePowerOffVisible);
				});

			TriList.SetSigFalseAction(UIBoolJoin.TechExitButton, () =>
				PopupInterlock.HideAndClear());

			// Help button and popup
			if (CurrentRoom.Config.Help != null)
			{
				TriList.SetString(UIStringJoin.HelpMessage, roomConf.Help.Message);
				TriList.SetBool(UIBoolJoin.HelpPageShowCallButtonVisible, roomConf.Help.ShowCallButton);
				TriList.SetString(UIStringJoin.HelpPageCallButtonText, roomConf.Help.CallButtonText);
				if (roomConf.Help.ShowCallButton)
					TriList.SetSigFalseAction(UIBoolJoin.HelpPageShowCallButtonPress, () => { }); // ************ FILL IN
				else
					TriList.ClearBoolSigAction(UIBoolJoin.HelpPageShowCallButtonPress);
			}
			else // older config
			{
				TriList.SetString(UIStringJoin.HelpMessage, CurrentRoom.Config.HelpMessage);
				TriList.SetBool(UIBoolJoin.HelpPageShowCallButtonVisible, false);
				TriList.SetString(UIStringJoin.HelpPageCallButtonText, null);
				TriList.ClearBoolSigAction(UIBoolJoin.HelpPageShowCallButtonPress);
			}
			var helpButton = new HeaderListButton(HeaderButtonsList, 4);
			helpButton.SetIcon(HeaderListButton.Help);
			helpButton.OutputSig.SetSigFalseAction(() =>
			{
				string message = null;
				var room = DeviceManager.GetDeviceForKey(Config.DefaultRoomKey)
					as EssentialsHuddleSpaceRoom;
				if (room != null)
					message = room.Config.HelpMessage;
				else
					message = "Sorry, no help message available. No room connected.";
				//TriList.StringInput[UIStringJoin.HelpMessage].StringValue = message;
				PopupInterlock.ShowInterlockedWithToggle(UIBoolJoin.HelpPageVisible);
			});
			uint nextIndex = 3;

			// Lights button
			//if (WHATEVER MAKES LIGHTS WORK)
			//{
			//    // do lights
			//    nextIndex--;
			//}

			// Calendar button
			if (_CurrentRoom.ScheduleSource != null) // ******************* Do we need a config option here as well?
			{
				var calBut = new HeaderListButton(HeaderButtonsList, nextIndex);
				calBut.SetIcon(HeaderListButton.Calendar);
				calBut.OutputSig.SetSigFalseAction(CalendarPress);
				nextIndex--;
			}

			// Call button
			HeaderCallButton = new HeaderListButton(HeaderButtonsList, nextIndex);
			HeaderCallButton.SetIcon(HeaderListButton.OnHook);
			HeaderCallButton.OutputSig.SetSigFalseAction(() =>
				PopupInterlock.ShowInterlockedWithToggle(UIBoolJoin.HeaderActiveCallsListVisible));
			
			nextIndex--;

			// blank any that remain
			for (var i = nextIndex; i > 0; i--)
			{
				var blankBut = new HeaderListButton(HeaderButtonsList, i);
				blankBut.ClearIcon();
				blankBut.OutputSig.SetSigFalseAction(() => { });
			}
		}

		/// <summary>
		/// 
		/// </summary>
		void RefreshMeetingsList()
		{
			ushort i = 0;
			foreach (var m in CurrentRoom.ScheduleSource.CodecSchedule.Meetings)
			{
				i++;
				MeetingsSrl.StringInputSig(i, 1).StringValue = m.StartTime.ToShortTimeString();
				MeetingsSrl.StringInputSig(i, 2).StringValue = m.EndTime.ToShortTimeString();
				MeetingsSrl.StringInputSig(i, 3).StringValue = m.Title;
				MeetingsSrl.StringInputSig(i, 4).StringValue = "Join";
				MeetingsSrl.BoolInputSig(i, 2).BoolValue = m.Joinable;
				var mm = m; // lambda scope
				MeetingsSrl.GetBoolFeedbackSig(i, 1).SetSigFalseAction(() =>
				{
					PopupInterlock.Hide();
					var d = CurrentRoom.ScheduleSource as VideoCodecBase;
					if (d != null)
						RoomOnAndDialMeeting(mm.ConferenceNumberToDial);
				});
			}
			MeetingsSrl.Count = i;
		}

		/// <summary>
		/// For room on/off changes
		/// </summary>
		void CurrentRoom_OnFeedback_OutputChange(object sender, EventArgs e)
        {
            CurrentRoom_SyncOnFeedback();
        }

        /// <summary>
        /// 
        /// </summary>
        void CurrentRoom_SyncOnFeedback()
        {
            var value = _CurrentRoom.OnFeedback.BoolValue;
            TriList.BooleanInput[UIBoolJoin.RoomIsOn].BoolValue = value;

            if (value) //ON
            {
                SetupActivityFooterWhenRoomOn();
                TriList.BooleanInput[UIBoolJoin.SelectASourceVisible].BoolValue = false;
                //TriList.BooleanInput[UIBoolJoin.SourceStagingBarVisible].BoolValue = true;
                TriList.BooleanInput[UIBoolJoin.StartPageVisible].BoolValue = false;
                TriList.BooleanInput[UIBoolJoin.VolumeDualMute1Visible].BoolValue = true;

            }
            else
            {
                CurrentMode = UiDisplayMode.Start;
                if (VCDriver.IsVisible)
                    VCDriver.Hide();
                SetupActivityFooterWhenRoomOff();
                ShowLogo();
                SetActivityFooterFeedbacks();
                TriList.BooleanInput[UIBoolJoin.StartPageVisible].BoolValue = true;
                TriList.BooleanInput[UIBoolJoin.VolumeDualMute1Visible].BoolValue = false;
                TriList.BooleanInput[UIBoolJoin.SourceStagingBarVisible].BoolValue = false;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        void CurrentRoom_IsWarmingFeedback_OutputChange(object sender, EventArgs e)
        {
            if (CurrentRoom.IsWarmingUpFeedback.BoolValue)
            {
                WarmingCoolingModal = new ModalDialog(TriList);
                WarmingCoolingModal.PresentModalDialog(0, "Powering Up", "Power", "<p>Room is powering up</p><p>Please wait</p>",
                    "", "", false, false, null);
            }
            else
            {
                if (WarmingCoolingModal != null)
                    WarmingCoolingModal.CancelDialog();
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void CurrentRoom_IsCoolingDownFeedback_OutputChange(object sender, EventArgs e)
        {
            if (CurrentRoom.IsCoolingDownFeedback.BoolValue)
            {
                WarmingCoolingModal = new ModalDialog(TriList);
                WarmingCoolingModal.PresentModalDialog(0, "Shut Down", "Power", "<p>Room is shutting down</p><p>Please wait</p>",
                    "", "", false, false, null);
            }
            else
            {
                if (WarmingCoolingModal != null)
                    WarmingCoolingModal.CancelDialog();
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
            var routeInfo = CurrentRoom.CurrentSourceInfo;
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
            else if (CurrentRoom.CurrentSourceInfo != null)
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
            if (dev is ISetTopBoxControls)
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
        void CurrentRoom_CurrentAudioDeviceChange(object sender, VolumeDeviceChangeEventArgs args)
        {
            if (args.Type == ChangeType.WillChange)
                ClearAudioDeviceConnections();
            else // did change
                RefreshAudioDeviceConnections();
        }

        /// <summary>
        /// Handles source change
        /// </summary>
        void CurrentRoom_SourceInfoChange(EssentialsRoomBase room,
            SourceListItem info, ChangeType change)
        {
            if (change == ChangeType.WillChange)
                DisconnectSource(info);
            else
                RefreshSourceInfo();
        }
    }

    /// <summary>
    /// For hanging off various common things that child drivers might need from a parent AV driver
    /// </summary>
    public interface IAVDriver
    {
        PepperDash.Essentials.Core.Touchpanels.Keyboards.HabaneroKeyboardController Keyboard { get; }
        JoinedSigInterlock PopupInterlock { get; }
        void ShowNotificationRibbon(string message, int timeout);
        void HideNotificationRibbon();
		HeaderListButton HeaderGearButton { get; }
		HeaderListButton HeaderCallButton { get; }
    }
}
