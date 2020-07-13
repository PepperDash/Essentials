using System;
using System.Collections.Generic;
using Crestron.SimplSharp;
using Crestron.SimplSharpPro;
using PepperDash.Core;
using PepperDash.Essentials.Core;
using PepperDash.Essentials.Core.Config;
using PepperDash.Essentials.Core.PageManagers;
using PepperDash.Essentials.UIDrivers;

namespace PepperDash.Essentials
{
	public class EssentialsHuddlePanelAvFunctionsDriver : PanelDriverBase, IAVDriver
	{
	    private readonly CrestronTouchpanelPropertiesConfig _config;

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
			get { return _volumeButtonsPopupFeedback.TimeoutMs; }
			set { _volumeButtonsPopupFeedback.TimeoutMs = value; }
		}
		
		/// <summary>
		/// The amount of time that the volume gauge stays on screen, in ms
		/// </summary>
		public uint VolumeGaugePopupTimeout
		{
			get { return _volumeGaugeFeedback.TimeoutMs; }
			set { _volumeGaugeFeedback.TimeoutMs = value; }
		}

		/// <summary>
		/// 
		/// </summary>
		public uint PowerOffTimeout { get; set; }

	    /// <summary>
	    /// 
	    /// </summary>
	    public string DefaultRoomKey { get; set; }

	    /// <summary>
        /// Indicates that the SetHeaderButtons method has completed successfully
        /// </summary>
        public bool HeaderButtonsAreSetUp { get; private set; }

		/// <summary>
		/// 
		/// </summary>
		public EssentialsHuddleSpaceRoom CurrentRoom
		{
			get { return _currentRoom; }
			set
			{
				SetCurrentRoom(value);
			}
		}
		EssentialsHuddleSpaceRoom _currentRoom;

        /// <summary>
        /// 
        /// </summary>
        //uint CurrentInterlockedModalJoin;

        /// <summary>
        /// For hitting feedback
        /// </summary>
        private readonly BoolInputSig _shareButtonSig;
        private BoolInputSig _endMeetingButtonSig;

		/// <summary>
		/// Controls the extended period that the volume gauge shows on-screen,
		/// as triggered by Volume up/down operations
		/// </summary>
		private readonly BoolFeedbackPulseExtender _volumeGaugeFeedback;

		/// <summary>
		/// Controls the period that the volume buttons show on non-hard-button
		/// interfaces
		/// </summary>
		private readonly BoolFeedbackPulseExtender _volumeButtonsPopupFeedback;

        /// <summary>
        /// The parent driver for this
        /// </summary>
        private readonly PanelDriverBase _parent;

        /// <summary>
        /// All children attached to this driver.  For hiding and showing as a group.
        /// </summary>
        private List<PanelDriverBase> _childDrivers = new List<PanelDriverBase>();

	    private readonly List<BoolInputSig> _currentDisplayModeSigsInUse = new List<BoolInputSig>();

		//// Important smart objects

		/// <summary>
		/// Smart Object 3200
		/// </summary>
		private readonly SubpageReferenceList _sourcesSrl;

        /// <summary>
        /// Smart Object 15022
        /// </summary>
        private readonly SubpageReferenceList _activityFooterSrl;

		/// <summary>
		/// Tracks which audio page group the UI is in
		/// </summary>
		private UiDisplayMode _currentDisplayMode;

		/// <summary>
		/// The AV page mangagers that have been used, to keep them alive for later
		/// </summary>
		private readonly Dictionary<object, PageManager> _pageManagers = new Dictionary<object, PageManager>();

		/// <summary>
		/// Current page manager running for a source
		/// </summary>
		private PageManager _currentSourcePageManager;

		/// <summary>
		/// Will auto-timeout a power off
		/// </summary>
		private CTimer _powerOffTimer;

        private ModalDialog _powerDownModal;

        public JoinedSigInterlock PopupInterlock { get; private set; }

