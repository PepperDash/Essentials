﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using Crestron.SimplSharp;
using Crestron.SimplSharpPro.CrestronThread;
using Crestron.SimplSharpPro.DeviceSupport;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

using PepperDash.Core;
using PepperDash.Essentials.Core;
using PepperDash.Essentials.Core.Bridges;
using PepperDash.Essentials.Core.Config;
using PepperDash.Essentials.Core.DeviceTypeInterfaces;
using PepperDash.Essentials.Core.Routing;
using PepperDash.Essentials.Devices.Common.Cameras;
using PepperDash.Essentials.Devices.Common.Codec;
using PepperDash.Essentials.Devices.Common.VideoCodec;
using PepperDash.Essentials.Core.Queues;

namespace PepperDash.Essentials.Devices.Common.VideoCodec.Cisco
{
    enum eCommandType { SessionStart, SessionEnd, Command, GetStatus, GetConfiguration };
	public enum eExternalSourceType {camera, desktop, document_camera, mediaplayer, PC, whiteboard, other}
	public enum eExternalSourceMode {Ready, NotReady, Hidden, Error}

    public class CiscoSparkCodec : VideoCodecBase, IHasCallHistory, IHasCallFavorites, IHasDirectory,
        IHasScheduleAwareness, IOccupancyStatusProvider, IHasCodecLayouts, IHasCodecSelfView,
        ICommunicationMonitor, IRouting, IHasCodecCameras, IHasCameraAutoMode, IHasCodecRoomPresets, 
        IHasExternalSourceSwitching, IHasBranding, IHasCameraOff, IHasCameraMute, IHasDoNotDisturbMode,
        IHasHalfWakeMode, IHasCallHold, IJoinCalls
    {
        private CiscoSparkCodecPropertiesConfig _config;

        private bool _externalSourceChangeRequested;

        public event EventHandler<DirectoryEventArgs> DirectoryResultReturned;

        private CTimer _brandingTimer;

        public CommunicationGather PortGather { get; private set; }

        public StatusMonitorBase CommunicationMonitor { get; private set; }

        private GenericQueue _receiveQueue;

		public BoolFeedback PresentationViewMaximizedFeedback { get; private set; }

		private string _currentPresentationView;

        public BoolFeedback RoomIsOccupiedFeedback { get; private set; }

        public IntFeedback PeopleCountFeedback { get; private set; }

        public BoolFeedback CameraAutoModeIsOnFeedback { get; private set; }

        public BoolFeedback SelfviewIsOnFeedback { get; private set; }

        public StringFeedback SelfviewPipPositionFeedback { get; private set; }

        public StringFeedback LocalLayoutFeedback { get; private set; }

        public BoolFeedback LocalLayoutIsProminentFeedback { get; private set; }

        public BoolFeedback FarEndIsSharingContentFeedback { get; private set; }

        public IntFeedback RingtoneVolumeFeedback { get; private set; }

        private CodecCommandWithLabel _currentSelfviewPipPosition;

        private CodecCommandWithLabel _currentLocalLayout;

        /// <summary>
        /// List the available positions for the selfview PIP window
        /// </summary>
        public List<CodecCommandWithLabel> SelfviewPipPositions = new List<CodecCommandWithLabel>()
        {
            new CodecCommandWithLabel("CenterLeft", "Center Left"),
            new CodecCommandWithLabel("CenterRight", "Center Right"),
            new CodecCommandWithLabel("LowerLeft", "Lower Left"),
            new CodecCommandWithLabel("LowerRight", "Lower Right"),
            new CodecCommandWithLabel("UpperCenter", "Upper Center"),
            new CodecCommandWithLabel("UpperLeft", "Upper Left"),
            new CodecCommandWithLabel("UpperRight", "Upper Right"),
        };

        /// <summary>
        /// Lists the available options for local layout
        /// </summary>
        public List<CodecCommandWithLabel> LocalLayouts = new List<CodecCommandWithLabel>()
        {
            //new CodecCommandWithLabel("auto", "Auto"),
            //new CiscoCodecLocalLayout("custom", "Custom"),    // Left out for now
            new CodecCommandWithLabel("equal","Equal"),
            new CodecCommandWithLabel("overlay","Overlay"),
            new CodecCommandWithLabel("prominent","Prominent"),
            new CodecCommandWithLabel("single","Single")
        };

        private CiscoCodecConfiguration.RootObject CodecConfiguration = new CiscoCodecConfiguration.RootObject();

        private CiscoCodecStatus.RootObject CodecStatus = new CiscoCodecStatus.RootObject();

        public CodecCallHistory CallHistory { get; private set; }

        public CodecCallFavorites CallFavorites { get; private set; }

        /// <summary>
        /// The root level of the directory
        /// </summary>
        public CodecDirectory DirectoryRoot { get; private set; }

        /// <summary>
        /// Represents the current state of the directory and is computed on get
        /// </summary>
        public CodecDirectory CurrentDirectoryResult
        {
            get
            {
                if (DirectoryBrowseHistory.Count > 0)
                    return DirectoryBrowseHistory[DirectoryBrowseHistory.Count - 1];
                else
                    return DirectoryRoot;
            }
        }

        public BoolFeedback CurrentDirectoryResultIsNotDirectoryRoot { get; private set; }

        /// <summary>
        /// Tracks the directory browse history when browsing beyond the root directory
        /// </summary>
        public List<CodecDirectory> DirectoryBrowseHistory { get; private set; }

        public CodecScheduleAwareness CodecSchedule { get; private set; }

        /// <summary>
        /// Gets and returns the scaled volume of the codec
        /// </summary>
        protected override Func<int> VolumeLevelFeedbackFunc
        {
            get
            {
                return () => CrestronEnvironment.ScaleWithLimits(CodecStatus.Status.Audio.Volume.IntValue, 100, 0, 65535, 0);
            }
        }

        protected override Func<bool> PrivacyModeIsOnFeedbackFunc
        {
            get
            {
                return () => CodecStatus.Status.Audio.Microphones.Mute.BoolValue;
            }
        }

        protected override Func<bool> StandbyIsOnFeedbackFunc
        {
            get
            {
                return () => CodecStatus.Status.Standby.State.BoolValue;
            }
        }

        /// <summary>
        /// Gets the value of the currently shared source, or returns null
        /// </summary>
        protected override Func<string> SharingSourceFeedbackFunc
        {
            get 
            {
                return () => _presentationSourceKey;
            }
        }

        protected override Func<bool> SharingContentIsOnFeedbackFunc
        {
            get 
            {
                return () => CodecStatus.Status.Conference.Presentation.Mode.BoolValue; 
            }
        }

        protected Func<bool> FarEndIsSharingContentFeedbackFunc
        {
            get
            {
                return () => CodecStatus.Status.Conference.Presentation.Mode.Value == "Receiving";
            }
        }

        protected override Func<bool> MuteFeedbackFunc
        {
            get 
            { 
                return () => CodecStatus.Status.Audio.VolumeMute.BoolValue; 
            }
        }

        protected Func<bool> RoomIsOccupiedFeedbackFunc
        {
            get
            {
                return () => CodecStatus.Status.RoomAnalytics.PeoplePresence.BoolValue;
            }
        }

        protected Func<int> PeopleCountFeedbackFunc
        {
            get
            {
                return () => CodecStatus.Status.RoomAnalytics.PeopleCount.Current.IntValue;
            }
        }

        protected Func<bool> SpeakerTrackIsOnFeedbackFunc
        {
            get
            {
                return () => CodecStatus.Status.Cameras.SpeakerTrack.Status.BoolValue;
            }
        }

        protected Func<bool> SelfViewIsOnFeedbackFunc
        {
            get
            {
                return () => CodecStatus.Status.Video.Selfview.Mode.BoolValue;
            }
        }

        protected Func<string> SelfviewPipPositionFeedbackFunc
        {
            get
            {               
                return () => _currentSelfviewPipPosition.Label;
            }
        }

        protected Func<string> LocalLayoutFeedbackFunc
        {
            get
            {
                return () => _currentLocalLayout.Label;
            }
        }

        protected Func<bool> LocalLayoutIsProminentFeedbackFunc
        {
            get
            {
                return () => _currentLocalLayout.Label == "Prominent";
            }
        }


        private string _cliFeedbackRegistrationExpression;

        private CodecSyncState _syncState;

        public CodecPhonebookSyncState PhonebookSyncState { get; private set; }

        private StringBuilder _jsonMessage;

        private bool _jsonFeedbackMessageIsIncoming;

        public bool CommDebuggingIsOn;

        string Delimiter = "\r\n";

        public IntFeedback PresentationSourceFeedback { get; private set; }

        public BoolFeedback PresentationSendingLocalOnlyFeedback { get; private set; }

        public BoolFeedback PresentationSendingLocalRemoteFeedback { get; private set; }

        /// <summary>
        /// Used to track the current connector used for the presentation source
        /// </summary>
        private int _presentationSource;

        /// <summary>
        /// Used to track the connector that is desired to be the current presentation source (until the command is send)
        /// </summary>
        private int _desiredPresentationSource;

        private string _presentationSourceKey;

        private bool _presentationLocalOnly;

        private bool _presentationLocalRemote;

        private string _phonebookMode = "Local"; // Default to Local

        private uint _phonebookResultsLimit = 255; // Could be set later by config.

        private CTimer _loginMessageReceivedTimer;
        private CTimer _retryConnectionTimer;

        // **___________________________________________________________________**
        //  Timers to be moved to the global system timer at a later point....
        private CTimer BookingsRefreshTimer;
        private CTimer PhonebookRefreshTimer;
        // **___________________________________________________________________**

        public RoutingInputPort CodecOsdIn { get; private set; }
        public RoutingInputPort HdmiIn2 { get; private set; }
        public RoutingInputPort HdmiIn3 { get; private set; }
        public RoutingOutputPort HdmiOut1 { get; private set; }
        public RoutingOutputPort HdmiOut2 { get; private set; }


        // Constructor for IBasicCommunication
        public CiscoSparkCodec(DeviceConfig config, IBasicCommunication comm)
            : base(config)
        {
            var props = JsonConvert.DeserializeObject<Codec.CiscoSparkCodecPropertiesConfig>(config.Properties.ToString());

            _config = props;

            // Use the configured phonebook results limit if present
            if (props.PhonebookResultsLimit > 0)
            {
                _phonebookResultsLimit = props.PhonebookResultsLimit;
            }

            // The queue that will collect the repsonses in the order they are received
            _receiveQueue = new GenericQueue(this.Key + "-rxQueue", 25);

            RoomIsOccupiedFeedback = new BoolFeedback(RoomIsOccupiedFeedbackFunc);
            PeopleCountFeedback = new IntFeedback(PeopleCountFeedbackFunc);
            CameraAutoModeIsOnFeedback = new BoolFeedback(SpeakerTrackIsOnFeedbackFunc);
            SelfviewIsOnFeedback = new BoolFeedback(SelfViewIsOnFeedbackFunc);
            SelfviewPipPositionFeedback = new StringFeedback(SelfviewPipPositionFeedbackFunc);
            LocalLayoutFeedback = new StringFeedback(LocalLayoutFeedbackFunc);
            LocalLayoutIsProminentFeedback = new BoolFeedback(LocalLayoutIsProminentFeedbackFunc);
			FarEndIsSharingContentFeedback = new BoolFeedback(FarEndIsSharingContentFeedbackFunc);
            CameraIsOffFeedback = new BoolFeedback(() => CodecStatus.Status.Video.Input.MainVideoMute.BoolValue);
            CameraIsMutedFeedback = CameraIsOffFeedback;
            SupportsCameraOff = true;

            DoNotDisturbModeIsOnFeedback = new BoolFeedback(() => CodecStatus.Status.Conference.DoNotDisturb.BoolValue);
            HalfWakeModeIsOnFeedback = new BoolFeedback(() => CodecStatus.Status.Standby.State.Value.ToLower() == "halfwake");
            EnteringStandbyModeFeedback = new BoolFeedback(() => CodecStatus.Status.Standby.State.Value.ToLower() == "enteringstandby");

			PresentationViewMaximizedFeedback = new BoolFeedback(() => _currentPresentationView == "Maximized");

            RingtoneVolumeFeedback = new IntFeedback(() => CodecConfiguration.Configuration.Audio.SoundsAndAlerts.RingVolume.Volume);

            PresentationSourceFeedback = new IntFeedback(() => _presentationSource);
            PresentationSendingLocalOnlyFeedback = new BoolFeedback(() => _presentationLocalOnly);
            PresentationSendingLocalRemoteFeedback = new BoolFeedback(() => _presentationLocalRemote);

            Communication = comm;

            if (props.CommunicationMonitorProperties != null)
            {
                CommunicationMonitor = new GenericCommunicationMonitor(this, Communication, props.CommunicationMonitorProperties);
            }
            else
            {
                var command = string.Format("xCommand Peripherals HeartBeat ID: {0}{1}", CrestronEthernetHelper.ETHERNET_PARAMETER_TO_GET.GET_MAC_ADDRESS, Delimiter);
                CommunicationMonitor = new GenericCommunicationMonitor(this, Communication, 30000, 120000, 300000, command);
            }

            if (props.Sharing != null)
                AutoShareContentWhileInCall = props.Sharing.AutoShareContentWhileInCall;

            ShowSelfViewByDefault = props.ShowSelfViewByDefault;

            DeviceManager.AddDevice(CommunicationMonitor);

            _phonebookMode = props.PhonebookMode;

            _syncState = new CodecSyncState(Key + "--Sync", this);

            PhonebookSyncState = new CodecPhonebookSyncState(Key + "--PhonebookSync");

            _syncState.InitialSyncCompleted += new EventHandler<EventArgs>(SyncState_InitialSyncCompleted);

            PortGather = new CommunicationGather(Communication, Delimiter);
            PortGather.IncludeDelimiter = true;
            PortGather.LineReceived += this.Port_LineReceived;

            CodecInfo = new CiscoCodecInfo(CodecStatus, CodecConfiguration);

            CallHistory = new CodecCallHistory();

			
            if (props.Favorites != null)
            {
                CallFavorites = new CodecCallFavorites();
                CallFavorites.Favorites = props.Favorites;
            }

            DirectoryRoot = new CodecDirectory();

            DirectoryBrowseHistory = new List<CodecDirectory>();

            CurrentDirectoryResultIsNotDirectoryRoot = new BoolFeedback(() => DirectoryBrowseHistory.Count > 0);

            CurrentDirectoryResultIsNotDirectoryRoot.FireUpdate();

            CodecSchedule = new CodecScheduleAwareness();
 
            //Set Feedback Actions
            SetFeedbackActions();

            CodecOsdIn = new RoutingInputPort(RoutingPortNames.CodecOsd, eRoutingSignalType.Audio | eRoutingSignalType.Video, 
				eRoutingPortConnectionType.Hdmi, new Action(StopSharing), this);
			HdmiIn2 = new RoutingInputPort(RoutingPortNames.HdmiIn2, eRoutingSignalType.Audio | eRoutingSignalType.Video, 
				eRoutingPortConnectionType.Hdmi, new Action(SelectPresentationSource1), this);
			HdmiIn3 = new RoutingInputPort(RoutingPortNames.HdmiIn3, eRoutingSignalType.Audio | eRoutingSignalType.Video, 
				eRoutingPortConnectionType.Hdmi, new Action(SelectPresentationSource2), this);

			HdmiOut1 = new RoutingOutputPort(RoutingPortNames.HdmiOut1, eRoutingSignalType.Audio | eRoutingSignalType.Video, 
				eRoutingPortConnectionType.Hdmi, null, this);
            HdmiOut2 = new RoutingOutputPort(RoutingPortNames.HdmiOut2, eRoutingSignalType.Audio | eRoutingSignalType.Video,
                eRoutingPortConnectionType.Hdmi, null, this);

			InputPorts.Add(CodecOsdIn);
			InputPorts.Add(HdmiIn2);
			InputPorts.Add(HdmiIn3);
			OutputPorts.Add(HdmiOut1);
			CreateOsdSource();

            ExternalSourceListEnabled = props.ExternalSourceListEnabled;
            ExternalSourceInputPort = props.ExternalSourceInputPort;

            if (props.UiBranding == null)
            {
                return;
            }
            Debug.Console(2, this, "Setting branding properties enable: {0} _brandingUrl {1}", props.UiBranding.Enable,
                props.UiBranding.BrandingUrl);

            BrandingEnabled = props.UiBranding.Enable;

            _brandingUrl = props.UiBranding.BrandingUrl;
        }

        private void SetFeedbackActions()
        {
            CodecStatus.Status.Audio.Volume.ValueChangedAction = VolumeLevelFeedback.FireUpdate;
            CodecStatus.Status.Audio.VolumeMute.ValueChangedAction = MuteFeedback.FireUpdate;
            CodecStatus.Status.Audio.Microphones.Mute.ValueChangedAction = PrivacyModeIsOnFeedback.FireUpdate;
            CodecStatus.Status.Standby.State.ValueChangedAction = new Action(() =>
                {
                    StandbyIsOnFeedback.FireUpdate();
                    HalfWakeModeIsOnFeedback.FireUpdate();
                    EnteringStandbyModeFeedback.FireUpdate();
                });
            CodecStatus.Status.RoomAnalytics.PeoplePresence.ValueChangedAction = RoomIsOccupiedFeedback.FireUpdate;
            CodecStatus.Status.RoomAnalytics.PeopleCount.Current.ValueChangedAction = PeopleCountFeedback.FireUpdate;
            CodecStatus.Status.Cameras.SpeakerTrack.Status.ValueChangedAction = CameraAutoModeIsOnFeedback.FireUpdate;
            CodecStatus.Status.Cameras.SpeakerTrack.Availability.ValueChangedAction = () => { SupportsCameraAutoMode = CodecStatus.Status.Cameras.SpeakerTrack.Availability.BoolValue; };
            CodecStatus.Status.Video.Selfview.Mode.ValueChangedAction = SelfviewIsOnFeedback.FireUpdate;
            CodecStatus.Status.Video.Selfview.PIPPosition.ValueChangedAction = ComputeSelfviewPipStatus;
            CodecStatus.Status.Video.Layout.LayoutFamily.Local.ValueChangedAction = ComputeLocalLayout;
            CodecStatus.Status.Conference.Presentation.Mode.ValueChangedAction = () =>
            {
                SharingContentIsOnFeedback.FireUpdate();
                FarEndIsSharingContentFeedback.FireUpdate();
            };
            CodecStatus.Status.Conference.DoNotDisturb.ValueChangedAction = DoNotDisturbModeIsOnFeedback.FireUpdate;

            CodecConfiguration.Configuration.Audio.SoundsAndAlerts.RingVolume.ValueChangedAction = RingtoneVolumeFeedback.FireUpdate;

            try
            {
                CodecStatus.Status.Video.Input.MainVideoMute.ValueChangedAction = CameraIsOffFeedback.FireUpdate;
            }
            catch (Exception ex)
            {
                Debug.Console(0, this, "Error setting MainVideuMute Action: {0}", ex);

                if (ex.InnerException != null)
                {
                    Debug.Console(0, this, "Error setting MainVideuMute Action: {0}", ex);
                }
            }
        }

        /// <summary>
		/// Creates the fake OSD source, and connects it's AudioVideo output to the CodecOsdIn input
		/// to enable routing 
		/// </summary>
		void CreateOsdSource()
		{
			OsdSource = new DummyRoutingInputsDevice(Key + "[osd]");
			DeviceManager.AddDevice(OsdSource);
			var tl = new TieLine(OsdSource.AudioVideoOutputPort, CodecOsdIn);
			TieLineCollection.Default.Add(tl);
		}

        public void InitializeBranding(string roomKey)
        {
            Debug.Console(1, this, "Initializing Branding for room {0}", roomKey);

            if (!BrandingEnabled)
            {
                return;
            }

            var mcBridgeKey = String.Format("mobileControlBridge-{0}", roomKey);

            var mcBridge = DeviceManager.GetDeviceForKey(mcBridgeKey) as IMobileControlRoomBridge;

            if (!String.IsNullOrEmpty(_brandingUrl))
            {
                Debug.Console(1, this, "Branding URL found: {0}", _brandingUrl);
                if (_brandingTimer != null)
                {
                    _brandingTimer.Stop();
                    _brandingTimer.Dispose();
                }

                _brandingTimer = new CTimer((o) =>
                {
                    if (_sendMcUrl)
                    {
                        SendMcBrandingUrl(mcBridge);
                        _sendMcUrl = false;
                    }
                    else
                    {
                        SendBrandingUrl();
                        _sendMcUrl = true;
                    }
                }, 0, 15000);
            } else if (String.IsNullOrEmpty(_brandingUrl))
            {
                Debug.Console(1, this, "No Branding URL found");
                if (mcBridge == null) return;

                Debug.Console(2, this, "Setting QR code URL: {0}", mcBridge.QrCodeUrl);

                mcBridge.UserCodeChanged += (o, a) => SendMcBrandingUrl(mcBridge);
                mcBridge.UserPromptedForCode += (o, a) => DisplayUserCode(mcBridge.UserCode);

                SendMcBrandingUrl(mcBridge);
            }
        }

        /// <summary>
        /// Displays the code for the specified duration
        /// </summary>
        /// <param name="code">Mobile Control user code</param>
        private void DisplayUserCode(string code)
        {
            EnqueueCommand(string.Format("xcommand userinterface message alert display title:\"Mobile Control User Code:\" text:\"{0}\" duration: 30", code));
        }

        private void SendMcBrandingUrl(IMobileControlRoomBridge mcBridge)
        {
            if (mcBridge == null)
            {
                return;
            }

            Debug.Console(1, this, "Sending url: {0}", mcBridge.QrCodeUrl);

            EnqueueCommand("xconfiguration userinterface custommessage: \"Scan the QR code with a mobile phone to get started\"");
            EnqueueCommand("xconfiguration userinterface osd halfwakemessage: \"Tap the touch panel or scan the QR code with a mobile phone to get started\"");

            var checksum = !String.IsNullOrEmpty(mcBridge.QrCodeChecksum)
                ? String.Format("checksum: {0} ", mcBridge.QrCodeChecksum)
                : String.Empty;

            EnqueueCommand(String.Format(
                "xcommand userinterface branding fetch {1}type: branding url: {0}",
                mcBridge.QrCodeUrl, checksum));
            EnqueueCommand(String.Format(
                "xcommand userinterface branding fetch {1}type: halfwakebranding url: {0}",
                mcBridge.QrCodeUrl, checksum));
        }

        private void SendBrandingUrl()
        {
            Debug.Console(1, this, "Sending url: {0}", _brandingUrl);

            EnqueueCommand(String.Format("xcommand userinterface branding fetch type: branding url: {0}",
                            _brandingUrl));
            EnqueueCommand(String.Format("xcommand userinterface branding fetch type: halfwakebranding url: {0}",
                _brandingUrl));
        }
        /// <summary>
        /// Starts the HTTP feedback server and syncronizes state of codec
        /// </summary>
        /// <returns></returns>
        public override bool CustomActivate()
        {
            CrestronConsole.AddNewConsoleCommand(SetCommDebug, "SetCodecCommDebug", "0 for Off, 1 for on", ConsoleAccessLevelEnum.AccessOperator);
            CrestronConsole.AddNewConsoleCommand(GetPhonebook, "GetCodecPhonebook", "Triggers a refresh of the codec phonebook", ConsoleAccessLevelEnum.AccessOperator);
            CrestronConsole.AddNewConsoleCommand(GetBookings, "GetCodecBookings", "Triggers a refresh of the booking data for today", ConsoleAccessLevelEnum.AccessOperator);

            PhonebookSyncState.InitialSyncCompleted += new EventHandler<EventArgs>(PhonebookSyncState_InitialSyncCompleted);

            return base.CustomActivate();
        }

        void PhonebookSyncState_InitialSyncCompleted(object sender, EventArgs e)
        {
            OnDirectoryResultReturned(DirectoryRoot);
        }

        #region Overrides of Device

        public override void Initialize()
        {
            var socket = Communication as ISocketStatus;
            if (socket != null)
            {
                socket.ConnectionChange += new EventHandler<GenericSocketStatusChageEventArgs>(socket_ConnectionChange);
            }

            Communication.Connect();

            CommunicationMonitor.Start();

            const string prefix = "xFeedback register ";

            _cliFeedbackRegistrationExpression =
                prefix + "/Configuration" + Delimiter +
                prefix + "/Status/Audio" + Delimiter +
                prefix + "/Status/Call" + Delimiter +
                prefix + "/Status/Conference/Presentation" + Delimiter +
                prefix + "/Status/Conference/DoNotDisturb" + Delimiter +
                prefix + "/Status/Cameras/SpeakerTrack" + Delimiter +
                prefix + "/Status/RoomAnalytics" + Delimiter +
                prefix + "/Status/RoomPreset" + Delimiter +
                prefix + "/Status/Standby" + Delimiter +
                prefix + "/Status/Video/Selfview" + Delimiter +
                prefix + "/Status/Video/Layout" + Delimiter +
                prefix + "/Status/Video/Input/MainVideoMute" + Delimiter +
                prefix + "/Bookings" + Delimiter +
                prefix + "/Event/Bookings" + Delimiter +
                prefix + "/Event/CameraPresetListUpdated" + Delimiter +
                prefix + "/Event/UserInterface/Presentation/ExternalSource/Selected/SourceIdentifier" + Delimiter +
                prefix + "/Event/CallDisconnect" + Delimiter; // Keep CallDisconnect last to detect when feedback registration completes correctly

        }

        #endregion

        /// <summary>
        /// Fires when initial codec sync is completed.  Used to then send commands to get call history, phonebook, bookings, etc.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void SyncState_InitialSyncCompleted(object sender, EventArgs e)
        {
            // Check for camera config info first
            if (_config.CameraInfo.Count > 0)
            {
                Debug.Console(0, this, "Reading codec cameraInfo from config properties.");
                SetUpCameras(_config.CameraInfo);
            }
            else
            {
                Debug.Console(0, this, "No cameraInfo defined in video codec config.  Attempting to get camera info from codec status data");
                try
                {
                    var cameraInfo = new List<CameraInfo>();

                    Debug.Console(0, this, "Codec reports {0} cameras", CodecStatus.Status.Cameras.Camera.Count);

                    foreach (var camera in CodecStatus.Status.Cameras.Camera)
                    {
                        Debug.Console(0, this,
@"Camera id: {0}
Name: {1}
ConnectorID: {2}"
, camera.id
, camera.Manufacturer.Value
, camera.Model.Value);

                        var id = Convert.ToUInt16(camera.id);
                        var info = new CameraInfo() { CameraNumber = id, Name = string.Format("{0} {1}", camera.Manufacturer.Value, camera.Model.Value), SourceId = camera.DetectedConnector.ConnectorId };
                        cameraInfo.Add(info);
                    }

                    Debug.Console(0, this, "Successfully got cameraInfo for {0} cameras from codec.", cameraInfo.Count);

                    SetUpCameras(cameraInfo);
                }
                catch (Exception ex)
                {
                    Debug.Console(2, this, "Error generating camera info from codec status data: {0}", ex);
                }
            }

            //CommDebuggingIsOn = false;

            GetCallHistory();

            PhonebookRefreshTimer = new CTimer(CheckCurrentHour, 3600000, 3600000);     // check each hour to see if the phonebook should be downloaded
            GetPhonebook(null);

            BookingsRefreshTimer = new CTimer(GetBookings, 900000,  900000);       // 15 minute timer to check for new booking info
            GetBookings(null);

            // Fire the ready event
            SetIsReady();
        }

        public void SetCommDebug(string s)
        {
            if (s == "1")
            {
                CommDebuggingIsOn = true;
                Debug.Console(0, this, "Comm Debug Enabled.");
            }
            else
            {
                CommDebuggingIsOn = false;
                Debug.Console(0, this, "Comm Debug Disabled.");
            }
        }

        void socket_ConnectionChange(object sender, GenericSocketStatusChageEventArgs e)
        {
			Debug.Console(1, this, "Socket status change {0}", e.Client.ClientStatus);
            if (e.Client.IsConnected)
            {
                if(!_syncState.LoginMessageWasReceived)
				    _loginMessageReceivedTimer = new CTimer(o => DisconnectClientAndReconnect(), 5000);
            }
            else
            {
                _syncState.CodecDisconnected();
                PhonebookSyncState.CodecDisconnected();

                if (PhonebookRefreshTimer != null)
                {
                    PhonebookRefreshTimer.Stop();
                    PhonebookRefreshTimer = null;
                }

                if (BookingsRefreshTimer != null)
                {
                    BookingsRefreshTimer.Stop();
                    BookingsRefreshTimer = null;
                }
            }
        }

        void DisconnectClientAndReconnect()
        {
            Debug.Console(1, this, "Retrying connection to codec.");

            Communication.Disconnect();

			_retryConnectionTimer = new CTimer(o => Communication.Connect(), 2000);

			//CrestronEnvironment.Sleep(2000);

			//Communication.Connect();
        }

        /// <summary>
        /// Gathers responses from the codec (including the delimiter.  Responses are checked to see if they contain JSON data and if so, the data is collected until a complete JSON
        /// message is received before forwarding the message to be deserialized.
        /// </summary>
        /// <param name="dev"></param>
        /// <param name="args"></param>
        void Port_LineReceived(object dev, GenericCommMethodReceiveTextArgs args)
        {
            if (CommDebuggingIsOn)
            {
                if (!_jsonFeedbackMessageIsIncoming)
                    Debug.Console(1, this, "RX: '{0}'", ComTextHelper.GetDebugText(args.Text));
            }

            if(args.Text.ToLower().Contains("xcommand"))
            {
                Debug.Console(1, this, "Received command echo response.  Ignoring");
                return;
            }

            if (args.Text == "{" + Delimiter)        // Check for the beginning of a new JSON message
            {
                _jsonFeedbackMessageIsIncoming = true;

                if (CommDebuggingIsOn)
                    Debug.Console(1, this, "Incoming JSON message...");

                _jsonMessage = new StringBuilder();
            }
            else if (args.Text == "}" + Delimiter)  // Check for the end of a JSON message
            {
                _jsonFeedbackMessageIsIncoming = false;

                _jsonMessage.Append(args.Text);

                if (CommDebuggingIsOn)
                    Debug.Console(1, this, "Complete JSON Received:\n{0}", _jsonMessage.ToString());

                // Enqueue the complete message to be deserialized

                _receiveQueue.Enqueue(new ProcessStringMessage(_jsonMessage.ToString(), DeserializeResponse));

                return;
            }

            if(_jsonFeedbackMessageIsIncoming)
            {
                _jsonMessage.Append(args.Text);

                //Debug.Console(1, this, "Building JSON:\n{0}", JsonMessage.ToString());
                return;
            }

            if (!_syncState.InitialSyncComplete)
            {
                switch (args.Text.Trim().ToLower()) // remove the whitespace
                {
                    case "*r login successful":
                        {
                            _syncState.LoginMessageReceived();

                            if(_loginMessageReceivedTimer != null)
                                _loginMessageReceivedTimer.Stop();

                            //SendText("echo off");
                            SendText("xPreferences outputmode json");
                            break;
                        }
                    case "xpreferences outputmode json":
                        {
                            if (_syncState.JsonResponseModeSet)
                                return;

                            _syncState.JsonResponseModeMessageReceived();

                            if (!_syncState.InitialStatusMessageWasReceived)
                                SendText("xStatus");
                            break;
                        }
                    case "xfeedback register /event/calldisconnect":
                        {
                            _syncState.FeedbackRegistered();
                            break;
                        }
                }
            }
        }

        /// <summary>
        /// Enqueues a command to be sent to the codec.
        /// </summary>
        /// <param name="command"></param>
        public void EnqueueCommand(string command)
        {
            _syncState.AddCommandToQueue(command);
        }

        /// <summary>
        /// Appends the delimiter and send the command to the codec.
        /// Should not be used for sending general commands to the codec.  Use EnqueueCommand instead.
        /// Should be used to get initial Status and Configuration as well as set up Feedback Registration
        /// </summary>
        /// <param name="command"></param>
        public void SendText(string command)
        {
            if (CommDebuggingIsOn)
                Debug.Console(1, this, "Sending: '{0}'",  ComTextHelper.GetDebugText(command + Delimiter));

            Communication.SendText(command + Delimiter);
        }

        void DeserializeResponse(string response)
        {
            try
            {
            //// Serializer settings.  We want to ignore null values and missing members
            //JsonSerializerSettings settings = new JsonSerializerSettings();
            //settings.NullValueHandling = NullValueHandling.Ignore;
            //settings.MissingMemberHandling = MissingMemberHandling.Ignore;
            //settings.ObjectCreationHandling = ObjectCreationHandling.Auto;

                if (response.IndexOf("\"Status\":{") > -1 || response.IndexOf("\"Status\": {") > -1)
                {
                    // Status Message

                    // Temp object so we can inpsect for call data before simply deserializing
                    CiscoCodecStatus.RootObject tempCodecStatus = new CiscoCodecStatus.RootObject();

                    JsonConvert.PopulateObject(response, tempCodecStatus);

                    // Check to see if the message contains /Status/Conference/Presentation/LocalInstance and extract source value 
                    var conference = tempCodecStatus.Status.Conference;

                    if (conference.Presentation != null && conference.Presentation.LocalInstance == null)
                    {
                        // Handles an empty presentation object response
                        return;   
                    }

                    if (conference.Presentation.LocalInstance.Count > 0)
                    {
                        if (!string.IsNullOrEmpty(conference.Presentation.LocalInstance[0].ghost))
                        {
                            _presentationSource = 0;
                            _presentationLocalOnly = false;
                            _presentationLocalRemote = false;
                        }
                        else if (conference.Presentation.LocalInstance[0].Source != null)
                        {
                            _presentationSource = conference.Presentation.LocalInstance[0].Source.IntValue;

                            // Check for any values in the SendingMode property
                            if (conference.Presentation.LocalInstance.Any((i) => !string.IsNullOrEmpty(i.SendingMode.Value)))
                            {
                                _presentationLocalOnly = conference.Presentation.LocalInstance.Any((i) => i.SendingMode.LocalOnly);
                                _presentationLocalRemote = conference.Presentation.LocalInstance.Any((i) => i.SendingMode.LocalRemote);
                            }
                        }

                        PresentationSourceFeedback.FireUpdate();
                        PresentationSendingLocalOnlyFeedback.FireUpdate();
                        PresentationSendingLocalRemoteFeedback.FireUpdate();
                    }

                    // Check to see if this is a call status message received after the initial status message
                    if (tempCodecStatus.Status.Call.Count > 0)
                    {
                        // Iterate through the call objects in the response
                        foreach (CiscoCodecStatus.Call call in tempCodecStatus.Status.Call)
                        {
                            var tempActiveCall = ActiveCalls.FirstOrDefault(c => c.Id.Equals(call.id));

                            if (tempActiveCall != null)
                            {
                                bool changeDetected = false;

                                eCodecCallStatus newStatus = eCodecCallStatus.Unknown;

                                // Update properties of ActiveCallItem
                                if(call.Status != null)
                                    if (!string.IsNullOrEmpty(call.Status.Value))
                                    {
                                        tempActiveCall.Status = CodecCallStatus.ConvertToStatusEnum(call.Status.Value);
                                        tempActiveCall.IsOnHold = tempActiveCall.Status == eCodecCallStatus.OnHold;
   
                                        if (newStatus == eCodecCallStatus.Connected)
                                            GetCallHistory();

                                        changeDetected = true;
                                    }
                                if (call.CallType != null)
                                    if (!string.IsNullOrEmpty(call.CallType.Value))
                                    {
                                        tempActiveCall.Type = CodecCallType.ConvertToTypeEnum(call.CallType.Value);
                                        changeDetected = true;
                                    }
                                if (call.DisplayName != null)
                                    if (!string.IsNullOrEmpty(call.DisplayName.Value))
                                    {
                                        tempActiveCall.Name = call.DisplayName.Value;
                                        changeDetected = true;
                                    }
                                if (call.Direction != null)
                                {
                                    if (!string.IsNullOrEmpty(call.Direction.Value))
                                    {
                                        tempActiveCall.Direction = CodecCallDirection.ConvertToDirectionEnum(call.Direction.Value);
                                        changeDetected = true;
                                    }
                                }
                                if(call.Duration != null)
                                {
                                    if(!string.IsNullOrEmpty(call.Duration.Value))
                                    {
                                        tempActiveCall.Duration = call.Duration.DurationValue;
                                        changeDetected = true;
                                    }
                                }
                                if(call.PlacedOnHold != null)
                                {
                                    tempActiveCall.IsOnHold = call.PlacedOnHold.BoolValue;
                                    changeDetected = true;
                                }

                                if (changeDetected)
                                {
                                    SetSelfViewMode();
                                    OnCallStatusChange(tempActiveCall);
                                    ListCalls();
                                }
                            }
                            else if( call.ghost == null )   // if the ghost value is present the call has ended already
                            {
                                // Create a new call item
                                var newCallItem = new CodecActiveCallItem()
                                {
                                    Id = call.id,
                                    Status = CodecCallStatus.ConvertToStatusEnum(call.Status.Value),
                                    Name = call.DisplayName.Value,
                                    Number = call.RemoteNumber.Value,
                                    Type = CodecCallType.ConvertToTypeEnum(call.CallType.Value),
                                    Direction = CodecCallDirection.ConvertToDirectionEnum(call.Direction.Value),
                                    Duration = call.Duration.DurationValue,
                                    IsOnHold = call.PlacedOnHold.BoolValue,
                                };

                                // Add it to the ActiveCalls List
                                ActiveCalls.Add(newCallItem);

                                ListCalls();

                                SetSelfViewMode();
                                OnCallStatusChange(newCallItem);
                            }

                        }

                    }

                    // Check for Room Preset data (comes in partial, so we need to handle these responses differently to prevent appending duplicate items
                    var tempPresets = tempCodecStatus.Status.RoomPreset;

                    if (tempPresets.Count > 0)
                    {
                        // Create temporary list to store the existing items from the CiscoCodecStatus.RoomPreset collection
                        var existingRoomPresets = new List<CiscoCodecStatus.RoomPreset>();
                        // Add the existing items to the temporary list
                        existingRoomPresets.AddRange(CodecStatus.Status.RoomPreset);
                        // Populate the CodecStatus object (this will append new values to the RoomPreset collection
                        JsonConvert.PopulateObject(response, CodecStatus);

                        var jResponse = JObject.Parse(response);


                        IList<JToken> roomPresets = jResponse["Status"]["RoomPreset"].Children().ToList();
                        // Iterate the new items in this response agains the temporary list.  Overwrite any existing items and add new ones.
                        foreach (var camPreset in tempPresets)
                        {
                            var preset = camPreset as CiscoCodecStatus.RoomPreset;
                            if (preset == null) continue;
                            // First fine the existing preset that matches the id
                            var existingPreset = existingRoomPresets.FirstOrDefault(p => p.id.Equals(preset.id));
                            if (existingPreset != null)
                            {
                                Debug.Console(1, this, "Existing Room Preset with ID: {0} found. Updating.", existingPreset.id);

                                JToken updatedPreset = null;

                                // Find the JToken from the response with the matching id
                                foreach (var jPreset in roomPresets)
                                {
                                    if (jPreset["id"].Value<string>() == existingPreset.id)
                                        updatedPreset = jPreset;
                                }

                                if (updatedPreset != null)
                                {
                                    // use PopulateObject to overlay the partial data onto the existing object
                                    JsonConvert.PopulateObject(updatedPreset.ToString(), existingPreset);
                                }
                               
                            }
                            else
                            {
                                Debug.Console(1, this, "New Room Preset with ID: {0}. Adding.", preset.id);
                                existingRoomPresets.Add(preset);
                            }
                        }

                        // Replace the list in the CodecStatus object with the processed list
                        CodecStatus.Status.RoomPreset = existingRoomPresets;

                        // Generecise the list
                        NearEndPresets = existingRoomPresets.GetGenericPresets<CiscoCodecStatus.RoomPreset, CodecRoomPreset>();

                        var handler = CodecRoomPresetsListHasChanged;
                        if (handler != null)
                        {
                            handler(this, new EventArgs());
                        }
                    }
                    else
                    {
                        JsonConvert.PopulateObject(response, CodecStatus);
                    }

                    if (!_syncState.InitialStatusMessageWasReceived)
                    {
                        _syncState.InitialStatusMessageReceived();

                        if (!_syncState.InitialConfigurationMessageWasReceived)
                        {
                            SendText("xConfiguration");
                        }
                    }
                }
                else if (response.IndexOf("\"Configuration\":{") > -1 || response.IndexOf("\"Configuration\": {") > -1)
                {
                    // Configuration Message

                    JsonConvert.PopulateObject(response, CodecConfiguration);

                    if (!_syncState.InitialConfigurationMessageWasReceived)
                    {
                        _syncState.InitialConfigurationMessageReceived();
                        if (!_syncState.FeedbackWasRegistered)
                        {
                            SendText(_cliFeedbackRegistrationExpression);
                        }
                    }

                }
                else if (response.IndexOf("\"Event\":{") > -1 || response.IndexOf("\"Event\": {") > -1)
                {
                    if (response.IndexOf("\"CallDisconnect\":{") > -1 || response.IndexOf("\"CallDisconnect\": {") > -1)
                    {
                        CiscoCodecEvents.RootObject eventReceived = new CiscoCodecEvents.RootObject();

                        JsonConvert.PopulateObject(response, eventReceived);

                        EvalutateDisconnectEvent(eventReceived);
                    }
                    else if (response.IndexOf("\"Bookings\":{") > -1 || response.IndexOf("\"Bookings\": {") > -1) // The list has changed, reload it
					{
						GetBookings(null);
					}
					
					else if (response.IndexOf("\"UserInterface\":{") > -1 || response.IndexOf("\"UserInterface\": {") > -1) // External Source Trigger
					{
						CiscoCodecEvents.RootObject eventReceived = new CiscoCodecEvents.RootObject();
                        JsonConvert.PopulateObject(response, eventReceived);
						Debug.Console(2, this, "*** Got an External Source Selection {0} {1}", eventReceived, eventReceived.Event.UserInterface, eventReceived.Event.UserInterface.Presentation.ExternalSource.Selected.SourceIdentifier.Value);

						if (RunRouteAction != null && !_externalSourceChangeRequested)
						{
							RunRouteAction(eventReceived.Event.UserInterface.Presentation.ExternalSource.Selected.SourceIdentifier.Value, null);
						}

					    _externalSourceChangeRequested = false;
					}
                }
                else if (response.IndexOf("\"CommandResponse\":{") > -1 || response.IndexOf("\"CommandResponse\": {") > -1)
                {
                    // CommandResponse Message

                    if (response.IndexOf("\"CallHistoryRecentsResult\":{") > -1 || response.IndexOf("\"CallHistoryRecentsResult\": {") > -1)
                    {
                        var codecCallHistory = new CiscoCallHistory.RootObject();

                        JsonConvert.PopulateObject(response, codecCallHistory);

                        CallHistory.ConvertCiscoCallHistoryToGeneric(codecCallHistory.CommandResponse.CallHistoryRecentsResult.Entry);
                    }
                    else if (response.IndexOf("\"CallHistoryDeleteEntryResult\":{") > -1 || response.IndexOf("\"CallHistoryDeleteEntryResult\": {") > -1)
                    {
                        GetCallHistory();
                    }
                    else if (response.IndexOf("\"PhonebookSearchResult\":{") > -1 || response.IndexOf("\"PhonebookSearchResult\": {") > -1)
                    {
                        var codecPhonebookResponse = new CiscoCodecPhonebook.RootObject();

                        JsonConvert.PopulateObject(response, codecPhonebookResponse);

                        if (!PhonebookSyncState.InitialPhonebookFoldersWasReceived)
                        {
                            // Check if the phonebook has any folders
                            PhonebookSyncState.InitialPhonebookFoldersReceived();

                            PhonebookSyncState.SetPhonebookHasFolders(codecPhonebookResponse.CommandResponse.PhonebookSearchResult.Folder.Count > 0);

                            if (PhonebookSyncState.PhonebookHasFolders)
                            {
                                DirectoryRoot.AddFoldersToDirectory(CiscoCodecPhonebook.GetRootFoldersFromSearchResult(codecPhonebookResponse.CommandResponse.PhonebookSearchResult));
                            }

                            // Get the number of contacts in the phonebook
                            GetPhonebookContacts();
                        }
                        else if (!PhonebookSyncState.NumberOfContactsWasReceived)
                        {
                            // Store the total number of contacts in the phonebook
                            PhonebookSyncState.SetNumberOfContacts(Int32.Parse(codecPhonebookResponse.CommandResponse.PhonebookSearchResult.ResultInfo.TotalRows.Value));

                            DirectoryRoot.AddContactsToDirectory(CiscoCodecPhonebook.GetRootContactsFromSearchResult(codecPhonebookResponse.CommandResponse.PhonebookSearchResult));

                            PhonebookSyncState.PhonebookRootEntriesReceived();

                            PrintDirectory(DirectoryRoot);
                        }
                        else if (PhonebookSyncState.InitialSyncComplete)
                        {
                            var directoryResults = new CodecDirectory();

                            if(codecPhonebookResponse.CommandResponse.PhonebookSearchResult.ResultInfo.TotalRows.Value != "0")
                                directoryResults = CiscoCodecPhonebook.ConvertCiscoPhonebookToGeneric(codecPhonebookResponse.CommandResponse.PhonebookSearchResult);

                            PrintDirectory(directoryResults);

                            DirectoryBrowseHistory.Add(directoryResults);

                            OnDirectoryResultReturned(directoryResults);
   
                        }
                    }
                    else if (response.IndexOf("\"BookingsListResult\":{") > -1)
                    {
                        var codecBookings = new CiscoCodecBookings.RootObject();

                        JsonConvert.PopulateObject(response, codecBookings);

                        if(codecBookings.CommandResponse.BookingsListResult.ResultInfo.TotalRows.Value != "0")
                            CodecSchedule.Meetings = CiscoCodecBookings.GetGenericMeetingsFromBookingResult(codecBookings.CommandResponse.BookingsListResult.Booking);

                        BookingsRefreshTimer.Reset(900000, 900000);
                    }

                }  
                
            }
            catch (Exception ex)
            {
                Debug.Console(1, this, "Error Deserializing feedback from codec: {0}", ex);

                if (ex is Newtonsoft.Json.JsonReaderException)
                {
                    Debug.Console(1, this, "Received malformed response from codec.");

                    //Communication.Disconnect();

                    //Initialize();
                }

            }
        }

        /// <summary>
        /// Call when directory results are updated
        /// </summary>
        /// <param name="result"></param>
        void OnDirectoryResultReturned(CodecDirectory result)
        {
            CurrentDirectoryResultIsNotDirectoryRoot.FireUpdate();

            // This will return the latest results to all UIs.  Multiple indendent UI Directory browsing will require a different methodology
            var handler = DirectoryResultReturned;
            if (handler != null)
            {
                Debug.Console(2, this, "Directory result returned");
                handler(this, new DirectoryEventArgs()
                {
                    Directory = result,
                    DirectoryIsOnRoot = !CurrentDirectoryResultIsNotDirectoryRoot.BoolValue
                });
            }

            PrintDirectory(result);
        }

        /// <summary>
        /// Evaluates an event received from the codec
        /// </summary>
        /// <param name="eventReceived"></param>
        void EvalutateDisconnectEvent(CiscoCodecEvents.RootObject eventReceived)
        {
            if (eventReceived.Event.CallDisconnect != null)
            {
                var tempActiveCall = ActiveCalls.FirstOrDefault(c => c.Id.Equals(eventReceived.Event.CallDisconnect.CallId.Value));

                // Remove the call from the Active calls list
                if (tempActiveCall != null)
                {
                    ActiveCalls.Remove(tempActiveCall);

                    ListCalls();

                    SetSelfViewMode();
                    // Notify of the call disconnection
                    SetNewCallStatusAndFireCallStatusChange(eCodecCallStatus.Disconnected, tempActiveCall);

                    GetCallHistory();
                }
            }
        }

		/// <summary>
		/// 
		/// </summary>
		/// <param name="selector"></param>
		public override void ExecuteSwitch(object selector)
		{
			(selector as Action)();
			_presentationSourceKey = selector.ToString();
		}

		/// <summary>
		/// This is necessary for devices that are "routers" in the middle of the path, even though it only has one output and 
		/// may only have one input.
		/// </summary>
        public void ExecuteSwitch(object inputSelector, object outputSelector, eRoutingSignalType signalType)
        {
			ExecuteSwitch(inputSelector);
            _presentationSourceKey = inputSelector.ToString();
        }


        /// <summary>
        /// Gets the ID of the last connected call
        /// </summary>
        /// <returns></returns>
        public string GetCallId()
        {
            string callId = null;

            if (ActiveCalls.Count > 1)
            {
                var lastCallIndex = ActiveCalls.Count - 1;
                callId = ActiveCalls[lastCallIndex].Id;
            }
            else if (ActiveCalls.Count == 1)
                callId =  ActiveCalls[0].Id;

            return callId;

        }

        public void GetCallHistory()
        {
            EnqueueCommand("xCommand CallHistory Recents Limit: 20 Order: OccurrenceTime");
        }

        /// <summary>
        /// Required for IHasScheduleAwareness
        /// </summary>
        public void GetSchedule()
        {
            GetBookings(null);
        }

        /// <summary>
        /// Gets the bookings for today
        /// </summary>
        /// <param name="command"></param>
        public void GetBookings(object command)
        {
            Debug.Console(1, this, "Retrieving Booking Info from Codec. Current Time: {0}", DateTime.Now.ToLocalTime());

            EnqueueCommand("xCommand Bookings List Days: 1 DayOffset: 0");
        }

        /// <summary>
        /// Checks to see if it is 2am (or within that hour) and triggers a download of the phonebook
        /// </summary>
        /// <param name="o"></param>
        public void CheckCurrentHour(object o)
        {
            if (DateTime.Now.Hour == 2)
            {
                Debug.Console(1, this, "Checking hour to see if phonebook should be downloaded.  Current hour is {0}", DateTime.Now.Hour);

                GetPhonebook(null);
                PhonebookRefreshTimer.Reset(3600000, 3600000);
            }
        }

        /// <summary>
        /// Triggers a refresh of the codec phonebook
        /// </summary>
        /// <param name="command">Just to allow this method to be called from a console command</param>
        public void GetPhonebook(string command)
        {
            PhonebookSyncState.CodecDisconnected();

            DirectoryRoot = new CodecDirectory();

            GetPhonebookFolders();
        }

        private void GetPhonebookFolders()
        {
            // Get Phonebook Folders (determine local/corporate from config, and set results limit)
            EnqueueCommand(string.Format("xCommand Phonebook Search PhonebookType: {0} ContactType: Folder", _phonebookMode));
        }

        private void GetPhonebookContacts()
        {
            // Get Phonebook Folders (determine local/corporate from config, and set results limit)
            EnqueueCommand(string.Format("xCommand Phonebook Search PhonebookType: {0} ContactType: Contact Limit: {1}", _phonebookMode, _phonebookResultsLimit));
        }

        /// <summary>
        /// Searches the codec phonebook for all contacts matching the search string
        /// </summary>
        /// <param name="searchString"></param>
        public void SearchDirectory(string searchString)
        {
            EnqueueCommand(string.Format("xCommand Phonebook Search SearchString: \"{0}\" PhonebookType: {1} ContactType: Contact Limit: {2}", searchString, _phonebookMode, _phonebookResultsLimit));
        }

        /// <summary>
        /// // Get contents of a specific folder in the phonebook
        /// </summary>
        /// <param name="folderId"></param>
        public void GetDirectoryFolderContents(string folderId)
        {
            EnqueueCommand(string.Format("xCommand Phonebook Search FolderId: {0} PhonebookType: {1} ContactType: Any Limit: {2}", folderId, _phonebookMode, _phonebookResultsLimit));
        }

        /// <summary>
        /// Sets the parent folder contents or the directory root as teh current directory and fires the event. Used to browse up a level
        /// </summary>
        /// <returns></returns>
        public void GetDirectoryParentFolderContents()
        {
            var currentDirectory = new CodecDirectory();

            if (DirectoryBrowseHistory.Count > 0)
            {
                var lastItemIndex = DirectoryBrowseHistory.Count - 1;
                var parentDirectoryContents = DirectoryBrowseHistory[lastItemIndex];

                DirectoryBrowseHistory.Remove(DirectoryBrowseHistory[lastItemIndex]);

                currentDirectory = parentDirectoryContents;

            }
            else
            {
                currentDirectory = DirectoryRoot;
            }

            OnDirectoryResultReturned(currentDirectory);
        }

        /// <summary>
        /// Clears the session browse history and fires the event with the directory root
        /// </summary>
        public void SetCurrentDirectoryToRoot()
        {
            DirectoryBrowseHistory.Clear();

            OnDirectoryResultReturned(DirectoryRoot);
        }

        /// <summary>
        /// Prints the directory to console
        /// </summary>
        /// <param name="directory"></param>
        void PrintDirectory(CodecDirectory directory)
        {
            if (Debug.Level > 0)
            {
                Debug.Console(1, this, "Directory Results:\n");

                foreach (DirectoryItem item in directory.CurrentDirectoryResults)
                {
                    if (item is DirectoryFolder)
                    {
                        Debug.Console(1, this, "[+] {0}", item.Name);
                    }
                    else if (item is DirectoryContact)
                    {
                        Debug.Console(1, this, "{0}", item.Name);
                    }
                }
                Debug.Console(1, this, "Directory is on Root Level: {0}", !CurrentDirectoryResultIsNotDirectoryRoot.BoolValue);
            }

        }

        /// <summary>
        /// Simple dial method
        /// </summary>
        /// <param name="number"></param>
        public override void Dial(string number)
        {
         	EnqueueCommand(string.Format("xCommand Dial Number: \"{0}\"", number));
        }

        /// <summary>
        /// Dials a specific meeting
        /// </summary>
        /// <param name="meeting"></param>
        public override void Dial(Meeting meeting)
        {
            foreach (Call c in meeting.Calls)
            {
                Dial(c.Number, c.Protocol, c.CallRate, c.CallType, meeting.Id);
            }
        }

        /// <summary>
        /// Detailed dial method
        /// </summary>
        /// <param name="number"></param>
        /// <param name="protocol"></param>
        /// <param name="callRate"></param>
        /// <param name="callType"></param>
        /// <param name="meetingId"></param>
        public void Dial(string number, string protocol, string callRate, string callType, string meetingId)
        {
            EnqueueCommand(string.Format("xCommand Dial Number: \"{0}\" Protocol: {1} CallRate: {2} CallType: {3} BookingId: {4}", number, protocol, callRate, callType, meetingId));
        }


        public override void EndCall(CodecActiveCallItem activeCall)
        {
            EnqueueCommand(string.Format("xCommand Call Disconnect CallId: {0}", activeCall.Id));
        }

        public override void EndAllCalls()
        {
            foreach (CodecActiveCallItem activeCall in ActiveCalls)
            {
                EnqueueCommand(string.Format("xCommand Call Disconnect CallId: {0}", activeCall.Id));
            }
        }

        public override void AcceptCall(CodecActiveCallItem item)
        {
            EnqueueCommand("xCommand Call Accept");
        }

        public override void RejectCall(CodecActiveCallItem item)
        {
            EnqueueCommand("xCommand Call Reject");
        }

        #region IHasCallHold Members

        public void HoldCall(CodecActiveCallItem activeCall)
        {
            EnqueueCommand(string.Format("xCommand Call Hold CallId: {0}", activeCall.Id));
        }

        public void ResumeCall(CodecActiveCallItem activeCall)
        {
            EnqueueCommand(string.Format("xCommand Call Resume CallId: {0}", activeCall.Id));
        }

        #endregion

        #region IJoinCalls

        public void JoinCall(CodecActiveCallItem activeCall)
        {
            EnqueueCommand(string.Format("xCommand Call Join CallId: {0}", activeCall.Id));
        }

        public void JoinAllCalls()
        {
            StringBuilder ids = new StringBuilder();

            foreach (var call in ActiveCalls)
            {
                if (call.IsActiveCall)
                {
                    ids.Append(string.Format(" CallId: {0}", call.Id));
                }
            }

            if (ids.Length > 0)
            {
                EnqueueCommand(string.Format("xCommand Call Join {0}", ids.ToString()));
            }
        }

        #endregion

        /// <summary>
        /// Sends tones to the last connected call
        /// </summary>
        /// <param name="s"></param>
        public override void SendDtmf(string s)
        {
            EnqueueCommand(string.Format("xCommand Call DTMFSend CallId: {0} DTMFString: \"{1}\"", GetCallId(), s));
        }

        /// <summary>
        /// Sends tones to a specific call
        /// </summary>
        /// <param name="s"></param>
        /// <param name="activeCall"></param>
        public override void SendDtmf(string s, CodecActiveCallItem activeCall)
        {
            EnqueueCommand(string.Format("xCommand Call DTMFSend CallId: {0} DTMFString: \"{1}\"", activeCall.Id, s));
        }

        public void SelectPresentationSource(int source)
        {
            _desiredPresentationSource = source;

            StartSharing();
        }

        /// <summary>
        /// Sets the ringtone volume level
        /// </summary>
        /// <param name="volume">level from 0 - 100 in increments of 5</param>
        public void SetRingtoneVolume(int volume)
        {
            if (volume < 0 || volume > 100)
            {
                Debug.Console(0, this, "Cannot set ringtone volume to '{0}'. Value must be between 0 - 100", volume);
                return;
            }

            if (volume % 5 != 0)
            {
                Debug.Console(0, this, "Cannot set ringtone volume to '{0}'. Value must be between 0 - 100 and a multiple of 5", volume);
                return;
            }

            EnqueueCommand(string.Format("xConfiguration Audio SoundsAndAlerts RingVolume: {0}", volume));
        }

        /// <summary>
        /// Select source 1 as the presetnation source
        /// </summary>
        public void SelectPresentationSource1()
        {
            SelectPresentationSource(2);
        }

        /// <summary>
        /// Select source 2 as the presetnation source
        /// </summary>
        public void SelectPresentationSource2()
        {
            SelectPresentationSource(3);
        }



        /// <summary>
        /// Starts presentation sharing
        /// </summary>
        public override void StartSharing()
        {
            string sendingMode = string.Empty;

            if (IsInCall)
                sendingMode = "LocalRemote";
            else
                sendingMode = "LocalOnly";

            if (_desiredPresentationSource > 0)
                EnqueueCommand(string.Format("xCommand Presentation Start PresentationSource: {0} SendingMode: {1}", _desiredPresentationSource, sendingMode));
        }

        /// <summary>
        /// Stops sharing the current presentation
        /// </summary>
        public override void StopSharing()
        {
            _desiredPresentationSource = 0;

            EnqueueCommand("xCommand Presentation Stop");
        }



        public override void PrivacyModeOn()
        {
            EnqueueCommand("xCommand Audio Microphones Mute");
        }

        public override void PrivacyModeOff()
        {
            EnqueueCommand("xCommand Audio Microphones Unmute");
        }

        public override void PrivacyModeToggle()
        {
            EnqueueCommand("xCommand Audio Microphones ToggleMute");
        }

        public override void MuteOff()
        {
            EnqueueCommand("xCommand Audio Volume Unmute");
        }

        public override void MuteOn()
        {
            EnqueueCommand("xCommand Audio Volume Mute");
        }

        public override void MuteToggle()
        {
            EnqueueCommand("xCommand Audio Volume ToggleMute");
        }

        /// <summary>
        /// Increments the voluem
        /// </summary>
        /// <param name="pressRelease"></param>
        public override void VolumeUp(bool pressRelease)
        {
            EnqueueCommand("xCommand Audio Volume Increase");
        }

        /// <summary>
        /// Decrements the volume
        /// </summary>
        /// <param name="pressRelease"></param>
        public override void VolumeDown(bool pressRelease)
        {
            EnqueueCommand("xCommand Audio Volume Decrease");
        }

        /// <summary>
        /// Scales the level and sets the codec to the specified level within its range
        /// </summary>
        /// <param name="level">level from slider (0-65535 range)</param>
        public override void SetVolume(ushort level)
        {
            var scaledLevel = CrestronEnvironment.ScaleWithLimits(level, 65535, 0, 100, 0); 
            EnqueueCommand(string.Format("xCommand Audio Volume Set Level: {0}", scaledLevel));
        }

        /// <summary>
        /// Recalls the default volume on the codec
        /// </summary>
        public void VolumeSetToDefault()
        {
            EnqueueCommand("xCommand Audio Volume SetToDefault");
        }

        /// <summary>
        /// Puts the codec in standby mode
        /// </summary>
        public override void StandbyActivate()
        {
            EnqueueCommand("xCommand Standby Activate");
        }

        /// <summary>
        /// Wakes the codec from standby
        /// </summary>
        public override void StandbyDeactivate()
        {
            EnqueueCommand("xCommand Standby Deactivate");
        }

        public override void LinkToApi(BasicTriList trilist, uint joinStart, string joinMapKey, EiscApiAdvanced bridge)
        {
            var joinMap = new CiscoCodecJoinMap(joinStart);

            var customJoins = JoinMapHelper.TryGetJoinMapAdvancedForDevice(joinMapKey);

            if (customJoins != null)
            {
                joinMap.SetCustomJoinData(customJoins);
            }

            if (bridge != null)
            {
                bridge.AddJoinMap(Key, joinMap);
            }

            LinkVideoCodecToApi(this, trilist, joinStart, joinMapKey, bridge);

            LinkCiscoCodecToApi(trilist, joinMap);
        }

        public void LinkCiscoCodecToApi(BasicTriList trilist, CiscoCodecJoinMap joinMap)
        {
            // Custom commands to codec
            trilist.SetStringSigAction(joinMap.CommandToDevice.JoinNumber, (s) => this.EnqueueCommand(s));


            var dndCodec = this as IHasDoNotDisturbMode;
            if (dndCodec != null)
            {
                dndCodec.DoNotDisturbModeIsOnFeedback.LinkInputSig(trilist.BooleanInput[joinMap.ActivateDoNotDisturbMode.JoinNumber]);
                dndCodec.DoNotDisturbModeIsOnFeedback.LinkComplementInputSig(trilist.BooleanInput[joinMap.DeactivateDoNotDisturbMode.JoinNumber]);

                trilist.SetSigFalseAction(joinMap.ActivateDoNotDisturbMode.JoinNumber, () => dndCodec.ActivateDoNotDisturbMode());
                trilist.SetSigFalseAction(joinMap.DeactivateDoNotDisturbMode.JoinNumber, () => dndCodec.DeactivateDoNotDisturbMode());
                trilist.SetSigFalseAction(joinMap.ToggleDoNotDisturbMode.JoinNumber, () => dndCodec.ToggleDoNotDisturbMode());
            }

            var halfwakeCodec = this as IHasHalfWakeMode;
            if (halfwakeCodec != null)
            {
                halfwakeCodec.StandbyIsOnFeedback.LinkInputSig(trilist.BooleanInput[joinMap.ActivateStandby.JoinNumber]);
                halfwakeCodec.StandbyIsOnFeedback.LinkComplementInputSig(trilist.BooleanInput[joinMap.DeactivateStandby.JoinNumber]);
                halfwakeCodec.HalfWakeModeIsOnFeedback.LinkInputSig(trilist.BooleanInput[joinMap.ActivateHalfWakeMode.JoinNumber]);
                halfwakeCodec.EnteringStandbyModeFeedback.LinkInputSig(trilist.BooleanInput[joinMap.EnteringStandbyMode.JoinNumber]);

                trilist.SetSigFalseAction(joinMap.ActivateStandby.JoinNumber, () => halfwakeCodec.StandbyActivate());
                trilist.SetSigFalseAction(joinMap.DeactivateStandby.JoinNumber, () => halfwakeCodec.StandbyDeactivate());
                trilist.SetSigFalseAction(joinMap.ActivateHalfWakeMode.JoinNumber, () => halfwakeCodec.HalfwakeActivate());
            }

            // Ringtone volume
            trilist.SetUShortSigAction(joinMap.RingtoneVolume.JoinNumber, (u) => SetRingtoneVolume(u));
            RingtoneVolumeFeedback.LinkInputSig(trilist.UShortInput[joinMap.RingtoneVolume.JoinNumber]);

            // Presentation Source
            trilist.SetUShortSigAction(joinMap.PresentationSource.JoinNumber, (u) => SelectPresentationSource(u));
            PresentationSourceFeedback.LinkInputSig(trilist.UShortInput[joinMap.PresentationSource.JoinNumber]);

            PresentationSendingLocalOnlyFeedback.LinkInputSig(trilist.BooleanInput[joinMap.PresentationLocalOnly.JoinNumber]);
            PresentationSendingLocalRemoteFeedback.LinkInputSig(trilist.BooleanInput[joinMap.PresentationLocalRemote.JoinNumber]);
        }

        /// <summary>
        /// Reboots the codec
        /// </summary>
        public void Reboot()
        {
            EnqueueCommand("xCommand SystemUnit Boot Action: Restart");
        }

        /// <summary>
        /// Sets SelfView Mode based on config
        /// </summary>
        void SetSelfViewMode()
        {
            if (!IsInCall)
            {
                SelfViewModeOff();
            }
            else
            {
                if (ShowSelfViewByDefault)
                    SelfViewModeOn();
                else
                    SelfViewModeOff();
            }
        }

        /// <summary>
        /// Turns on Selfview Mode
        /// </summary>
        public void SelfViewModeOn()
        {
            EnqueueCommand("xCommand Video Selfview Set Mode: On");
        }

        /// <summary>
        /// Turns off Selfview Mode
        /// </summary>
        public void SelfViewModeOff()
        {
            EnqueueCommand("xCommand Video Selfview Set Mode: Off");
        }

        /// <summary>
        /// Toggles Selfview mode on/off
        /// </summary>
        public void SelfViewModeToggle()
        {
            string mode = string.Empty;

            if (CodecStatus.Status.Video.Selfview.Mode.BoolValue)
                mode = "Off";
            else
                mode = "On";

            EnqueueCommand(string.Format("xCommand Video Selfview Set Mode: {0}", mode));                    
        }

        /// <summary>
        /// Sets a specified position for the selfview PIP window
        /// </summary>
        /// <param name="position"></param>
        public void SelfviewPipPositionSet(CodecCommandWithLabel position)
        {
            EnqueueCommand(string.Format("xCommand Video Selfview Set Mode: On PIPPosition: {0}", position.Command));
        }

        /// <summary>
        /// Toggles to the next selfview PIP position
        /// </summary>
        public void SelfviewPipPositionToggle()
        {
            if (_currentSelfviewPipPosition != null)
            {
                var nextPipPositionIndex = SelfviewPipPositions.IndexOf(_currentSelfviewPipPosition) + 1;

                if (nextPipPositionIndex >= SelfviewPipPositions.Count) // Check if we need to loop back to the first item in the list
                    nextPipPositionIndex = 0;                  

                SelfviewPipPositionSet(SelfviewPipPositions[nextPipPositionIndex]);
            }
        }

        /// <summary>
        /// Sets a specific local layout
        /// </summary>
        /// <param name="layout"></param>
        public void LocalLayoutSet(CodecCommandWithLabel layout)
        {
            EnqueueCommand(string.Format("xCommand Video Layout LayoutFamily Set Target: local LayoutFamily: {0}", layout.Command));
        }

        /// <summary>
        /// Toggles to the next local layout
        /// </summary>
        public void LocalLayoutToggle()
        {
            if(_currentLocalLayout != null)
            {
                var nextLocalLayoutIndex = LocalLayouts.IndexOf(_currentLocalLayout) + 1;

                if (nextLocalLayoutIndex >= LocalLayouts.Count)  // Check if we need to loop back to the first item in the list
                    nextLocalLayoutIndex = 0;
                
                LocalLayoutSet(LocalLayouts[nextLocalLayoutIndex]);
            }
        }

        /// <summary>
        /// Toggles between single/prominent layouts
        /// </summary>
        public void LocalLayoutToggleSingleProminent()
        {
            if (_currentLocalLayout != null)
            {
                if (_currentLocalLayout.Label != "Prominent")
                    LocalLayoutSet(LocalLayouts.FirstOrDefault(l => l.Label.Equals("Prominent")));
                else
                    LocalLayoutSet(LocalLayouts.FirstOrDefault(l => l.Label.Equals("Single")));
            }

        }

		/// <summary>
		/// 
		/// </summary>
		public void MinMaxLayoutToggle()
		{
			if (PresentationViewMaximizedFeedback.BoolValue)
				_currentPresentationView = "Minimized";
			else
				_currentPresentationView = "Maximized";

			EnqueueCommand(string.Format("xCommand Video PresentationView Set View:  {0}", _currentPresentationView));
			PresentationViewMaximizedFeedback.FireUpdate();
		}

        /// <summary>
        /// Calculates the current selfview PIP position
        /// </summary>
        void ComputeSelfviewPipStatus()
        {
            _currentSelfviewPipPosition = SelfviewPipPositions.FirstOrDefault(p => p.Command.ToLower().Equals(CodecStatus.Status.Video.Selfview.PIPPosition.Value.ToLower()));

            if(_currentSelfviewPipPosition != null)
                SelfviewIsOnFeedback.FireUpdate();
        }

        /// <summary>
        /// Calculates the current local Layout
        /// </summary>
        void ComputeLocalLayout()
        {
            _currentLocalLayout = LocalLayouts.FirstOrDefault(l => l.Command.ToLower().Equals(CodecStatus.Status.Video.Layout.LayoutFamily.Local.Value.ToLower()));

            if (_currentLocalLayout != null)
                LocalLayoutFeedback.FireUpdate();
        }

        public void RemoveCallHistoryEntry(CodecCallHistory.CallHistoryEntry entry)
        {
            EnqueueCommand(string.Format("xCommand CallHistory DeleteEntry CallHistoryId: {0} AcknowledgeConsecutiveDuplicates: True", entry.OccurrenceHistoryId));
        }

        #region IHasCameraSpeakerTrack

        public void CameraAutoModeToggle()
        {
            if (!CameraAutoModeIsOnFeedback.BoolValue)
            {
                EnqueueCommand("xCommand Cameras SpeakerTrack Activate");
            }
            else
            {
                EnqueueCommand("xCommand Cameras SpeakerTrack Deactivate");
            }
        }

        public void CameraAutoModeOn()
        {
            if (CameraIsOffFeedback.BoolValue)
            {
                CameraMuteOff();
            }

            EnqueueCommand("xCommand Cameras SpeakerTrack Activate");
        }

        public void CameraAutoModeOff()
        {
            if (CameraIsOffFeedback.BoolValue)
            {
                CameraMuteOff();
            }

            EnqueueCommand("xCommand Cameras SpeakerTrack Deactivate");
        }

        #endregion

        /// <summary>
        /// Builds the cameras List.  Could later be modified to build from config data
        /// </summary>
        void SetUpCameras(List<CameraInfo> cameraInfo)
        {
            // Add the internal camera
            Cameras = new List<CameraBase>();

            var camCount = CodecStatus.Status.Cameras.Camera.Count;

            // Deal with the case of 1 or no reported cameras
            if (camCount <= 1)
            {
                var internalCamera = new CiscoSparkCamera(Key + "-camera1", "Near End", this, 1);

                if (camCount > 0)
                {
                    // Try to get the capabilities from the codec
                    if (CodecStatus.Status.Cameras.Camera[0] != null && CodecStatus.Status.Cameras.Camera[0].Capabilities != null)
                    {
                        internalCamera.SetCapabilites(CodecStatus.Status.Cameras.Camera[0].Capabilities.Options.Value);
                    }
                }

                Cameras.Add(internalCamera);
                //DeviceManager.AddDevice(internalCamera);
            }
            else
            {
                // Setup all the cameras
                for (int i = 0; i < camCount; i++)
                {
                    var cam = CodecStatus.Status.Cameras.Camera[i];

                    var id = (uint)i;
                    var name = string.Format("Camera {0}", id);

                    // Check for a config object that matches the camera number
                    var camInfo = cameraInfo.FirstOrDefault(c => c.CameraNumber == i + 1);
                    if (camInfo != null)
                    {
                        id = (uint)camInfo.SourceId;
                        name = camInfo.Name;
                    }

                    var key = string.Format("{0}-camera{1}", Key, id);
                    var camera = new CiscoSparkCamera(key, name, this, id);

                    if (cam.Capabilities != null)
                    {
                        camera.SetCapabilites(cam.Capabilities.Options.Value);
                    }

                    Cameras.Add(camera);                    
                }
            }

            // Add the far end camera
            var farEndCamera = new CiscoFarEndCamera(Key + "-cameraFar", "Far End", this);
            Cameras.Add(farEndCamera);

            SelectedCameraFeedback = new StringFeedback(() => SelectedCamera.Key);            

            ControllingFarEndCameraFeedback = new BoolFeedback(() => SelectedCamera is IAmFarEndCamera);            

            NearEndPresets = new List<CodecRoomPreset>(15);

            FarEndRoomPresets = new List<CodecRoomPreset>(15);

            // Add the far end presets
            for (int i = 1; i <= FarEndRoomPresets.Capacity; i++)
            {
                var label = string.Format("Far End Preset {0}", i);
                FarEndRoomPresets.Add(new CodecRoomPreset(i, label, true, false));
            }

            SelectedCamera = Cameras[0]; ; // call the method to select the camera and ensure the feedbacks get updated.

        }

        #region IHasCodecCameras Members

        public event EventHandler<CameraSelectedEventArgs> CameraSelected;

        public List<CameraBase> Cameras { get; private set; }

        public StringFeedback SelectedCameraFeedback { get; private set; }

        private CameraBase _selectedCamera;

        /// <summary>
        /// Returns the selected camera
        /// </summary>
        public CameraBase SelectedCamera
        {
            get
            {
                return _selectedCamera;
            }
            private set
            {
                _selectedCamera = value;
                SelectedCameraFeedback.FireUpdate();
                ControllingFarEndCameraFeedback.FireUpdate();
                if (CameraIsOffFeedback.BoolValue)
                    CameraMuteOff();

                var handler = CameraSelected;
                if (handler != null)
                {
                    handler(this, new CameraSelectedEventArgs(SelectedCamera));
                }
            }
        }

        public void SelectCamera(string key)
        {
            var camera = Cameras.FirstOrDefault(c => c.Key.IndexOf(key, StringComparison.OrdinalIgnoreCase) > -1);
            if (camera != null)
            {
                Debug.Console(2, this, "Selected Camera with key: '{0}'", camera.Key);
                SelectedCamera = camera;
            }
            else
                Debug.Console(2, this, "Unable to select camera with key: '{0}'", key);

            var ciscoCam = camera as CiscoSparkCamera;
            if (ciscoCam != null)
            {
                EnqueueCommand(string.Format("xCommand Video Input SetMainVideoSource SourceId: {0}", ciscoCam.CameraId));
            }
        }

        public CameraBase FarEndCamera { get; private set; }

        public BoolFeedback ControllingFarEndCameraFeedback { get; private set; }

        #endregion

        /// <summary>
		/// 
		/// </summary>
        public class CiscoCodecInfo : VideoCodecInfo
        {
            public CiscoCodecStatus.RootObject CodecStatus { get; private set; }

            public CiscoCodecConfiguration.RootObject CodecConfiguration { get; private set; }

            public override bool MultiSiteOptionIsEnabled
            {
                get
                {
                    if (!string.IsNullOrEmpty(CodecStatus.Status.SystemUnit.Software.OptionKeys.MultiSite.Value) && CodecStatus.Status.SystemUnit.Software.OptionKeys.MultiSite.Value.ToLower() == "true")
                        return true;
                    else
                        return false;
                }

            }
            public override string IpAddress
            {
                get
                {
                    var address = string.Empty;
                    if (CodecConfiguration.Configuration.Network.Count > 0)
                    {
                        if(!string.IsNullOrEmpty(CodecConfiguration.Configuration.Network[0].IPv4.Address.Value))
                            address = CodecConfiguration.Configuration.Network[0].IPv4.Address.Value;
                    }
                    
                    if (string.IsNullOrEmpty(address) && CodecStatus.Status.Network.Count > 0)
                    {
                        if(!string.IsNullOrEmpty(CodecStatus.Status.Network[0].IPv4.Address.Value))
                            address = CodecStatus.Status.Network[0].IPv4.Address.Value;
                    }
                    return address;
                }
            }
            public override string E164Alias
            {
                get 
                {
                    if (CodecConfiguration.Configuration.H323 != null && CodecConfiguration.Configuration.H323.H323Alias.E164 != null)
                    {
                        return CodecConfiguration.Configuration.H323.H323Alias.E164.Value;
                    }
                    else
                    {
                        return string.Empty;
                    }
                }
            }
            public override string H323Id
            {
                get
                {
                    if (CodecConfiguration.Configuration.H323 != null && CodecConfiguration.Configuration.H323.H323Alias != null
                        && CodecConfiguration.Configuration.H323.H323Alias.ID != null)
                    {
                        return CodecConfiguration.Configuration.H323.H323Alias.ID.Value;
                    }
                    else
                    {
                        return string.Empty;
                    }
                }
            }
            public override string SipPhoneNumber
            {
                get
                {
                    if (CodecStatus.Status.SIP != null && CodecStatus.Status.SIP.Registration.Count > 0)
                    {
                        var match = Regex.Match(CodecStatus.Status.SIP.Registration[0].URI.Value, @"(\d+)"); // extract numbers only
                        if (match.Success)
                        {
                            Debug.Console(1, "Extracted phone number as '{0}' from string '{1}'", match.Groups[1].Value, CodecStatus.Status.SIP.Registration[0].URI.Value);
                            return match.Groups[1].Value;
                        }
                        else
                        {
                            Debug.Console(1, "Unable to extract phone number from string: '{0}'", CodecStatus.Status.SIP.Registration[0].URI.Value);
                            return string.Empty;
                        }
                    }
                    else
                    {
                        Debug.Console(1, "Unable to extract phone number. No SIP Registration items found");
                        return string.Empty;
                    }
                }
            }

            public override string SipUri
            {
                get
                {
                    if (CodecStatus.Status.SIP != null && CodecStatus.Status.SIP.AlternateURI.Primary.URI.Value != null)
                    {
                        return CodecStatus.Status.SIP.AlternateURI.Primary.URI.Value;
                    }
                    else if (CodecStatus.Status.UserInterface != null &&
                             CodecStatus.Status.UserInterface.ContactInfo.ContactMethod[0].Number.Value != null)
                    {
                        return CodecStatus.Status.UserInterface.ContactInfo.ContactMethod[0].Number.Value;
                    }
                    else
                        return string.Empty;
                }
            }

            public override bool AutoAnswerEnabled
            {
                get
                {
                    if (CodecConfiguration.Configuration.Conference.AutoAnswer.Mode.Value == null) return false;
                    return CodecConfiguration.Configuration.Conference.AutoAnswer.Mode.Value.ToLower() == "on";
                }
            }

            public CiscoCodecInfo(CiscoCodecStatus.RootObject status, CiscoCodecConfiguration.RootObject configuration)
            {
                CodecStatus = status;
                CodecConfiguration = configuration;
            }
        }


        #region IHasCameraPresets Members

        public event EventHandler<EventArgs> CodecRoomPresetsListHasChanged;

        public List<CodecRoomPreset> NearEndPresets { get; private set; }

        public List<CodecRoomPreset> FarEndRoomPresets { get; private set; }

        public void CodecRoomPresetSelect(int preset)
        {
            Debug.Console(1, this, "Selecting Preset: {0}", preset);
            if (SelectedCamera is IAmFarEndCamera)
                SelectFarEndPreset(preset);
            else
                EnqueueCommand(string.Format("xCommand RoomPreset Activate PresetId: {0}", preset));
        }

        public void CodecRoomPresetStore(int preset, string description)
        {
            EnqueueCommand(string.Format("xCommand RoomPreset Store PresetId: {0} Description: \"{1}\" Type: All", preset, description));
        }

        #endregion

        public void SelectFarEndPreset(int preset)
        {
            EnqueueCommand(string.Format("xCommand Call FarEndControl RoomPreset Activate CallId: {0} PresetId: {1}", GetCallId(), preset));
        }


		#region IHasExternalSourceSwitching Members

		/// <summary>
		/// Wheather the Cisco supports External Source Lists or not 
		/// </summary>
		public bool ExternalSourceListEnabled
		{
			get;
			private set; 
		}

        /// <summary>
        /// The name of the RoutingInputPort to which the upstream external switcher is connected
        /// </summary>
        public string ExternalSourceInputPort { get;  private set; }

        public bool BrandingEnabled { get; private set; }
        private string _brandingUrl;
        private bool _sendMcUrl;

		/// <summary>
		/// Adds an external source to the Cisco 
		/// </summary>
		/// <param name="connectorId"></param>
		/// <param name="key"></param>
		/// <param name="name"></param>
		public void AddExternalSource(string connectorId, string key, string name, eExternalSourceType type)
		{
			int id = 2;
			if (connectorId.ToLower() == "hdmiin3")
			{
				id = 3;
			}
			EnqueueCommand(string.Format("xCommand UserInterface Presentation ExternalSource Add ConnectorId: {0} SourceIdentifier: \"{1}\" Name: \"{2}\" Type: {3}", id, key, name, type.ToString()));
			// SendText(string.Format("xCommand UserInterface Presentation ExternalSource State Set SourceIdentifier: \"{0}\" State: Ready", key));
			Debug.Console(2, this, "Adding ExternalSource {0} {1}", connectorId, name);

		}


		/// <summary>
		/// Sets the state of the External Source 
		/// </summary>
		/// <param name="key"></param>
		/// <param name="mode"></param>
		public void SetExternalSourceState(string key, eExternalSourceMode mode)
		{
			EnqueueCommand(string.Format("xCommand UserInterface Presentation ExternalSource State Set SourceIdentifier: \"{0}\" State: {1}", key, mode.ToString()));
		}
		/// <summary>
		/// Clears all external sources on the codec
		/// </summary>
		public void ClearExternalSources()
		{
			EnqueueCommand("xCommand UserInterface Presentation ExternalSource RemoveAll");
			
		}

        /// <summary>
        /// Sets the selected source of the available external sources on teh Touch10 UI
        /// </summary>
        public void SetSelectedSource(string key)
        {
            EnqueueCommand(string.Format("xCommand UserInterface Presentation ExternalSource Select SourceIdentifier: {0}", key));
            _externalSourceChangeRequested = true;
        }

		/// <summary>
		/// Action that will run when the External Source is selected. 
		/// </summary>
		public Action<string, string> RunRouteAction { private get;  set; }
				





		#endregion
		#region ExternalDevices 


		
		#endregion

        #region IHasCameraOff Members

        public BoolFeedback CameraIsOffFeedback { get; private set; }

        public void CameraOff()
        {
            CameraMuteOn();
        }

        #endregion

        public BoolFeedback CameraIsMutedFeedback { get; private set; }

        /// <summary>
        /// Mutes the outgoing camera video
        /// </summary>
        public void CameraMuteOn()
        {
            EnqueueCommand("xCommand Video Input MainVideo Mute");
        }

        /// <summary>
        /// Unmutes the outgoing camera video
        /// </summary>
        public void CameraMuteOff()
        {
            EnqueueCommand("xCommand Video Input MainVideo Unmute");
        }

        /// <summary>
        /// Toggles the camera mute state
        /// </summary>
        public void CameraMuteToggle()
        {
            if (CameraIsMutedFeedback.BoolValue)
                CameraMuteOff();
            else
                CameraMuteOn();
        }

        #region IHasDoNotDisturbMode Members

        public BoolFeedback DoNotDisturbModeIsOnFeedback { get; private set; }

        public void ActivateDoNotDisturbMode()
        {
            EnqueueCommand("xCommand Conference DoNotDisturb Activate");
        }

        public void DeactivateDoNotDisturbMode()
        {
            EnqueueCommand("xCommand Conference DoNotDisturb Deactivate");
        }

        public void ToggleDoNotDisturbMode()
        {
            if (DoNotDisturbModeIsOnFeedback.BoolValue)
            {
                DeactivateDoNotDisturbMode();
            }
            else
            {
                ActivateDoNotDisturbMode();
            }
        }

        #endregion

        #region IHasHalfWakeMode Members

        public BoolFeedback HalfWakeModeIsOnFeedback { get; private set; }

        public BoolFeedback EnteringStandbyModeFeedback { get; private set; }

        public void HalfwakeActivate()
        {
            EnqueueCommand("xCommand Standby Halfwake");
        }

        #endregion
    }



