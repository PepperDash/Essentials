using System;
using System.Collections.Generic;
using System.Linq;
using Crestron.SimplSharp;
using Crestron.SimplSharpPro;
using PepperDash.Core;
using PepperDash.Essentials;
using PepperDash.Essentials.Core;
using PepperDash.Essentials.Core.Config;
using PepperDash.Essentials.Core.Devices.Codec;
using PepperDash.Essentials.Core.PageManagers;
using PepperDash.Essentials.Core.Touchpanels.Keyboards;
using PepperDash.Essentials.UIDrivers;
using PepperDash.Essentials.UIDrivers.VC;

namespace PepperDashEssentials.UIDrivers.EssentialsDualDisplay
{
    public class EssentialsDualDisplayPanelAvFunctionsDriver : PanelDriverBase, IAVWithVCDriver
    {
        #region UiDisplayMode enum

        public enum UiDisplayMode
        {
            Presentation,
            AudioSetup,
            Call,
            Start
        }

        #endregion

        private readonly SubpageReferenceList _activityFooterSrl;
        private readonly BoolInputSig _callButtonSig;
        private readonly CrestronTouchpanelPropertiesConfig _config;

        private readonly List<BoolInputSig> _currentDisplayModeSigsInUse = new List<BoolInputSig>();
        private readonly BoolInputSig _endMeetingButtonSig;
        private readonly Dictionary<object, PageManager> _pageManagers = new Dictionary<object, PageManager>();
        private readonly PanelDriverBase _parent;
        private readonly BoolInputSig _shareButtonSig;

        private readonly SubpageReferenceList _sourceStagingSrl;

        private BoolFeedback _callSharingInfoVisibleFeedback;
        private List<PanelDriverBase> _childDrivers = new List<PanelDriverBase>();
        private UiDisplayMode _currentMode = UiDisplayMode.Start;
        private EssentialsDualDisplayRoom _currentRoom;
        private PageManager _currentSourcePageManager;
        private string _lastMeetingDismissedId;
        private CTimer _nextMeetingTimer;
        private ModalDialog _powerDownModal;
        private CTimer _powerOffTimer;
        private CTimer _ribbonTimer;
        private EssentialsHuddleTechPageDriver _techDriver;
        private EssentialsVideoCodecUiDriver _vcDriver;

        public EssentialsDualDisplayPanelAvFunctionsDriver(PanelDriverBase parent,
            CrestronTouchpanelPropertiesConfig config) : base(parent.TriList)
        {
            _config = config;
            _parent = parent;

            _sourceStagingSrl = new SubpageReferenceList(TriList, UISmartObjectJoin.SourceStagingSRL, 3, 3, 3);
            _activityFooterSrl = new SubpageReferenceList(TriList, UISmartObjectJoin.ActivityFooterSRL, 3, 3, 3);
            _callButtonSig = _activityFooterSrl.BoolInputSig(2, 1);
            _shareButtonSig = _activityFooterSrl.BoolInputSig(1, 1);
            _endMeetingButtonSig = _activityFooterSrl.BoolInputSig(3, 1);

            MeetingOrContactMethodModalSrl = new SubpageReferenceList(TriList, UISmartObjectJoin.MeetingListSRL, 3, 3, 3);

            SetupActivityFooterWhenRoomOff();

            ShowVolumeGauge = true;
            Keyboard = new HabaneroKeyboardController(TriList);
        }

        public bool ShowVolumeGauge { get; set; }
        public uint PowerOffTimeout { get; set; }
        public string DefaultRoomKey { get; set; }

        #region IAVWithVCDriver Members

        public EssentialsRoomBase CurrentRoom
        {
            get { return _currentRoom; }
            set { SetCurrentRoom(value as EssentialsDualDisplayRoom); }
        }

        public SubpageReferenceList MeetingOrContactMethodModalSrl { get; set; }
        public JoinedSigInterlock PopupInterlock { get; private set; }