        /// <summary>
        /// The driver for the tech page. Lazy getter for memory usage
        /// </summary>
        EssentialsHuddleTechPageDriver TechDriver
        {
            get {
                return _techDriver ??
                       (_techDriver = new EssentialsHuddleTechPageDriver(TriList, CurrentRoom.PropertiesConfig.Tech));
            }
        }
        private EssentialsHuddleTechPageDriver _techDriver;


        /// <summary>
        /// Controls timeout of notification ribbon timer
        /// </summary>
        private CTimer _ribbonTimer;

		/// <summary>
		/// Constructor
		/// </summary>
		public EssentialsHuddlePanelAvFunctionsDriver(PanelDriverBase parent, CrestronTouchpanelPropertiesConfig config) 
			: base(parent.TriList)
		{
			_config = config;
            _parent = parent;
            PopupInterlock = new JoinedSigInterlock(TriList);

            _sourcesSrl = new SubpageReferenceList(TriList, 3200, 3, 3, 3);
            _activityFooterSrl = new SubpageReferenceList(TriList, 15022, 3, 3, 3);
            _shareButtonSig = _activityFooterSrl.BoolInputSig(1, 1);

            SetupActivityFooterWhenRoomOff();

			ShowVolumeGauge = true;

			// One-second pulse extender for volume gauge
			_volumeGaugeFeedback = new BoolFeedbackPulseExtender(1500);
			_volumeGaugeFeedback.Feedback
				.LinkInputSig(TriList.BooleanInput[UIBoolJoin.VolumeGaugePopupVisible]);

			_volumeButtonsPopupFeedback = new BoolFeedbackPulseExtender(4000);
			_volumeButtonsPopupFeedback.Feedback
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

		    switch (_config.HeaderStyle.ToLower())
		    {
		        case CrestronTouchpanelPropertiesConfig.Habanero:
		            TriList.SetSigFalseAction(UIBoolJoin.HeaderRoomButtonPress, () =>
		                PopupInterlock.ShowInterlockedWithToggle(UIBoolJoin.RoomHeaderPageVisible));
		            break;
		        case CrestronTouchpanelPropertiesConfig.Verbose:
		            break;
		    }

            TriList.SetBool(UIBoolJoin.DateAndTimeVisible, _config.ShowDate && _config.ShowTime);
            TriList.SetBool(UIBoolJoin.DateOnlyVisible, _config.ShowDate && !_config.ShowTime);
            TriList.SetBool(UIBoolJoin.TimeOnlyVisible, !_config.ShowDate && _config.ShowTime);

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
                TriList.SetBool(UIBoolJoin.StartPageVisible, true);
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
					if (CurrentRoom != null && CurrentRoom.DefaultDisplay is IPower)
						(CurrentRoom.DefaultDisplay as IPower).PowerToggle();
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
            if (CurrentRoom.LogoUrl == null)
            {
                TriList.SetBool(UIBoolJoin.LogoDefaultVisible, true);
                TriList.SetBool(UIBoolJoin.LogoUrlVisible, false);
            }
            else
            {
                TriList.SetBool(UIBoolJoin.LogoDefaultVisible, false);
                TriList.SetBool(UIBoolJoin.LogoUrlVisible, true);
                TriList.SetString(UIStringJoin.LogoUrl, _currentRoom.LogoUrl);
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
            TriList.BooleanInput[UIBoolJoin.StartPageVisible].BoolValue = false;
            TriList.BooleanInput[UIBoolJoin.TapToBeginVisible].BoolValue = false;
            TriList.BooleanInput[UIBoolJoin.SelectASourceVisible].BoolValue = false;
            //TriList.BooleanInput[UIBoolJoin.StagingPageVisible].BoolValue = false;
			_volumeButtonsPopupFeedback.ClearNow();
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
                if (_ribbonTimer != null)
                    _ribbonTimer.Stop();
                _ribbonTimer = new CTimer(o =>
                {
                    TriList.SetBool(UIBoolJoin.NotificationRibbonVisible, false);
                    _ribbonTimer = null;
                }, timeout);
            }
        }

