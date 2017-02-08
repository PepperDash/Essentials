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
	public class EssentialsPresentationPanelAvFunctionsDriver : PanelDriverBase
	{
		CrestronTouchpanelPropertiesConfig Config;

		public enum UiDisplayMode
		{
			AudioSetup, AudioCallMode, PresentationMode
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
		/// Controls the extended period that the volume gauge shows on-screen,
		/// as triggered by Volume up/down operations
		/// </summary>
		BoolFeedbackPulseExtender VolumeGaugeFeedback;

		/// <summary>
		/// Controls the period that the volume buttons show on non-hard-button
		/// interfaces
		/// </summary>
		BoolFeedbackPulseExtender VolumeButtonsPopupFeedback;

		PanelDriverBase Parent;

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

		/// <summary>
		/// Constructor
		/// </summary>
        public EssentialsPresentationPanelAvFunctionsDriver(PanelDriverBase parent, CrestronTouchpanelPropertiesConfig config) 
			: base(parent.TriList)
		{
			Config = config;
			Parent = parent;

            SourcesSrl = new SubpageReferenceList(TriList, 3200, 3, 3, 3);
            ActivityFooterSrl = new SubpageReferenceList(TriList, 15022, 3, 3, 3);
            SetupActivityFooterWhenRoomOff();

			ShowVolumeGauge = true;

			// One-second pulse extender for volume gauge
			VolumeGaugeFeedback = new BoolFeedbackPulseExtender(1500);
			VolumeGaugeFeedback.Feedback
				.LinkInputSig(TriList.BooleanInput[UIBoolJoin.VolumeGaugePopupVisbible]);

			VolumeButtonsPopupFeedback = new BoolFeedbackPulseExtender(4000);
			VolumeButtonsPopupFeedback.Feedback
				.LinkInputSig(TriList.BooleanInput[UIBoolJoin.VolumeButtonPopupVisbible]);

			PowerOffTimeout = 30000;
		}

		/// <summary>
		/// 
		/// </summary>
		public override void Show()
		{
			// We'll want to show the current state of AV, but for now, just show rooms
			TriList.BooleanInput[UIBoolJoin.TopBarVisible].BoolValue = true;
            TriList.BooleanInput[UIBoolJoin.ActivityFooterVisible].BoolValue = true;

			// Default to showing rooms/sources now.
			ShowMode(UiDisplayMode.PresentationMode);

			// Attach actions
			TriList.SetSigFalseAction(UIBoolJoin.VolumeButtonPopupPress, VolumeButtonsTogglePress);

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
			TriList.SetSigFalseAction(UIBoolJoin.DisplayPowerTogglePress, () =>
				{ 
					if (CurrentRoom != null && CurrentRoom.DefaultDisplay is IPower)
						(CurrentRoom.DefaultDisplay as IPower).PowerToggle();
				});

			base.Show();
		}

		public override void Hide()
		{
			HideAndClearCurrentDisplayModeSigsInUse();
			TriList.BooleanInput[UIBoolJoin.TopBarVisible].BoolValue = false;
            TriList.BooleanInput[UIBoolJoin.ActivityFooterVisible].BoolValue = false;
			VolumeButtonsPopupFeedback.ClearNow();
			CancelPowerOff();

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
					CurrentDisplayModeSigsInUse.Add(TriList.BooleanInput[UIBoolJoin.StagingPageVisible]);
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
                b => { if (!b) ShowMode(UiDisplayMode.PresentationMode); }));
            ActivityFooterSrl.AddItem(new SubpageReferenceListActivityItem(2, ActivityFooterSrl, 0,
                b => { if (!b) ShowMode(UiDisplayMode.AudioCallMode); }));
            ActivityFooterSrl.Count = 2;
            TriList.UShortInput[UIUshortJoin.PresentationListCaretMode].UShortValue = 1;
        }

        /// <summary>
        /// Sets up the footer SRL for when the room is on
        /// </summary>
        void SetupActivityFooterWhenRoomOn()
        {
            ActivityFooterSrl.Clear();
            ActivityFooterSrl.AddItem(new SubpageReferenceListActivityItem(1, ActivityFooterSrl, 0,
                b => { if (!b) ShowMode(UiDisplayMode.PresentationMode); }));
            ActivityFooterSrl.AddItem(new SubpageReferenceListActivityItem(2, ActivityFooterSrl, 0,
                b => { if (!b) ShowMode(UiDisplayMode.AudioCallMode); }));
            ActivityFooterSrl.AddItem(new SubpageReferenceListActivityItem(3, ActivityFooterSrl,
                3, b => { if (!b) PowerButtonPressed(); }));
            ActivityFooterSrl.Count = 3;
            TriList.UShortInput[UIUshortJoin.PresentationListCaretMode].UShortValue = 2;
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
			{
                //var offPm = new DefaultPageManager(UIBoolJoin.SelectSourcePopupVisible, TriList);
                //PageManagers["OFF"] = offPm;
                //CurrentSourcePageManager = offPm;
                //offPm.Show();
				return;
			}

			var uiDev = CurrentRoom.CurrentSourceInfo.SourceDevice as IUiDisplayInfo;
			PageManager pm = null;
			// If we need a page manager, get an appropriate one
			if (uiDev != null)
			{
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
			else // show some default thing
			{
				CurrentDisplayModeSigsInUse.Add(TriList.BooleanInput[12345]);
			}

			ShowCurrentDisplayModeSigsInUse();
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
			if (!CurrentRoom.OnFeedback.BoolValue) 
                return;
            // Timeout or button 1 press will shut down
            var modal = new ModalDialog(TriList);
			uint time = 60000;
            uint seconds = time / 1000;
            var message = string.Format("Meeting will end in {0} seconds", seconds);
            modal.PresentModalTimerDialog(2, "End Meeting", "Info", message,
                "End Meeting Now", "Cancel", time, true,
				but => { if (but != 2) CurrentRoom.RunRouteAction("roomOff"); });
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
				_CurrentRoom.CurrentSourceInfoChange -= this._CurrentRoom_SourceInfoChange;
				DisconnectSource(_CurrentRoom.CurrentSourceInfo);
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
                        //Debug.Console(0, "Adding source '{0}'", srcConfig.SourceKey);
                        //var s = srcConfig; // assign locals for scope in button lambda
						var routeKey = kvp.Key;
                        var item = new SubpageReferenceListSourceItem(i++, SourcesSrl, srcConfig.PreferredName,
                            b => { if (!b) UiSelectSource(routeKey); });
                        SourcesSrl.AddItem(item); // add to the SRL 
					}
                    SourcesSrl.Count = (ushort)(i - 1);
				}

				TriList.StringInput[UIStringJoin.CurrentRoomName].StringValue = _CurrentRoom.Name;

                // Link up all the change events from the room
                _CurrentRoom.OnFeedback.OutputChange += _CurrentRoom_OnFeedback_OutputChange;
				_CurrentRoom.CurrentVolumeDeviceChange += _CurrentRoom_CurrentAudioDeviceChange;
				RefreshAudioDeviceConnections();
				_CurrentRoom.CurrentSourceInfoChange += _CurrentRoom_SourceInfoChange;
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
        void _CurrentRoom_OnFeedback_OutputChange(object sender, EventArgs e)
        {
            var value = _CurrentRoom.OnFeedback.BoolValue;
            TriList.BooleanInput[UIBoolJoin.RoomIsOn].BoolValue = value;
            if (value)
                SetupActivityFooterWhenRoomOn();
            else
                SetupActivityFooterWhenRoomOff();
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

			if (routeInfo == null || !CurrentRoom.OnFeedback.BoolValue)
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
		void _CurrentRoom_CurrentAudioDeviceChange(object sender, VolumeDeviceChangeEventArgs args)
		{
			if (args.Type == ChangeType.WillChange)
				ClearAudioDeviceConnections();
			else // did change
				RefreshAudioDeviceConnections();
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
	}
}