    /// <summary>
    /// Tracks the initial sycnronization state of the codec when making a connection
    /// </summary>
    public class CodecSyncState : IKeyed
    {
        bool _InitialSyncComplete;
        private readonly CiscoSparkCodec _parent;

        public event EventHandler<EventArgs> InitialSyncCompleted;
        private readonly CrestronQueue<string> _commandQueue;

        public string Key { get; private set; }

        public bool InitialSyncComplete 
        {
            get { return _InitialSyncComplete; }
            private set
            {
                if (value == true)
                {
                    var handler = InitialSyncCompleted;
                    if (handler != null)
                        handler(this, new EventArgs());
                }
                _InitialSyncComplete = value;
            }
        }

        public bool LoginMessageWasReceived { get; private set; }

        public bool JsonResponseModeSet { get; private set; }

        public bool InitialStatusMessageWasReceived { get; private set; }

        public bool InitialConfigurationMessageWasReceived { get; private set; }

        public bool FeedbackWasRegistered { get; private set; }

        public CodecSyncState(string key, CiscoSparkCodec parent)
        {
            Key = key;
            _parent = parent;
            _commandQueue = new CrestronQueue<string>(50);
            CodecDisconnected();
        }

        private void ProcessQueuedCommands()
        {
            while (InitialSyncComplete)
            {
                var query = _commandQueue.Dequeue();

                _parent.SendText(query);
            }
        }