        public void ShowNotificationRibbon(string message, int timeout)
        {
            TriList.SetString(UIStringJoin.NotificationRibbonText, message);
            TriList.SetBool(UIBoolJoin.NotificationRibbonVisible, true);
            if (timeout <= 0)
            {
                return;
            }

            if (_ribbonTimer != null)
            {
                _ribbonTimer.Stop();
            }
            _ribbonTimer = new CTimer(o =>
            {
                TriList.SetBool(UIBoolJoin.NotificationRibbonVisible, false);
                _ribbonTimer = null;
            }, timeout);
        }

                private void SetupActivityFooterWhenRoomOff()
        {
            _activityFooterSrl.Clear();
            _activityFooterSrl.AddItem(new SubpageReferenceListActivityItem(1, _activityFooterSrl, 0,
                b =>
                {
                    if (!b)
                    {
                        ActivityShareButtonPressed();
                    }
                }));
            _activityFooterSrl.AddItem(new SubpageReferenceListActivityItem(2, _activityFooterSrl, 3,
                b =>
                {
                    if (!b)
                    {
                        ActivityCallButtonPressed();
                    }
                }));
            _activityFooterSrl.Count = 2;
            TriList.SetUshort(UIUshortJoin.PresentationStagingCaretMode, 1); // right one slot
            TriList.SetUshort(UIUshortJoin.CallStagingCaretMode, 5); // left one slot
        }

        public void HideNotificationRibbon()
        {
            TriList.SetBool(UIBoolJoin.NotificationRibbonVisible, false);
            if (_ribbonTimer == null)
            {
                return;
            }
            _ribbonTimer.Stop();
            _ribbonTimer = null;
        }

        public void ShowTech()
        {
            PopupInterlock.HideAndClear();
            _techDriver.Show();
        }

        public HabaneroKeyboardController Keyboard { get; private set; }

        public void ActivityCallButtonPressed()
        {
            if (_vcDriver.IsVisible)
            {
                return;
            }
            HideLogo();
            HideNextMeetingPopup();
            TriList.SetBool(UIBoolJoin.StartPageVisible, false);
            TriList.SetBool(UIBoolJoin.SourceStagingBarVisible, false);
            TriList.SetBool(UIBoolJoin.SelectASourceVisible, false);
            if (_currentSourcePageManager != null)
            {
                _currentSourcePageManager.Hide();
            }
            PowerOnFromCall();
            _currentMode = UiDisplayMode.Call;
            SetActivityFooterFeedbacks();
            _vcDriver.Show();
        }

        public void PrepareForCodecIncomingCall()
        {
            if (_powerDownModal != null && _powerDownModal.ModalIsVisible)
            {
                _powerDownModal.CancelDialog();
            }
            PopupInterlock.Hide();
        }

        #endregion

        private void HideLogo()
        {
            TriList.SetBool(UIBoolJoin.LogoDefaultVisible, false);
            TriList.SetBool(UIBoolJoin.LogoUrlVisible, false);
        }

        private void HideNextMeetingPopup()
        {
            TriList.SetBool(UIBoolJoin.NextMeetingModalVisible, false);
        }

        private void PowerOnFromCall()
        {
            if (!CurrentRoom.OnFeedback.BoolValue)
            {
                _currentRoom.RunDefaultCallRoute();
            }
        }

        private void SetActivityFooterFeedbacks()
        {
            _callButtonSig.BoolValue = _currentMode == UiDisplayMode.Call
                                       && CurrentRoom.ShutdownType == eShutdownType.None;
            _shareButtonSig.BoolValue = _currentMode == UiDisplayMode.Presentation
                                        && CurrentRoom.ShutdownType == eShutdownType.None;
            _endMeetingButtonSig.BoolValue = CurrentRoom.ShutdownType != eShutdownType.None;
        }
        
        private void UiSelectSource(string key)
        {
            // Run the route and when it calls back, show the source
            CurrentRoom.RunRouteAction(key, () => { });
        }

