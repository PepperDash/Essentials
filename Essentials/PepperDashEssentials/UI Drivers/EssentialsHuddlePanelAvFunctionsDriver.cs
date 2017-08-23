using System;
using System.Collections.Generic;
using Crestron.SimplSharp;
using Crestron.SimplSharpPro;

using PepperDash.Core;
using PepperDash.Essentials.Core;
using PepperDash.Essentials.Core.SmartObjects;
using PepperDash.Essentials.Core.PageManagers;

namespace PepperDash.Essentials
{
	/// <summary>
	/// 
	/// </summary>
	public class EssentialsHuddlePanelAvFunctionsDriver : PanelDriverBase
	{
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
				CurrentRoom = DeviceManager.GetDeviceForKey(value) as EssentialsHuddleSpaceRoom;
			}	
		}
		string _DefaultRoomKey;

		/// <summary>
		/// 
		/// </summary>
		public EssentialsHuddleSpaceRoom CurrentRoom
		{
			get { return _CurrentRoom; }
			set
			{
				SetCurrentRoom(value);
			}
		}
		EssentialsHuddleSpaceRoom _CurrentRoom;

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

        ModalDialog WarmingCoolingModal;

		/// <summary>
		/// Constructor
		/// </summary>
		public EssentialsHuddlePanelAvFunctionsDriver(PanelDriverBase parent, CrestronTouchpanelPropertiesConfig config) 
			: base(parent.TriList)
		{
			Config = config;
            Parent = parent;

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

            TriList.BooleanInput[UIBoolJoin.LogoDefaultVisible].BoolValue = true;
		}

		/// <summary>
		/// 
		/// </summary>
		public override void Show()
		{
			TriList.BooleanInput[UIBoolJoin.TopBarVisible].BoolValue = true;
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
                    as EssentialsHuddleSpaceRoom;
                if (room != null)
                    message = room.Config.HelpMessage;
                else
                    message = "Sorry, no help message available. No room connected.";
                TriList.StringInput[UIStringJoin.HelpMessage].StringValue = message;
                ShowInterlockedModal(UIBoolJoin.HelpPageVisible);
            });

            //TriList.SetSigFalseAction(UIBoolJoin.RoomHeaderButtonPress, () =>
            //    ShowInterlockedModal(UIBoolJoin.RoomHeaderPageVisible));