        /// <summary>
        /// Hides the notification ribbon
        /// </summary>
        public void HideNotificationRibbon()
        {
            TriList.SetBool(UIBoolJoin.NotificationRibbonVisible, false);
            if (_ribbonTimer != null)
            {
                _ribbonTimer.Stop();
                _ribbonTimer = null;
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
			_currentDisplayMode = mode;
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
                        TriList.BooleanInput[UIBoolJoin.StartPageVisible].BoolValue = true;
                        TriList.BooleanInput[UIBoolJoin.TapToBeginVisible].BoolValue = true;
                        TriList.BooleanInput[UIBoolJoin.SelectASourceVisible].BoolValue = false;
                    }
                    // Date/time
					if (_config.ShowDate && _config.ShowTime)
					{
						TriList.BooleanInput[UIBoolJoin.DateAndTimeVisible].BoolValue = true;
						TriList.BooleanInput[UIBoolJoin.DateOnlyVisible].BoolValue = false;
						TriList.BooleanInput[UIBoolJoin.TimeOnlyVisible].BoolValue = false;
					}
					else
					{
						TriList.BooleanInput[UIBoolJoin.DateAndTimeVisible].BoolValue = false;
						TriList.BooleanInput[UIBoolJoin.DateOnlyVisible].BoolValue = _config.ShowDate;
						TriList.BooleanInput[UIBoolJoin.TimeOnlyVisible].BoolValue = _config.ShowTime;
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
            _activityFooterSrl.Clear();
            _activityFooterSrl.AddItem(new SubpageReferenceListActivityItem(1, _activityFooterSrl, 0, 
                b => { if (!b) ShareButtonPressed(); }));
            _activityFooterSrl.Count = 1;
            TriList.UShortInput[UIUshortJoin.PresentationStagingCaretMode].UShortValue = 0;
            _shareButtonSig.BoolValue = false;
        }

        /// <summary>
        /// Sets up the footer SRL for when the room is on
        /// </summary>
        void SetupActivityFooterWhenRoomOn()
        {
            _activityFooterSrl.Clear();
            _activityFooterSrl.AddItem(new SubpageReferenceListActivityItem(1, _activityFooterSrl,
                0, null));
            _activityFooterSrl.AddItem(new SubpageReferenceListActivityItem(2, _activityFooterSrl,
                4, b => { if (!b) PowerButtonPressed(); }));
            _activityFooterSrl.Count = 2;
            TriList.UShortInput[UIUshortJoin.PresentationStagingCaretMode].UShortValue = 1;
            _endMeetingButtonSig = _activityFooterSrl.BoolInputSig(2, 1);
            _shareButtonSig.BoolValue = CurrentRoom.OnFeedback.BoolValue;
        }

        /// <summary>
        /// Attached to activity list share button
        /// </summary>
        void ShareButtonPressed()
        {
            _shareButtonSig.BoolValue = true;
            TriList.BooleanInput[UIBoolJoin.StartPageVisible].BoolValue = false;
            TriList.BooleanInput[UIBoolJoin.SourceStagingBarVisible].BoolValue = true;
            TriList.BooleanInput[UIBoolJoin.SelectASourceVisible].BoolValue = true;
            // Run default source when room is off and share is pressed
            if (!CurrentRoom.OnFeedback.BoolValue)
                CurrentRoom.RunDefaultPresentRoute();
        }


		/// <summary>
		/// Shows all sigs that are in CurrentDisplayModeSigsInUse
		/// </summary>
		void ShowCurrentDisplayModeSigsInUse()
		{
			foreach (var sig in _currentDisplayModeSigsInUse)
				sig.BoolValue = true;
		}

		/// <summary>
		/// Hides all CurrentDisplayModeSigsInUse sigs and clears the array
		/// </summary>
		void HideAndClearCurrentDisplayModeSigsInUse()
		{
			foreach (var sig in _currentDisplayModeSigsInUse)
				sig.BoolValue = false;
			_currentDisplayModeSigsInUse.Clear();
		}

		/// <summary>
		/// Send the UI back depending on location, not used in huddle UI
		/// </summary>
		public override void BackButtonPressed()
		{
			switch (_currentDisplayMode)
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
			_parent.Show();
		}

		/// <summary>
		/// Loads the appropriate Sigs into CurrentDisplayModeSigsInUse and shows them
		/// </summary>
		void ShowCurrentSource()
		{
			if (CurrentRoom.CurrentSourceInfo == null)
				return;

			var uiDev = CurrentRoom.CurrentSourceInfo.SourceDevice as IUiDisplayInfo;
		    // If we need a page manager, get an appropriate one
		    if (uiDev == null)
		    {
		        return;
		    }

		    TriList.BooleanInput[UIBoolJoin.SelectASourceVisible].BoolValue = false;
		    // Got an existing page manager, get it
		    PageManager pm;
		    if (_pageManagers.ContainsKey(uiDev))
		        pm = _pageManagers[uiDev];
		        // Otherwise make an apporiate one
		    else if (uiDev is ISetTopBoxControls)
		        //pm = new SetTopBoxMediumPageManager(uiDev as ISetTopBoxControls, TriList);
		        pm = new SetTopBoxThreePanelPageManager(uiDev as ISetTopBoxControls, TriList);
		    else if (uiDev is IDiscPlayerControls)
		        pm = new DiscPlayerMediumPageManager(uiDev as IDiscPlayerControls, TriList);
		    else
		        pm = new DefaultPageManager(uiDev, TriList);
		    _pageManagers[uiDev] = pm;
		    _currentSourcePageManager = pm;
		    pm.Show();
		}

		/// <summary>
		/// Called from button presses on source, where We can assume we want
		/// to change to the proper screen.
		/// </summary>
		/// <param name="key">The key name of the route to run</param>
		void UiSelectSource(string key)
		{
			// Run the route and when it calls back, show the source
			CurrentRoom.RunRouteAction(key, () => { });
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
            _endMeetingButtonSig.BoolValue = true;
            _shareButtonSig.BoolValue = false;

            if (CurrentRoom.ShutdownType == eShutdownType.Manual || CurrentRoom.ShutdownType == eShutdownType.Vacancy)
            {
                _powerDownModal = new ModalDialog(TriList);
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
                        _endMeetingButtonSig.BoolValue = false;
                        _powerDownModal.HideDialog();
                        onFb.OutputChange -= offHandler;
                        //gauge.OutputChange -= gaugeHandler;
                    }
                };
                onFb.OutputChange += offHandler;

                _powerDownModal.PresentModalDialog(2, "End Meeting", "Power", message, "Cancel", "End Meeting Now", true, true,
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
            _endMeetingButtonSig.BoolValue = false;
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
            if (_powerDownModal != null)
                _powerDownModal.HideDialog();
            _endMeetingButtonSig.BoolValue = false;
            _shareButtonSig.BoolValue = CurrentRoom.OnFeedback.BoolValue;

            CurrentRoom.ShutdownPromptTimer.TimeRemainingFeedback.OutputChange += ShutdownPromptTimer_TimeRemainingFeedback_OutputChange;
            CurrentRoom.ShutdownPromptTimer.PercentFeedback.OutputChange -= ShutdownPromptTimer_PercentFeedback_OutputChange;
        }

        void ShutdownPromptTimer_TimeRemainingFeedback_OutputChange(object sender, EventArgs e)
        {
            var stringFeedback = sender as StringFeedback;
            if (stringFeedback == null)
            {
                return;
            }

            var message = string.Format("Meeting will end in {0} seconds", stringFeedback.StringValue);
            TriList.StringInput[ModalDialog.MessageTextJoin].StringValue = message;
        }

	    void ShutdownPromptTimer_PercentFeedback_OutputChange(object sender, EventArgs e)
	    {
	        var intFeedback = sender as IntFeedback;
	        if (intFeedback == null)
	        {
	            return;
	        }
	        var value = (ushort)(intFeedback.UShortValue * 65535 / 100);
	        TriList.UShortInput[ModalDialog.TimerGaugeJoin].UShortValue = value;
	    }

	    /// <summary>
        /// 
        /// </summary>
		void CancelPowerOffTimer()
		{
	        if (_powerOffTimer == null)
	        {
	            return;
	        }
	        _powerOffTimer.Stop();
	        _powerOffTimer = null;
		}

		/// <summary>
		/// 
		/// </summary>
		void VolumeButtonsTogglePress()
		{
			if (_volumeButtonsPopupFeedback.BoolValue)
				_volumeButtonsPopupFeedback.ClearNow();
			else
			{
				// Trigger the popup
				_volumeButtonsPopupFeedback.BoolValue = true;
				_volumeButtonsPopupFeedback.BoolValue = false;
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
				_volumeGaugeFeedback.BoolValue = state;		
			_volumeButtonsPopupFeedback.BoolValue = state;
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
				_volumeGaugeFeedback.BoolValue = state;
			_volumeButtonsPopupFeedback.BoolValue = state; 
			if (CurrentRoom.CurrentVolumeControls != null)
				CurrentRoom.CurrentVolumeControls.VolumeDown(state);
		}


        /// <summary>
        /// Helper for property setter. Sets the panel to the given room, latching up all functionality
        /// </summary>
        public void RefreshCurrentRoom(EssentialsHuddleSpaceRoom room)
        {
            if (_currentRoom != null)
            {
                // Disconnect current room
                _currentRoom.CurrentVolumeDeviceChange -= CurrentRoom_CurrentAudioDeviceChange;
                ClearAudioDeviceConnections();
                _currentRoom.CurrentSourceChange -= CurrentRoom_SourceInfoChange;
                DisconnectSource(_currentRoom.CurrentSourceInfo);
                _currentRoom.ShutdownPromptTimer.HasStarted -= ShutdownPromptTimer_HasStarted;
                _currentRoom.ShutdownPromptTimer.HasFinished -= ShutdownPromptTimer_HasFinished;
                _currentRoom.ShutdownPromptTimer.WasCancelled -= ShutdownPromptTimer_WasCancelled;

                _currentRoom.OnFeedback.OutputChange -= CurrentRoom_OnFeedback_OutputChange;
                _currentRoom.IsWarmingUpFeedback.OutputChange -= CurrentRoom_IsWarmingFeedback_OutputChange;
                _currentRoom.IsCoolingDownFeedback.OutputChange -= IsCoolingDownFeedback_OutputChange;
            }

            _currentRoom = room;

            if (_currentRoom != null)
            {
                // get the source list config and set up the source list
                var config = ConfigReader.ConfigObject.SourceLists;
                if (config.ContainsKey(_currentRoom.SourceListKey))
                {
                    var srcList = config[_currentRoom.SourceListKey];
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
                        var item = new SubpageReferenceListSourceItem(i++, _sourcesSrl, srcConfig,
                            b => { if (!b) UiSelectSource(routeKey); });
                        _sourcesSrl.AddItem(item); // add to the SRL
                        item.RegisterForSourceChange(_currentRoom);
                    }
                    _sourcesSrl.Count = (ushort)(i - 1);
                }
                // Name and logo
                TriList.StringInput[UIStringJoin.CurrentRoomName].StringValue = _currentRoom.Name;
                if (_currentRoom.LogoUrl == null)
                {
                    TriList.BooleanInput[UIBoolJoin.LogoDefaultVisible].BoolValue = true;
                    TriList.BooleanInput[UIBoolJoin.LogoUrlVisible].BoolValue = false;
                }
                else
                {
                    TriList.BooleanInput[UIBoolJoin.LogoDefaultVisible].BoolValue = false;
                    TriList.BooleanInput[UIBoolJoin.LogoUrlVisible].BoolValue = true;
                    TriList.StringInput[UIStringJoin.LogoUrl].StringValue = _currentRoom.LogoUrl;
                }

                // Shutdown timer
                _currentRoom.ShutdownPromptTimer.HasStarted += ShutdownPromptTimer_HasStarted;
                _currentRoom.ShutdownPromptTimer.HasFinished += ShutdownPromptTimer_HasFinished;
                _currentRoom.ShutdownPromptTimer.WasCancelled += ShutdownPromptTimer_WasCancelled;

                // Link up all the change events from the room
                _currentRoom.OnFeedback.OutputChange += CurrentRoom_OnFeedback_OutputChange;
                CurrentRoom_SyncOnFeedback();
                _currentRoom.IsWarmingUpFeedback.OutputChange += CurrentRoom_IsWarmingFeedback_OutputChange;
                _currentRoom.IsCoolingDownFeedback.OutputChange += IsCoolingDownFeedback_OutputChange;

                _currentRoom.CurrentVolumeDeviceChange += CurrentRoom_CurrentAudioDeviceChange;
                RefreshAudioDeviceConnections();
                _currentRoom.CurrentSourceChange += CurrentRoom_SourceInfoChange;
                RefreshSourceInfo();

                var essentialsPanelMainInterfaceDriver = _parent as EssentialsPanelMainInterfaceDriver;

                if (essentialsPanelMainInterfaceDriver != null)
                {
                    essentialsPanelMainInterfaceDriver.HeaderDriver.SetupHeaderButtons(this, CurrentRoom);
                }
            }
            else
            {
                // Clear sigs that need to be
                TriList.StringInput[UIStringJoin.CurrentRoomName].StringValue = "Select a room";
            }
        }

		void SetCurrentRoom(EssentialsHuddleSpaceRoom room)
		{
			if (_currentRoom == room) return;
            // Disconnect current (probably never called)

            room.ConfigChanged -= room_ConfigChanged;
            room.ConfigChanged += room_ConfigChanged;

            RefreshCurrentRoom(room);
		}

        /// <summary>
        /// Fires when room config of current room has changed.  Meant to refresh room values to propegate any updates to UI
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void room_ConfigChanged(object sender, EventArgs e)
        {
            RefreshCurrentRoom(_currentRoom);
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
            var value = _currentRoom.OnFeedback.BoolValue;
            //Debug.Console(2, CurrentRoom, "UI: Is on event={0}", value);
            TriList.BooleanInput[UIBoolJoin.RoomIsOn].BoolValue = value;

            if (value) //ON
            {
                SetupActivityFooterWhenRoomOn();
                TriList.BooleanInput[UIBoolJoin.SelectASourceVisible].BoolValue = false;
                TriList.BooleanInput[UIBoolJoin.SourceStagingBarVisible].BoolValue = true;
                TriList.BooleanInput[UIBoolJoin.StartPageVisible].BoolValue = false;
                TriList.BooleanInput[UIBoolJoin.VolumeSingleMute1Visible].BoolValue = true;

            }
            else
            {
                SetupActivityFooterWhenRoomOff();
                ShowLogo();
                TriList.BooleanInput[UIBoolJoin.StartPageVisible].BoolValue = true;
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
				if (_currentSourcePageManager != null)
				{
					_currentSourcePageManager.Hide();
					_currentSourcePageManager = null;
				}
			}

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
			if (IsVisible)
				ShowCurrentSource();

			if (routeInfo == null)// || !CurrentRoom.OnFeedback.BoolValue)
			{
				// Check for power off and insert "Room is off"
				TriList.StringInput[UIStringJoin.CurrentSourceName].StringValue = "Room is off";
				TriList.StringInput[UIStringJoin.CurrentSourceIcon].StringValue = "Power";
				Hide();
				_parent.Show();
				return;
			}

		    if (CurrentRoom.CurrentSourceInfo != null)
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