        private void SetupSourceList()
        {
            var inCall = _currentRoom.InCallFeedback.BoolValue;
            var config = ConfigReader.ConfigObject.SourceLists;
            if (config.ContainsKey(_currentRoom.SourceListKey))
            {
                var srcList = config[_currentRoom.SourceListKey].OrderBy(kv => kv.Value.Order);

                // Setup sources list			
                _sourceStagingSrl.Clear();
                uint i = 1; // counter for UI list
                foreach (var kvp in srcList)
                {
                    var srcConfig = kvp.Value;
                    Debug.Console(1, "**** {0}, {1}, {2}, {3}, {4}", srcConfig.PreferredName,
                        srcConfig.IncludeInSourceList,
                        srcConfig.DisableCodecSharing, inCall, _currentMode);
                    // Skip sources marked as not included, and filter list of non-sharable sources when in call
                    // or on share screen
                    if (!srcConfig.IncludeInSourceList || (inCall && srcConfig.DisableCodecSharing)
                        || _currentMode == UiDisplayMode.Call && srcConfig.DisableCodecSharing)
                    {
                        Debug.Console(1, "Skipping {0}", srcConfig.PreferredName);
                        continue;
                    }

                    var routeKey = kvp.Key;
                    var item = new SubpageReferenceListSourceItem(i++, _sourceStagingSrl, srcConfig,
                        b =>
                        {
                            if (!b)
                            {
                                UiSelectSource(routeKey);
                            }
                        });
                    _sourceStagingSrl.AddItem(item); // add to the SRL
                    item.RegisterForSourceChange(_currentRoom);
                }
                _sourceStagingSrl.Count = (ushort) (i - 1);
            }
        }

        private void ActivityShareButtonPressed()
        {
            SetupSourceList();
            if (_vcDriver.IsVisible)
            {
                _vcDriver.Hide();
            }
            HideNextMeetingPopup();
            TriList.SetBool(UIBoolJoin.StartPageVisible, false);
            TriList.SetBool(UIBoolJoin.CallStagingBarVisible, false);
            TriList.SetBool(UIBoolJoin.SourceStagingBarVisible, true);
            // Run default source when room is off and share is pressed
            if (!CurrentRoom.OnFeedback.BoolValue)
            {
                if (!CurrentRoom.OnFeedback.BoolValue)
                {
                    // If there's no default, show UI elements
                    if (!CurrentRoom.RunDefaultPresentRoute())
                    {
                        TriList.SetBool(UIBoolJoin.SelectASourceVisible, true);
                    }
                }
            }
            else // room is on show what's active or select a source if nothing is yet active
            {
                if (CurrentRoom.CurrentSourceInfo == null ||
                    CurrentRoom.CurrentSourceInfoKey == EssentialsHuddleVtc1Room.DefaultCodecRouteString)
                {
                    TriList.SetBool(UIBoolJoin.SelectASourceVisible, true);
                }
                else if (_currentSourcePageManager != null)
                {
                    _currentSourcePageManager.Show();
                }
            }
            _currentMode = UiDisplayMode.Presentation;
            SetupSourceList();
            SetActivityFooterFeedbacks();
        }

        private void SetupActivityFooterWhenRoomOn()
        {
            _activityFooterSrl.Clear();
            _activityFooterSrl.AddItem(new SubpageReferenceListActivityItem(1, _activityFooterSrl, 0,
                b =>
                {
                    if (!b)
                    {
                        ActivityShareButtonPressed();
                    }
                }));
            _activityFooterSrl.AddItem(new SubpageReferenceListActivityItem(2, _activityFooterSrl, 3,
                b =>
                {
                    if (!b)
                    {
                        ActivityCallButtonPressed();
                    }
                }));
            _activityFooterSrl.AddItem(new SubpageReferenceListActivityItem(3, _activityFooterSrl, 4,
                b =>
                {
                    if (!b)
                    {
                        EndMeetingPress();
                    }
                }));
            _activityFooterSrl.Count = 3;
            TriList.SetUshort(UIUshortJoin.PresentationStagingCaretMode, 2); // center
            TriList.SetUshort(UIUshortJoin.CallStagingCaretMode, 0); // left -2
        }