#warning Add press and hold to gear button here
#warning Hide Gear on ipad for now
            TriList.BooleanInput[UIBoolJoin.GearButtonVisible].BoolValue = true;
            TriList.SetSigFalseAction(UIBoolJoin.GearHeaderButtonPress, () =>
                ShowInterlockedModal(UIBoolJoin.TechPanelSetupVisible));
                //ShowInterlockedModal(UIBoolJoin.VolumesPageVisible));
            TriList.SetSigFalseAction(UIBoolJoin.TechPagesExitButton, () =>
                HideCurrentInterlockedModal());

			// power-related functions
            // Note: some of these are not directly-related to the huddle space UI, but are held over
            // in case
			TriList.SetSigFalseAction(UIBoolJoin.ShowPowerOffPress, PowerButtonPressed);
			TriList.SetSigFalseAction(UIBoolJoin.PowerOffMorePress, () =>
				{
					CancelPowerOffTimer();
					TriList.BooleanInput[UIBoolJoin.PowerOffStep1Visible].BoolValue = false;
					TriList.BooleanInput[UIBoolJoin.PowerOffStep2Visible].BoolValue = true;
				});
			TriList.SetSigFalseAction(UIBoolJoin.DisplayPowerTogglePress, () =>
				{ 
					if (CurrentRoom != null && CurrentRoom.DefaultDisplay is IPower)
						(CurrentRoom.DefaultDisplay as IPower).PowerToggle();
				});

			base.Show();
		}

        /// <summary>
        /// Handler for room on/off feedback
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        //void OnFeedback_OutputChange(object sender, EventArgs e)
        //{

        //}

		public override void Hide()
		{
			HideAndClearCurrentDisplayModeSigsInUse();
			TriList.BooleanInput[UIBoolJoin.TopBarVisible].BoolValue = false;
            TriList.BooleanInput[UIBoolJoin.ActivityFooterVisible].BoolValue = false;
            TriList.BooleanInput[UIBoolJoin.StartPageVisible].BoolValue = false;
            TriList.BooleanInput[UIBoolJoin.TapToBeginVisible].BoolValue = false;
            TriList.BooleanInput[UIBoolJoin.SelectASourceVisible].BoolValue = false;
            //TriList.BooleanInput[UIBoolJoin.StagingPageVisible].BoolValue = false;
			VolumeButtonsPopupFeedback.ClearNow();
            //CancelPowerOff();

			base.Hide();
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
                        TriList.BooleanInput[UIBoolJoin.StagingPageVisible].BoolValue = true;
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
            TriList.UShortInput[UIUshortJoin.PresentationListCaretMode].UShortValue = 0;
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
                3, b => { if (!b) PowerButtonPressed(); }));
            ActivityFooterSrl.Count = 2;
            TriList.UShortInput[UIUshortJoin.PresentationListCaretMode].UShortValue = 1;
            EndMeetingButtonSig = ActivityFooterSrl.BoolInputSig(2, 1);
        }

        /// <summary>
        /// Attached to activity list share button
        /// </summary>
        void ShareButtonPressed()
        {
            //if (!_CurrentRoom.OnFeedback.BoolValue)
            //{
                ShareButtonSig.BoolValue = true;
                TriList.BooleanInput[UIBoolJoin.StartPageVisible].BoolValue = false;
                TriList.BooleanInput[UIBoolJoin.StagingPageVisible].BoolValue = true;
                TriList.BooleanInput[UIBoolJoin.SelectASourceVisible].BoolValue = true;
            //}
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
			CurrentRoom.RunRouteAction(key, null);
		}

		/// <summary>
		/// 
		/// </summary>
		public void PowerButtonPressed()
		{
            var room = CurrentRoom;
			if (!room.OnFeedback.BoolValue || room.ShutdownPromptTimer.IsRunningFeedback.BoolValue) 
                return;

            CurrentRoom.StartShutdown(ShutdownType.Manual);
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

            if (CurrentRoom.ShutdownType == ShutdownType.Manual)
            {
                var modal = new ModalDialog(TriList);
                var message = string.Format("Meeting will end in {0} seconds", CurrentRoom.ShutdownPromptSeconds);

                // figure out a cleaner way to update gauge
                var gauge = CurrentRoom.ShutdownPromptTimer.PercentFeedback;
                EventHandler<EventArgs> gaugeHandler = null;
                gaugeHandler = (o, a) => TriList.UShortInput[ModalDialog.TimerGaugeJoin].UShortValue =
                    (ushort)(gauge.UShortValue * 65535 / 100);
                gauge.OutputChange += gaugeHandler;

                // respond to offs by cancelling dialog
                var onFb = CurrentRoom.OnFeedback;
                EventHandler<EventArgs> offHandler = null;
                offHandler = (o, a) =>
                {
                    if (!onFb.BoolValue)
                    {
                        EndMeetingButtonSig.BoolValue = false;
                        modal.HideDialog();
                        onFb.OutputChange -= offHandler;
                        gauge.OutputChange -= gaugeHandler;
                    }
                };
                onFb.OutputChange += offHandler;

                modal.PresentModalDialog(2, "End Meeting", "Power", message, "Cancel", "End Meeting Now", true, true,
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

            Debug.Console(2, "UI shutdown prompt finished");
            EndMeetingButtonSig.BoolValue = false;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void ShutdownPromptTimer_WasCancelled(object sender, EventArgs e)
        {
            Debug.Console(2, "UI shutdown prompt cancelled");
            ShareButtonSig.BoolValue = true; // restore Share fb
            EndMeetingButtonSig.BoolValue = false;
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
		/// Runs the power off function on the current room
		/// </summary>
        //public void FinishPowerOff()
        //{
        //    if (CurrentRoom == null)
        //        return;
        //    CurrentRoom.RunRouteAction("roomOff");
        //    CancelPowerOff();
        //}

		/// <summary>
		/// Hides power off pages and stops timer
		/// </summary>
        //void CancelPowerOff()
        //{
        //    CancelPowerOffTimer();
        //    TriList.BooleanInput[UIBoolJoin.PowerOffStep1Visible].BoolValue = false;
        //    TriList.BooleanInput[UIBoolJoin.PowerOffStep2Visible].BoolValue = false;
        //}

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
		void SetCurrentRoom(EssentialsHuddleSpaceRoom room)
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
							Debug.Console(0, "Cannot assign missing source '{0}' to source UI list",
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

				TriList.StringInput[UIStringJoin.CurrentRoomName].StringValue = _CurrentRoom.Name;

                // Shutdown timer
                _CurrentRoom.ShutdownPromptTimer.HasStarted += ShutdownPromptTimer_HasStarted;
                _CurrentRoom.ShutdownPromptTimer.HasFinished += ShutdownPromptTimer_HasFinished;
                _CurrentRoom.ShutdownPromptTimer.WasCancelled += ShutdownPromptTimer_WasCancelled;

                // Link up all the change events from the room
                _CurrentRoom.OnFeedback.OutputChange += CurrentRoom_OnFeedback_OutputChange;
                _CurrentRoom.IsWarmingUpFeedback.OutputChange += CurrentRoom_IsWarmingFeedback_OutputChange;
                _CurrentRoom.IsCoolingDownFeedback.OutputChange += IsCoolingDownFeedback_OutputChange;

				_CurrentRoom.CurrentVolumeDeviceChange += CurrentRoom_CurrentAudioDeviceChange;
				RefreshAudioDeviceConnections();
				_CurrentRoom.CurrentSingleSourceChange += CurrentRoom_SourceInfoChange;
				RefreshSourceInfo();
			}
			else
			{
				// Clear sigs that need to be
				TriList.StringInput[UIStringJoin.CurrentRoomName].StringValue = "Select a room";
			}
		}

        /// <summary>
        /// For room on/off changes
        /// </summary>
        void CurrentRoom_OnFeedback_OutputChange(object sender, EventArgs e)
        {
            var value = _CurrentRoom.OnFeedback.BoolValue;
            Debug.Console(2, CurrentRoom, "UI: Is on event={0}", value);
            TriList.BooleanInput[UIBoolJoin.RoomIsOn].BoolValue = value;
            if (value) //ON
            {
                SetupActivityFooterWhenRoomOn();
                TriList.BooleanInput[UIBoolJoin.StartPageVisible].BoolValue = false;
                TriList.BooleanInput[UIBoolJoin.VolumeSingleMute1Visible].BoolValue = true;
            }
            else
            {
                SetupActivityFooterWhenRoomOff();
                TriList.BooleanInput[UIBoolJoin.StartPageVisible].BoolValue = true;
                TriList.BooleanInput[UIBoolJoin.VolumeSingleMute1Visible].BoolValue = false;
                TriList.BooleanInput[UIBoolJoin.StagingPageVisible].BoolValue = false;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        void CurrentRoom_IsWarmingFeedback_OutputChange(object sender, EventArgs e)
        {
            var value = CurrentRoom.IsWarmingUpFeedback.BoolValue;
            Debug.Console(2, CurrentRoom, "UI: WARMING event={0}", value);

            if (value)
            {
                WarmingCoolingModal = new ModalDialog(TriList);
                WarmingCoolingModal.PresentModalDialog(0, "Powering up", "Power", "Room is warming up.  Please wait.",
                    "", "", false, false, null);
            }
            else
            {
                if (WarmingCoolingModal != null)
                    WarmingCoolingModal.CancelDialog();
            }
        }


        void IsCoolingDownFeedback_OutputChange(object sender, EventArgs e)
        {
            var value = CurrentRoom.IsCoolingDownFeedback.BoolValue;
            Debug.Console(2, CurrentRoom, "UI: Cooldown event={0}", value);

            if (value)
            {
                WarmingCoolingModal = new ModalDialog(TriList);
                WarmingCoolingModal.PresentModalDialog(0, "Shutting down", "Power", "Room is shutting down.  Please wait.",
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
		void CurrentRoom_SourceInfoChange(EssentialsRoomBase room, 
			SourceListItem info, ChangeType change)
		{
			if (change == ChangeType.WillChange)
				DisconnectSource(info);
			else
				RefreshSourceInfo();
		}
	}
}