        public void AddCommandToQueue(string query)
        {
            _commandQueue.Enqueue(query);
        }

        public void LoginMessageReceived()
        {
            LoginMessageWasReceived = true;
            Debug.Console(1, this, "Login Message Received.");
            CheckSyncStatus();
        }

        public void JsonResponseModeMessageReceived()
        {
            JsonResponseModeSet = true;
            Debug.Console(1, this, "Json Response Mode Message Received.");
            CheckSyncStatus();
        }

        public void InitialStatusMessageReceived()
        {
            InitialStatusMessageWasReceived = true;
            Debug.Console(1, this, "Initial Codec Status Message Received.");
            CheckSyncStatus();
        }

        public void InitialConfigurationMessageReceived()
        {
            InitialConfigurationMessageWasReceived = true;
            Debug.Console(1, this, "Initial Codec Configuration Message Received.");
            CheckSyncStatus();
        }

        public void FeedbackRegistered()
        {
            FeedbackWasRegistered = true;
            Debug.Console(1, this, "Initial Codec Feedback Registration Successful.");
            CheckSyncStatus();
        }

        public void CodecDisconnected()
        {
            _commandQueue.Clear();
            LoginMessageWasReceived = false;
            JsonResponseModeSet = false;
            InitialConfigurationMessageWasReceived = false;
            InitialStatusMessageWasReceived = false;
            FeedbackWasRegistered = false;
            InitialSyncComplete = false;
        }