        public void EndMeetingPress()
        {
            if (!CurrentRoom.OnFeedback.BoolValue
                || CurrentRoom.ShutdownPromptTimer.IsRunningFeedback.BoolValue)
            {
                return;
            }

            CurrentRoom.StartShutdown(eShutdownType.Manual);
        }

        private void SetCurrentRoom(EssentialsDualDisplayRoom room)
        {
            if (_currentRoom == room || room == null)
            {
                return;
            }
            // Disconnect current (probably never called)

            room.ConfigChanged -= room_ConfigChanged;
            room.ConfigChanged += room_ConfigChanged;

            RefreshCurrentRoom(room);
        }

        private void room_ConfigChanged(object sender, EventArgs e)
        {
            RefreshCurrentRoom(_currentRoom);
        }
        private void CurrentRoom_CurrentAudioDeviceChange(object sender, VolumeDeviceChangeEventArgs args)
        {
            if (args.Type == ChangeType.WillChange)
            {
                ClearAudioDeviceConnections();
            }
            else // did change
            {
                RefreshAudioDeviceConnections();
            }
        }
        public void VolumeUpPress(bool state)
        {
            if (CurrentRoom.CurrentVolumeControls != null)
            {
                CurrentRoom.CurrentVolumeControls.VolumeUp(state);
            }
        }

        public void VolumeDownPress(bool state)
        {
            if (CurrentRoom.CurrentVolumeControls != null)
            {
                CurrentRoom.CurrentVolumeControls.VolumeDown(state);
            }
        }

        private void RefreshAudioDeviceConnections()
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
            {
                TriList.UShortInput[UIUshortJoin.VolumeSlider1Value].UShortValue = 0;
            }
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

        private void ClearAudioDeviceConnections()
        {
            TriList.ClearBoolSigAction(UIBoolJoin.VolumeUpPress);
            TriList.ClearBoolSigAction(UIBoolJoin.VolumeDownPress);
            TriList.ClearBoolSigAction(UIBoolJoin.Volume1ProgramMutePressAndFB);

            var fDev = CurrentRoom.CurrentVolumeControls as IBasicVolumeWithFeedback;
            if (fDev == null)
            {
                return;
            }
            TriList.ClearUShortSigAction(UIUshortJoin.VolumeSlider1Value);
            fDev.VolumeLevelFeedback.UnlinkInputSig(
                TriList.UShortInput[UIUshortJoin.VolumeSlider1Value]);
        }

        private void CurrentRoom_SourceInfoChange(SourceListItem info, ChangeType change)
        {
            if (change == ChangeType.WillChange)
            {
                DisconnectSource(info);
            }
            else
            {
                RefreshSourceInfo();
            }
        }

        private void DisconnectSource(SourceListItem previousInfo)
        {
            if (previousInfo == null)
            {
                return;
            }

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
            {
                (previousDev as ISetTopBoxControls).UnlinkButtons(TriList);
            }
            // common interfaces
            if (previousDev is IChannel)
            {
                (previousDev as IChannel).UnlinkButtons(TriList);
            }
            if (previousDev is IColor)
            {
                (previousDev as IColor).UnlinkButtons(TriList);
            }
            if (previousDev is IDPad)
            {
                (previousDev as IDPad).UnlinkButtons(TriList);
            }
            if (previousDev is IDvr)
            {
                (previousDev as IDvr).UnlinkButtons(TriList);
            }
            if (previousDev is INumericKeypad)
            {
                (previousDev as INumericKeypad).UnlinkButtons(TriList);
            }
            if (previousDev is IPower)
            {
                (previousDev as IPower).UnlinkButtons(TriList);
            }
            if (previousDev is ITransport)
            {
                (previousDev as ITransport).UnlinkButtons(TriList);
            }
        }

