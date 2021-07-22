using System;
using System.Collections.Generic;
using Crestron.SimplSharp;
using Crestron.SimplSharpPro;
using Crestron.SimplSharpPro.UI;

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
	public class EssentialsHuddlePanelAvFunctionsDriver : PanelDriverBase, IAVDriver
	{
		CrestronTouchpanelPropertiesConfig Config;

		public enum UiDisplayMode
		{
			PresentationMode, AudioSetup
		}

        public uint StartPageVisibleJoin { get; private set; }


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
				//CurrentRoom = DeviceManager.GetDeviceForKey(value) as EssentialsHuddleSpaceRoom;
			}	
		}
		string _DefaultRoomKey;

        /// <summary>
        /// Indicates that the SetHeaderButtons method has completed successfully
        /// </summary>
        public bool HeaderButtonsAreSetUp { get; private set; }

		/// <summary>
		/// 
		/// </summary>
		public IEssentialsHuddleSpaceRoom CurrentRoom
		{
			get { return _CurrentRoom; }
			set
			{
				SetCurrentRoom(value);
			}
		}
		IEssentialsHuddleSpaceRoom _CurrentRoom;

        /// <summary>
        /// 
        /// </summary>
        //uint CurrentInterlockedModalJoin;

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
        public PanelDriverBase Parent { get; private set; }

        /// <summary>
        /// All children attached to this driver.  For hiding and showing as a group.
        /// </summary>
        List<PanelDriverBase> ChildDrivers = new List<PanelDriverBase>();

		List<BoolInputSig> CurrentDisplayModeSigsInUse = new List<BoolInputSig>();

		//// Important smart objects

		/// <summary>
		/// Smart Object 3200
		/// </summary>
        SubpageReferenceList SourcesSrl;

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

        ModalDialog PowerDownModal;

        public JoinedSigInterlock PopupInterlock { get; private set; }

        /// <summary>
        /// The driver for the tech page. Lazy getter for memory usage
        /// </summary>
        PepperDash.Essentials.UIDrivers.EssentialsHuddleTechPageDriver TechDriver
        {
            get
            {
                if (_TechDriver == null)
                    _TechDriver = new PepperDash.Essentials.UIDrivers.EssentialsHuddleTechPageDriver(TriList, CurrentRoom.PropertiesConfig.Tech);
                return _TechDriver;
            }
        }
        PepperDash.Essentials.UIDrivers.EssentialsHuddleTechPageDriver _TechDriver;


        /// <summary>
        /// Controls timeout of notification ribbon timer
        /// </summary>
        CTimer RibbonTimer;

		/// <summary>
		/// Constructor
		/// </summary>
		public EssentialsHuddlePanelAvFunctionsDriver(PanelDriverBase parent, CrestronTouchpanelPropertiesConfig config) 
			: base(parent.TriList)
		{
			Config = config;
            Parent = parent;
            PopupInterlock = new JoinedSigInterlock(TriList);

            SourcesSrl = new SubpageReferenceList(TriList, 3200, 3, 3, 3);
            ActivityFooterSrl = new SubpageReferenceList(TriList, 15022, 3, 3, 3);
            ShareButtonSig = ActivityFooterSrl.BoolInputSig(1, 1);

            SetupActivityFooterWhenRoomOff();

			ShowVolumeGauge = true;

			// One-second pulse extender for volume gauge
			VolumeGaugeFeedback = new BoolFeedbackPulseExtender(1500);
			VolumeGaugeFeedback.Feedback
				.LinkInputSig(TriList.BooleanInput[UIBoolJoin.VolumeGaugePopupVisible]);

			VolumeButtonsPopupFeedback = new BoolFeedbackPulseExtender(4000);
			VolumeButtonsPopupFeedback.Feedback
				.LinkInputSig(TriList.BooleanInput[UIBoolJoin.VolumeButtonPopupVisible]);

			PowerOffTimeout = 30000;

            TriList.StringInput[UIStringJoin.StartActivityText].StringValue =
                "Tap Share to begin";
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

            var roomConf = CurrentRoom.PropertiesConfig;

            if (Config.HeaderStyle.ToLower() == CrestronTouchpanelPropertiesConfig.Habanero)
            {
                TriList.SetSigFalseAction(UIBoolJoin.HeaderRoomButtonPress, () =>
                    {
                        if (CurrentRoom.IsMobileControlEnabled)
                        {
                            Debug.Console(1, "Showing Mobile Control Header Info");
                            PopupInterlock.ShowInterlockedWithToggle(UIBoolJoin.RoomHeaderInfoMCPageVisible);
                        }
                        else
                        {
                            Debug.Console(1, "Showing Non Mobile Control Header Info");
                            PopupInterlock.ShowInterlockedWithToggle(UIBoolJoin.RoomHeaderInfoPageVisible);
                        }
                    });
            }
            else if (Config.HeaderStyle.ToLower() == CrestronTouchpanelPropertiesConfig.Verbose)
            {
                TriList.SetSigFalseAction(UIBoolJoin.HeaderRoomButtonPress, () =>
                {
                    if (CurrentRoom.IsMobileControlEnabled)
                    {
                        Debug.Console(1, "Showing Mobile Control Header Info");
                        PopupInterlock.ShowInterlockedWithToggle(UIBoolJoin.RoomHeaderInfoMCPageVisible);
                    }
                    else
                    {
                        Debug.Console(1, "Showing Non Mobile Control Header Info");
                        PopupInterlock.ShowInterlockedWithToggle(UIBoolJoin.RoomHeaderInfoPageVisible);
                    }
                });
            }

            TriList.SetBool(UIBoolJoin.DateAndTimeVisible, Config.ShowDate && Config.ShowTime);
            TriList.SetBool(UIBoolJoin.DateOnlyVisible, Config.ShowDate && !Config.ShowTime);
            TriList.SetBool(UIBoolJoin.TimeOnlyVisible, !Config.ShowDate && Config.ShowTime);

            TriList.SetBool(UIBoolJoin.TopBarHabaneroDynamicVisible, true);
            TriList.BooleanInput[UIBoolJoin.ActivityFooterVisible].BoolValue = true;

            // Default to showing rooms/sources now.
            if (CurrentRoom.OnFeedback.BoolValue)
            {
                TriList.SetBool(UIBoolJoin.TapToBeginVisible, false);
                SetupActivityFooterWhenRoomOn();
            }
            else
            {
                TriList.SetBool(StartPageVisibleJoin, true);
                TriList.SetBool(UIBoolJoin.TapToBeginVisible, true);
                SetupActivityFooterWhenRoomOff();
            }
            ShowCurrentDisplayModeSigsInUse();

			// Attach actions
			TriList.SetSigFalseAction(UIBoolJoin.VolumeButtonPopupPress, VolumeButtonsTogglePress);

            // Generic "close" button for popup modals
            TriList.SetSigFalseAction(UIBoolJoin.InterlockedModalClosePress, PopupInterlock.HideAndClear);

            // Volume related things
            TriList.SetSigFalseAction(UIBoolJoin.VolumeDefaultPress, () => CurrentRoom.SetDefaultLevels());
            TriList.SetString(UIStringJoin.AdvancedVolumeSlider1Text, "Room"); 

            //TriList.SetSigFalseAction(UIBoolJoin.RoomHeaderButtonPress, () =>
            //    ShowInterlockedModal(UIBoolJoin.RoomHeaderPageVisible));


            //if(TriList is CrestronApp)
            //    TriList.BooleanInput[UIBoolJoin.GearButtonVisible].BoolValue = false;
            //else
            //    TriList.BooleanInput[UIBoolJoin.GearButtonVisible].BoolValue = true;

			// power-related functions
            // Note: some of these are not directly-related to the huddle space UI, but are held over
            // in case
            TriList.SetSigFalseAction(UIBoolJoin.ShowPowerOffPress, EndMeetingPress);

			TriList.SetSigFalseAction(UIBoolJoin.DisplayPowerTogglePress, () =>
				{
                    if (CurrentRoom != null && CurrentRoom.DefaultDisplay != null && CurrentRoom.DefaultDisplay is IHasPowerControl)
                        (CurrentRoom.DefaultDisplay as IHasPowerControl).PowerToggle();
				});

			base.Show();
		}

        /// <summary>
        /// 
        /// </summary>
        public void EndMeetingPress()
        {
            if (!CurrentRoom.OnFeedback.BoolValue
                || CurrentRoom.ShutdownPromptTimer.IsRunningFeedback.BoolValue)
                return;

            CurrentRoom.StartShutdown(eShutdownType.Manual);
        }

        /// <summary>
        /// Reveals the tech page and puts away anything that's in the way.
        /// </summary>
        public void ShowTech()
        {
            PopupInterlock.HideAndClear();
            TechDriver.Show();
        }

        /// <summary>
        /// 
        /// </summary>
        void ShowLogo()
        {
            if (CurrentRoom.LogoUrlLightBkgnd == null)
            {
                TriList.SetBool(UIBoolJoin.LogoDefaultVisible, true);
                TriList.SetBool(UIBoolJoin.LogoUrlVisible, false);
            }
            else
            {
                TriList.SetBool(UIBoolJoin.LogoDefaultVisible, false);
                TriList.SetBool(UIBoolJoin.LogoUrlVisible, true);
                TriList.SetString(UIStringJoin.LogoUrlLightBkgnd, _CurrentRoom.LogoUrlLightBkgnd);
                TriList.SetString(UIStringJoin.LogoUrlDarkBkgnd, _CurrentRoom.LogoUrlDarkBkgnd);
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
            TriList.BooleanInput[UIBoolJoin.TopBarHabaneroDynamicVisible].BoolValue = false;
            TriList.BooleanInput[UIBoolJoin.ActivityFooterVisible].BoolValue = false;
            TriList.BooleanInput[StartPageVisibleJoin].BoolValue = false;
            TriList.BooleanInput[UIBoolJoin.TapToBeginVisible].BoolValue = false;
            TriList.BooleanInput[UIBoolJoin.SelectASourceVisible].BoolValue = false;
            //TriList.BooleanInput[UIBoolJoin.StagingPageVisible].BoolValue = false;
			VolumeButtonsPopupFeedback.ClearNow();
            //CancelPowerOff();

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
                RibbonTimer = new CTimer(o =>
                {
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
                        TriList.BooleanInput[UIBoolJoin.SourceStagingBarVisible].BoolValue = true;
                        TriList.BooleanInput[UIBoolJoin.TapToBeginVisible].BoolValue = false;
                        TriList.BooleanInput[UIBoolJoin.SelectASourceVisible].BoolValue = false;
                    }
                    else
                    {
                        TriList.BooleanInput[StartPageVisibleJoin].BoolValue = true;
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

					ShowCurrentDisplayModeSigsInUse();
					break;
			}
		}

        /// <summary>
        /// When the room is off, set the footer SRL
        /// </summary>
        void SetupActivityFooterWhenRoomOff()
        {
            ActivityFooterSrl.Clear();
            ActivityFooterSrl.AddItem(new SubpageReferenceListActivityItem(1, ActivityFooterSrl, 0, 
                b => { if (!b) ShareButtonPressed(); }));
            ActivityFooterSrl.Count = 1;
            TriList.UShortInput[UIUshortJoin.PresentationStagingCaretMode].UShortValue = 0;
            ShareButtonSig.BoolValue = false;
        }

        /// <summary>
        /// Sets up the footer SRL for when the room is on
        /// </summary>
        void SetupActivityFooterWhenRoomOn()
        {
            ActivityFooterSrl.Clear();
            ActivityFooterSrl.AddItem(new SubpageReferenceListActivityItem(1, ActivityFooterSrl,
                0, null));
            ActivityFooterSrl.AddItem(new SubpageReferenceListActivityItem(2, ActivityFooterSrl,
                4, b => { if (!b) PowerButtonPressed(); }));
            ActivityFooterSrl.Count = 2;
            TriList.UShortInput[UIUshortJoin.PresentationStagingCaretMode].UShortValue = 1;
            EndMeetingButtonSig = ActivityFooterSrl.BoolInputSig(2, 1);
            ShareButtonSig.BoolValue = CurrentRoom.OnFeedback.BoolValue;
        }

        /// <summary>
        /// Attached to activity list share button
        /// </summary>
        void ShareButtonPressed()
        {
            ShareButtonSig.BoolValue = true;
            TriList.BooleanInput[StartPageVisibleJoin].BoolValue = false;
            TriList.BooleanInput[UIBoolJoin.SourceStagingBarVisible].BoolValue = true;
            TriList.BooleanInput[UIBoolJoin.SelectASourceVisible].BoolValue = true;
            // Run default source when room is off and share is pressed
            if (!CurrentRoom.OnFeedback.BoolValue)
                (CurrentRoom as IRunDefaultPresentRoute).RunDefaultPresentRoute();
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
					//pm = new SetTopBoxMediumPageManager(uiDev as ISetTopBoxControls, TriList);
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
			CurrentRoom.RunRouteAction(key);
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
            EndMeetingButtonSig.BoolValue = true;
            ShareButtonSig.BoolValue = false;

            if (CurrentRoom.ShutdownType == eShutdownType.Manual || CurrentRoom.ShutdownType == eShutdownType.Vacancy)
            {
                PowerDownModal = new ModalDialog(TriList);
                var message = string.Format("Meeting will end in {0} seconds", CurrentRoom.ShutdownPromptSeconds);

                // Attach timer things to modal
                CurrentRoom.ShutdownPromptTimer.TimeRemainingFeedback.OutputChange += ShutdownPromptTimer_TimeRemainingFeedback_OutputChange;
                CurrentRoom.ShutdownPromptTimer.PercentFeedback.OutputChange += ShutdownPromptTimer_PercentFeedback_OutputChange;
               
                // respond to offs by cancelling dialog
                var onFb = CurrentRoom.OnFeedback;
                EventHandler<FeedbackEventArgs> offHandler = null;
                offHandler = (o, a) =>
                {
                    if (!onFb.BoolValue)
                    {
                        EndMeetingButtonSig.BoolValue = false;
                        PowerDownModal.HideDialog();
                        onFb.OutputChange -= offHandler;
                        //gauge.OutputChange -= gaugeHandler;
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
            EndMeetingButtonSig.BoolValue = false;
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
            EndMeetingButtonSig.BoolValue = false;
            ShareButtonSig.BoolValue = CurrentRoom.OnFeedback.BoolValue;

            CurrentRoom.ShutdownPromptTimer.TimeRemainingFeedback.OutputChange += ShutdownPromptTimer_TimeRemainingFeedback_OutputChange;
            CurrentRoom.ShutdownPromptTimer.PercentFeedback.OutputChange -= ShutdownPromptTimer_PercentFeedback_OutputChange;
        }

        void ShutdownPromptTimer_TimeRemainingFeedback_OutputChange(object sender, EventArgs e)
        {

            var message = string.Format("Meeting will end in {0} seconds", (sender as StringFeedback).StringValue);
            TriList.StringInput[ModalDialog.MessageTextJoin].StringValue = message;
        }

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
        public void RefreshCurrentRoom(IEssentialsHuddleSpaceRoom room)
        {
            if (_CurrentRoom != null)
            {
                // Disconnect current room
                _CurrentRoom.CurrentVolumeDeviceChange -= this.CurrentRoom_CurrentAudioDeviceChange;
                ClearAudioDeviceConnections();
                _CurrentRoom.CurrentSourceChange -= this.CurrentRoom_SourceInfoChange;
                DisconnectSource(_CurrentRoom.CurrentSourceInfo);
                _CurrentRoom.ShutdownPromptTimer.HasStarted -= ShutdownPromptTimer_HasStarted;
                _CurrentRoom.ShutdownPromptTimer.HasFinished -= ShutdownPromptTimer_HasFinished;
                _CurrentRoom.ShutdownPromptTimer.WasCancelled -= ShutdownPromptTimer_WasCancelled;

                _CurrentRoom.OnFeedback.OutputChange -= CurrentRoom_OnFeedback_OutputChange;
                _CurrentRoom.IsWarmingUpFeedback.OutputChange -= CurrentRoom_IsWarmingFeedback_OutputChange;
                _CurrentRoom.IsCoolingDownFeedback.OutputChange -= IsCoolingDownFeedback_OutputChange;
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

                        var actualSource = DeviceManager.GetDeviceForKey(srcConfig.SourceKey) as Device;
                        if (actualSource == null)
                        {
                            Debug.Console(1, "Cannot assign missing source '{0}' to source UI list",
                                srcConfig.SourceKey);
                            continue;
                        }
                        var routeKey = kvp.Key;
                        var item = new SubpageReferenceListSourceItem(i++, SourcesSrl, srcConfig,
                            b => { if (!b) UiSelectSource(routeKey); });
                        SourcesSrl.AddItem(item); // add to the SRL
                        item.RegisterForSourceChange(_CurrentRoom);
                    }
                    SourcesSrl.Count = (ushort)(i - 1);
                }
                // Name and logo
                TriList.StringInput[UIStringJoin.CurrentRoomName].StringValue = _CurrentRoom.Name;
                if (_CurrentRoom.LogoUrlLightBkgnd == null)
                {
                    TriList.BooleanInput[UIBoolJoin.LogoDefaultVisible].BoolValue = true;
                    TriList.BooleanInput[UIBoolJoin.LogoUrlVisible].BoolValue = false;
                }
                else
                {
                    TriList.BooleanInput[UIBoolJoin.LogoDefaultVisible].BoolValue = false;
                    TriList.BooleanInput[UIBoolJoin.LogoUrlVisible].BoolValue = true;
                    TriList.StringInput[UIStringJoin.LogoUrlLightBkgnd].StringValue = _CurrentRoom.LogoUrlLightBkgnd;
                    TriList.StringInput[UIStringJoin.LogoUrlLightBkgnd].StringValue = _CurrentRoom.LogoUrlDarkBkgnd;

                }

                // Shutdown timer
                _CurrentRoom.ShutdownPromptTimer.HasStarted += ShutdownPromptTimer_HasStarted;
                _CurrentRoom.ShutdownPromptTimer.HasFinished += ShutdownPromptTimer_HasFinished;
                _CurrentRoom.ShutdownPromptTimer.WasCancelled += ShutdownPromptTimer_WasCancelled;

                // Link up all the change events from the room
                _CurrentRoom.OnFeedback.OutputChange += CurrentRoom_OnFeedback_OutputChange;
                CurrentRoom_SyncOnFeedback();
                _CurrentRoom.IsWarmingUpFeedback.OutputChange += CurrentRoom_IsWarmingFeedback_OutputChange;
                _CurrentRoom.IsCoolingDownFeedback.OutputChange += IsCoolingDownFeedback_OutputChange;

                _CurrentRoom.CurrentVolumeDeviceChange += CurrentRoom_CurrentAudioDeviceChange;
                RefreshAudioDeviceConnections();
                _CurrentRoom.CurrentSourceChange += CurrentRoom_SourceInfoChange;
                RefreshSourceInfo();

                (Parent as EssentialsPanelMainInterfaceDriver).HeaderDriver.SetupHeaderButtons(this, CurrentRoom);
            }
            else
            {
                // Clear sigs that need to be
                TriList.StringInput[UIStringJoin.CurrentRoomName].StringValue = "Select a room";
            }
        }

		void SetCurrentRoom(IEssentialsHuddleSpaceRoom room)
		{
			if (_CurrentRoom == room) return;
            // Disconnect current (probably never called)

            if (_CurrentRoom != null)
                _CurrentRoom.ConfigChanged -= room_ConfigChanged;

            room.ConfigChanged -= room_ConfigChanged;
            room.ConfigChanged += room_ConfigChanged;

            if (room.IsMobileControlEnabled)
            {
                StartPageVisibleJoin = UIBoolJoin.StartMCPageVisible;
                UpdateMCJoins(room);

                if (_CurrentRoom != null)
                    _CurrentRoom.MobileControlRoomBridge.UserCodeChanged -= MobileControlRoomBridge_UserCodeChanged;

                room.MobileControlRoomBridge.UserCodeChanged -= MobileControlRoomBridge_UserCodeChanged;
                room.MobileControlRoomBridge.UserCodeChanged += MobileControlRoomBridge_UserCodeChanged;
            }
            else
            {
                StartPageVisibleJoin = UIBoolJoin.StartPageVisible;
            }

            RefreshCurrentRoom(room);
		}

        void MobileControlRoomBridge_UserCodeChanged(object sender, EventArgs e)
        {
            UpdateMCJoins(_CurrentRoom);
        }

        void UpdateMCJoins(IEssentialsHuddleSpaceRoom room)
        {
            TriList.SetString(UIStringJoin.RoomMcUrl, room.MobileControlRoomBridge.McServerUrl);
            TriList.SetString(UIStringJoin.RoomMcQrCodeUrl, room.MobileControlRoomBridge.QrCodeUrl);
            TriList.SetString(UIStringJoin.RoomUserCode, room.MobileControlRoomBridge.UserCode);
        }

        /// <summary>
        /// Fires when room config of current room has changed.  Meant to refresh room values to propegate any updates to UI
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void room_ConfigChanged(object sender, EventArgs e)
        {
            RefreshCurrentRoom(_CurrentRoom);
        }

        /// <summary>
        /// For room on/off changes
        /// </summary>
        void CurrentRoom_OnFeedback_OutputChange(object sender, EventArgs e)
        {
            CurrentRoom_SyncOnFeedback();
        }

        void CurrentRoom_SyncOnFeedback()
        {
            var value = _CurrentRoom.OnFeedback.BoolValue;
            //Debug.Console(2, CurrentRoom, "UI: Is on event={0}", value);
            TriList.BooleanInput[UIBoolJoin.RoomIsOn].BoolValue = value;

            if (value) //ON
            {
                SetupActivityFooterWhenRoomOn();
                TriList.BooleanInput[UIBoolJoin.SelectASourceVisible].BoolValue = false;
                TriList.BooleanInput[UIBoolJoin.SourceStagingBarVisible].BoolValue = true;
                TriList.BooleanInput[StartPageVisibleJoin].BoolValue = false;
                TriList.BooleanInput[UIBoolJoin.VolumeSingleMute1Visible].BoolValue = true;

            }
            else
            {
                SetupActivityFooterWhenRoomOff();
                ShowLogo();
                TriList.BooleanInput[StartPageVisibleJoin].BoolValue = true;
                TriList.BooleanInput[UIBoolJoin.VolumeSingleMute1Visible].BoolValue = false;
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
                ShowNotificationRibbon("Room is powering on. Please wait...", 0);
            }
            else
            {
                ShowNotificationRibbon("Room is powered on. Welcome.", 2000);
            }
        }


        void IsCoolingDownFeedback_OutputChange(object sender, EventArgs e)
        {
            if (CurrentRoom.IsCoolingDownFeedback.BoolValue)
            {
                ShowNotificationRibbon("Room is powering off. Please wait.", 0);
            }
            else
            {
                HideNotificationRibbon();
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
            if (previousDev is IHasPowerControl)
                (previousDev as IHasPowerControl).UnlinkButtons(TriList);
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
            if (dev is IHasPowerControl)
                (dev as IHasPowerControl).LinkButtons(TriList);
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
		void CurrentRoom_SourceInfoChange(SourceListItem info, ChangeType change)
		{
			if (change == ChangeType.WillChange)
				DisconnectSource(info);
			else
				RefreshSourceInfo();
		}
	}
}