        void CheckSyncStatus()
        {
            if (LoginMessageWasReceived && JsonResponseModeSet  && InitialConfigurationMessageWasReceived && InitialStatusMessageWasReceived && FeedbackWasRegistered)
            {
                InitialSyncComplete = true;
                Debug.Console(1, this, "Initial Codec Sync Complete!");
                Debug.Console(1, this, "{0} Command queued. Processing now...", _commandQueue.Count);

                // Invoke a thread for the queue
                CrestronInvoke.BeginInvoke((o) => { 
                    ProcessQueuedCommands();
                });
            }
            else
                InitialSyncComplete = false;
        }
    }

    public class CiscoSparkCodecFactory : EssentialsDeviceFactory<CiscoSparkCodec>
    {
        public CiscoSparkCodecFactory()
        {
            TypeNames = new List<string>() { "ciscospark", "ciscowebex", "ciscowebexpro", "ciscoroomkit", "ciscosparkpluscodec" };
        }

        public override EssentialsDevice BuildDevice(DeviceConfig dc)
        {
            Debug.Console(1, "Factory Attempting to create new Cisco Codec Device");

            var comm = CommFactory.CreateCommForDevice(dc);
            return new VideoCodec.Cisco.CiscoSparkCodec(dc, comm);
        }
    }

}