        private void ShowCurrentSource()
        {
            if (CurrentRoom.CurrentSourceInfo == null)
            {
                return;
            }

            if (CurrentRoom.CurrentSourceInfo.SourceDevice == null)
            {
                TriList.SetBool(UIBoolJoin.SelectASourceVisible, true);
                return;
            }

            var uiDev = CurrentRoom.CurrentSourceInfo.SourceDevice as IUiDisplayInfo;
            // If we need a page manager, get an appropriate one
            if (uiDev == null)
            {
                return;
            }

            TriList.SetBool(UIBoolJoin.SelectASourceVisible, false);
            // Got an existing page manager, get it
            PageManager pm;
            if (_pageManagers.ContainsKey(uiDev))
            {
                pm = _pageManagers[uiDev];
            }
            // Otherwise make an apporiate one
            else if (uiDev is ISetTopBoxControls)
            {
                pm = new SetTopBoxThreePanelPageManager(uiDev as ISetTopBoxControls, TriList);
            }
            else if (uiDev is IDiscPlayerControls)
            {
                pm = new DiscPlayerMediumPageManager(uiDev as IDiscPlayerControls, TriList);
            }
            else
            {
                pm = new DefaultPageManager(uiDev, TriList);
            }
            _pageManagers[uiDev] = pm;
            _currentSourcePageManager = pm;
            pm.Show();
        }

        private void RefreshSourceInfo()
        {
            var routeInfo = CurrentRoom.CurrentSourceInfo;
            // This will show off popup too
            if (IsVisible && !_vcDriver.IsVisible)
            {
                ShowCurrentSource();
            }

            if (routeInfo == null) // || !CurrentRoom.OnFeedback.BoolValue)
            {
                // Check for power off and insert "Room is off"
                TriList.StringInput[UIStringJoin.CurrentSourceName].StringValue = "Room is off";
                TriList.StringInput[UIStringJoin.CurrentSourceIcon].StringValue = "Power";
                Hide();
                _parent.Show();
                return;
            }
            TriList.StringInput[UIStringJoin.CurrentSourceName].StringValue = routeInfo.PreferredName;
            TriList.StringInput[UIStringJoin.CurrentSourceIcon].StringValue = routeInfo.Icon; // defaults to "blank"

            //code that was here was unreachable becuase if we get past the if statement, routeInfo is

            // Connect controls
            if (routeInfo.SourceDevice != null)
            {
                ConnectControlDeviceMethods(routeInfo.SourceDevice);
            }
        }

        private void ConnectControlDeviceMethods(Device dev)
        {
            if (dev is ISetTopBoxControls)
            {
                (dev as ISetTopBoxControls).LinkButtons(TriList);
            }
            if (dev is IChannel)
            {
                (dev as IChannel).LinkButtons(TriList);
            }
            if (dev is IColor)
            {
                (dev as IColor).LinkButtons(TriList);
            }
            if (dev is IDPad)
            {
                (dev as IDPad).LinkButtons(TriList);
            }
            if (dev is IDvr)
            {
                (dev as IDvr).LinkButtons(TriList);
            }
            if (dev is INumericKeypad)
            {
                (dev as INumericKeypad).LinkButtons(TriList);
            }
            if (dev is IPower)
            {
                (dev as IPower).LinkButtons(TriList);
            }
            if (dev is ITransport)
            {
                (dev as ITransport).LinkButtons(TriList);
            }
        }

        private void ShutdownPromptTimer_TimeRemainingFeedback_OutputChange(object sender, EventArgs e)
        {
            var stringFeedback = sender as StringFeedback;
            if (stringFeedback == null)
            {
                return;
            }
            var message = string.Format("Meeting will end in {0} seconds", stringFeedback.StringValue);
            TriList.StringInput[ModalDialog.MessageTextJoin].StringValue = message;
        }

        /// <summary>
        /// Event handler for percentage on power off countdown
        /// </summary>
        private void ShutdownPromptTimer_PercentFeedback_OutputChange(object sender, EventArgs e)
        {
            var intFeedback = sender as IntFeedback;
            if (intFeedback == null)
            {
                return;
            }
            var value = (ushort)(intFeedback.UShortValue * 65535 / 100);
            TriList.UShortInput[ModalDialog.TimerGaugeJoin].UShortValue = value;
        }

        private void RefreshCurrentRoom(EssentialsDualDisplayRoom room)
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
                _currentRoom.IsCoolingDownFeedback.OutputChange -= CurrentRoom_IsCoolingDownFeedback_OutputChange;
                _currentRoom.InCallFeedback.OutputChange -= CurrentRoom_InCallFeedback_OutputChange;
            }

            _currentRoom = room;

            if (_currentRoom != null)
            {
                // get the source list config and set up the source list

                SetupSourceList();

                // Name and logo
                TriList.StringInput[UIStringJoin.CurrentRoomName].StringValue = _currentRoom.Name;
                ShowLogo();

                // Shutdown timer
                _currentRoom.ShutdownPromptTimer.HasStarted += ShutdownPromptTimer_HasStarted;
                _currentRoom.ShutdownPromptTimer.HasFinished += ShutdownPromptTimer_HasFinished;
                _currentRoom.ShutdownPromptTimer.WasCancelled += ShutdownPromptTimer_WasCancelled;

                // Link up all the change events from the room
                _currentRoom.OnFeedback.OutputChange += CurrentRoom_OnFeedback_OutputChange;
                CurrentRoom_SyncOnFeedback();
                _currentRoom.IsWarmingUpFeedback.OutputChange += CurrentRoom_IsWarmingFeedback_OutputChange;
                _currentRoom.IsCoolingDownFeedback.OutputChange += CurrentRoom_IsCoolingDownFeedback_OutputChange;
                _currentRoom.InCallFeedback.OutputChange += CurrentRoom_InCallFeedback_OutputChange;


                _currentRoom.CurrentVolumeDeviceChange += CurrentRoom_CurrentAudioDeviceChange;
                RefreshAudioDeviceConnections();
                _currentRoom.CurrentSourceChange += CurrentRoom_SourceInfoChange;
                RefreshSourceInfo();

                if (_currentRoom.VideoCodec is IHasScheduleAwareness)
                {
                    (_currentRoom.VideoCodec as IHasScheduleAwareness).CodecSchedule.MeetingsListHasChanged +=
                        CodecSchedule_MeetingsListHasChanged;
                }

                _callSharingInfoVisibleFeedback =
                    new BoolFeedback(() => _currentRoom.VideoCodec.SharingContentIsOnFeedback.BoolValue);
                _currentRoom.VideoCodec.SharingContentIsOnFeedback.OutputChange +=
                    SharingContentIsOnFeedback_OutputChange;
                _callSharingInfoVisibleFeedback.LinkInputSig(
                    TriList.BooleanInput[UIBoolJoin.CallSharedSourceInfoVisible]);

                SetActiveCallListSharingContentStatus();

                if (_currentRoom != null)
                {
                    _currentRoom.CurrentSourceChange +=
                        CurrentRoom_CurrentSingleSourceChange;
                }

                TriList.SetSigFalseAction(UIBoolJoin.CallStopSharingPress,
                    () => _currentRoom.RunRouteAction("codecOsd", _currentRoom.SourceListKey));

                var essentialsPanelMainInterfaceDriver = _parent as EssentialsPanelMainInterfaceDriver;
                if (essentialsPanelMainInterfaceDriver != null)
                {
                    essentialsPanelMainInterfaceDriver.HeaderDriver.SetupHeaderButtons(this, _currentRoom);
                }
            }
            else
            {
                // Clear sigs that need to be
                TriList.StringInput[UIStringJoin.CurrentRoomName].StringValue = "Select a room";
            }
        }
    }
}