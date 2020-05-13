using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using Crestron.SimplSharp;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

using PepperDash.Core;
using PepperDash.Essentials.Core;
using PepperDash.Essentials.Core.Config;
using PepperDash.Essentials.Core.Routing;
using PepperDash.Essentials.Devices.Common.Cameras;
using PepperDash.Essentials.Devices.Common.Codec;
using PepperDash.Essentials.Devices.Common.Occupancy;
using PepperDash.Essentials.Devices.Common.VideoCodec;

namespace PepperDash.Essentials.Devices.Common.VideoCodec.Cisco
{
    enum eCommandType { SessionStart, SessionEnd, Command, GetStatus, GetConfiguration };

    public class CiscoSparkCodec : VideoCodecBase, IHasCallHistory, IHasCallFavorites, IHasDirectory,
        IHasScheduleAwareness, IOccupancyStatusProvider, IHasCodecLayouts, IHasCodecSelfView,
        ICommunicationMonitor, IRouting, IHasCodecCameras, IHasCameraAutoMode, IHasCodecRoomPresets
    {
        public event EventHandler<DirectoryEventArgs> DirectoryResultReturned;

        public CommunicationGather PortGather { get; private set; }

        public StatusMonitorBase CommunicationMonitor { get; private set; }

		public BoolFeedback PresentationViewMaximizedFeedback { get; private set; }

		string CurrentPresentationView;

        public BoolFeedback RoomIsOccupiedFeedback { get; private set; }

        public IntFeedback PeopleCountFeedback { get; private set; }

        public BoolFeedback CameraAutoModeIsOnFeedback { get; private set; }

        public BoolFeedback SelfviewIsOnFeedback { get; private set; }

        public StringFeedback SelfviewPipPositionFeedback { get; private set; }

        public StringFeedback LocalLayoutFeedback { get; private set; }

        public BoolFeedback LocalLayoutIsProminentFeedback { get; private set; }

        public BoolFeedback FarEndIsSharingContentFeedback { get; private set; }

        private CodecCommandWithLabel CurrentSelfviewPipPosition;

        private CodecCommandWithLabel CurrentLocalLayout;

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
                return () => PresentationSourceKey;
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
                return () => CurrentSelfviewPipPosition.Label;
            }
        }

        protected Func<string> LocalLayoutFeedbackFunc
        {
            get
            {
                return () => CurrentLocalLayout.Label;
            }
        }

        protected Func<bool> LocalLayoutIsProminentFeedbackFunc
        {
            get
            {
                return () => CurrentLocalLayout.Label == "Prominent";
            }
        }


        private string CliFeedbackRegistrationExpression;

        private CodecSyncState SyncState;

        public CodecPhonebookSyncState PhonebookSyncState { get; private set; }

        private StringBuilder JsonMessage;

        private bool JsonFeedbackMessageIsIncoming;

        public bool CommDebuggingIsOn;

        string Delimiter = "\r\n";

        /// <summary>
        /// Used to track the current connector used for the presentation source
        /// </summary>
        int PresentationSource;

        string PresentationSourceKey;

        string PhonebookMode = "Local"; // Default to Local

        int PhonebookResultsLimit = 255; // Could be set later by config.

        CTimer LoginMessageReceivedTimer;
		CTimer RetryConnectionTimer;

        // **___________________________________________________________________**
        //  Timers to be moved to the global system timer at a later point....
        CTimer BookingsRefreshTimer;
        CTimer PhonebookRefreshTimer;
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

            RoomIsOccupiedFeedback = new BoolFeedback(RoomIsOccupiedFeedbackFunc);
            PeopleCountFeedback = new IntFeedback(PeopleCountFeedbackFunc);
            CameraAutoModeIsOnFeedback = new BoolFeedback(SpeakerTrackIsOnFeedbackFunc);
            SelfviewIsOnFeedback = new BoolFeedback(SelfViewIsOnFeedbackFunc);
            SelfviewPipPositionFeedback = new StringFeedback(SelfviewPipPositionFeedbackFunc);
            LocalLayoutFeedback = new StringFeedback(LocalLayoutFeedbackFunc);
            LocalLayoutIsProminentFeedback = new BoolFeedback(LocalLayoutIsProminentFeedbackFunc);
			FarEndIsSharingContentFeedback = new BoolFeedback(FarEndIsSharingContentFeedbackFunc);

			PresentationViewMaximizedFeedback = new BoolFeedback(() => CurrentPresentationView == "Maximized");

            Communication = comm;

            if (props.CommunicationMonitorProperties != null)
            {
                CommunicationMonitor = new GenericCommunicationMonitor(this, Communication, props.CommunicationMonitorProperties);
            }
            else
            {
                CommunicationMonitor = new GenericCommunicationMonitor(this, Communication, 30000, 120000, 300000, "xStatus SystemUnit Software Version\r");
            }

            if (props.Sharing != null)
                AutoShareContentWhileInCall = props.Sharing.AutoShareContentWhileInCall;

            ShowSelfViewByDefault = props.ShowSelfViewByDefault;

            DeviceManager.AddDevice(CommunicationMonitor);

            PhonebookMode = props.PhonebookMode;

            SyncState = new CodecSyncState(Key + "--Sync");

            PhonebookSyncState = new CodecPhonebookSyncState(Key + "--PhonebookSync");

            SyncState.InitialSyncCompleted += new EventHandler<EventArgs>(SyncState_InitialSyncCompleted);

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
            CodecStatus.Status.Audio.Volume.ValueChangedAction = VolumeLevelFeedback.FireUpdate;
            CodecStatus.Status.Audio.VolumeMute.ValueChangedAction = MuteFeedback.FireUpdate;
            CodecStatus.Status.Audio.Microphones.Mute.ValueChangedAction = PrivacyModeIsOnFeedback.FireUpdate;
            CodecStatus.Status.Standby.State.ValueChangedAction = StandbyIsOnFeedback.FireUpdate;
            CodecStatus.Status.RoomAnalytics.PeoplePresence.ValueChangedAction = RoomIsOccupiedFeedback.FireUpdate;
            CodecStatus.Status.RoomAnalytics.PeopleCount.Current.ValueChangedAction = PeopleCountFeedback.FireUpdate;
            CodecStatus.Status.Cameras.SpeakerTrack.Status.ValueChangedAction = CameraAutoModeIsOnFeedback.FireUpdate;
            CodecStatus.Status.Video.Selfview.Mode.ValueChangedAction = SelfviewIsOnFeedback.FireUpdate;
            CodecStatus.Status.Video.Selfview.PIPPosition.ValueChangedAction = ComputeSelfviewPipStatus;
            CodecStatus.Status.Video.Layout.LayoutFamily.Local.ValueChangedAction = ComputeLocalLayout;
            CodecStatus.Status.Conference.Presentation.Mode.ValueChangedAction += SharingContentIsOnFeedback.FireUpdate;
            CodecStatus.Status.Conference.Presentation.Mode.ValueChangedAction += FarEndIsSharingContentFeedback.FireUpdate;

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

            SetUpCameras();

			CreateOsdSource();
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

        /// <summary>
        /// Starts the HTTP feedback server and syncronizes state of codec
        /// </summary>
        /// <returns></returns>
        public override bool CustomActivate()
        {
            CrestronConsole.AddNewConsoleCommand(SetCommDebug, "SetCodecCommDebug", "0 for Off, 1 for on", ConsoleAccessLevelEnum.AccessOperator);
            CrestronConsole.AddNewConsoleCommand(GetPhonebook, "GetCodecPhonebook", "Triggers a refresh of the codec phonebook", ConsoleAccessLevelEnum.AccessOperator);
            CrestronConsole.AddNewConsoleCommand(GetBookings, "GetCodecBookings", "Triggers a refresh of the booking data for today", ConsoleAccessLevelEnum.AccessOperator);

            var socket = Communication as ISocketStatus;
            if (socket != null)
            {
                socket.ConnectionChange += new EventHandler<GenericSocketStatusChageEventArgs>(socket_ConnectionChange);
            }

            Communication.Connect();

			CommunicationMonitor.Start();

            string prefix = "xFeedback register ";

            CliFeedbackRegistrationExpression =
                prefix + "/Configuration" + Delimiter +
                prefix + "/Status/Audio" + Delimiter +
                prefix + "/Status/Call" + Delimiter +
                prefix + "/Status/Conference/Presentation" + Delimiter +
                prefix + "/Status/Cameras/SpeakerTrack" + Delimiter +
                prefix + "/Status/RoomAnalytics" + Delimiter +
                prefix + "/Status/RoomPreset" + Delimiter +
                prefix + "/Status/Standby" + Delimiter +
                prefix + "/Status/Video/Selfview" + Delimiter +
                prefix + "/Status/Video/Layout" + Delimiter +
                prefix + "/Bookings" + Delimiter +
                prefix + "/Event/CallDisconnect" + Delimiter + 
                prefix + "/Event/Bookings" + Delimiter +
                prefix + "/Event/CameraPresetListUpdated" + Delimiter;

            return base.CustomActivate();
        }

        /// <summary>
        /// Fires when initial codec sync is completed.  Used to then send commands to get call history, phonebook, bookings, etc.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void SyncState_InitialSyncCompleted(object sender, EventArgs e)
        {
            // Fire the ready event
            SetIsReady();
            //CommDebuggingIsOn = false;

            GetCallHistory();

            PhonebookRefreshTimer = new CTimer(CheckCurrentHour, 3600000, 3600000);     // check each hour to see if the phonebook should be downloaded
            GetPhonebook(null);

            BookingsRefreshTimer = new CTimer(GetBookings, 900000,  900000);       // 15 minute timer to check for new booking info
            GetBookings(null);
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
                if(!SyncState.LoginMessageWasReceived)
				    LoginMessageReceivedTimer = new CTimer(o => DisconnectClientAndReconnect(), 5000);
            }
            else
            {
                SyncState.CodecDisconnected();
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

			RetryConnectionTimer = new CTimer(o => Communication.Connect(), 2000);

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
                if(!JsonFeedbackMessageIsIncoming)
                    Debug.Console(1, this, "RX: '{0}'", args.Text);
            }

            if (args.Text == "{" + Delimiter)        // Check for the beginning of a new JSON message
            {
                JsonFeedbackMessageIsIncoming = true;

                if (CommDebuggingIsOn)
                    Debug.Console(1, this, "Incoming JSON message...");

                JsonMessage = new StringBuilder();
            }
            else if (args.Text == "}" + Delimiter)  // Check for the end of a JSON message
            {
                JsonFeedbackMessageIsIncoming = false;

                JsonMessage.Append(args.Text);

                if (CommDebuggingIsOn)
                    Debug.Console(1, this, "Complete JSON Received:\n{0}", JsonMessage.ToString());

                // Forward the complete message to be deserialized
                DeserializeResponse(JsonMessage.ToString());
                return;
            }

            if(JsonFeedbackMessageIsIncoming)
            {
                JsonMessage.Append(args.Text);

                //Debug.Console(1, this, "Building JSON:\n{0}", JsonMessage.ToString());
                return;
            }

            if (!SyncState.InitialSyncComplete)
            {
                switch (args.Text.Trim().ToLower()) // remove the whitespace
                {
                    case "*r login successful":
                        {
                            SyncState.LoginMessageReceived();

                            if(LoginMessageReceivedTimer != null)
                                LoginMessageReceivedTimer.Stop();

                            SendText("xPreferences outputmode json");
                            break;
                        }
                    case "xpreferences outputmode json":
                        {
                            if (!SyncState.InitialStatusMessageWasReceived)
                                SendText("xStatus");
                            break;
                        }
                    case "xfeedback register /event/calldisconnect":
                        {
                            SyncState.FeedbackRegistered();
                            break;
                        }
                }
            }
                
        }

        public void SendText(string command)
        {
            if (CommDebuggingIsOn)
                Debug.Console(1, this, "Sending: '{0}'", command);

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

                if (response.IndexOf("\"Status\":{") > -1)
                {
                    // Status Message

                    // Temp object so we can inpsect for call data before simply deserializing
                    CiscoCodecStatus.RootObject tempCodecStatus = new CiscoCodecStatus.RootObject();

                    JsonConvert.PopulateObject(response, tempCodecStatus);

                    // Check to see if the message contains /Status/Conference/Presentation/LocalInstance and extract source value 
                    var conference = tempCodecStatus.Status.Conference;

                    if (conference.Presentation.LocalInstance.Count > 0)
                    {
                        if (!string.IsNullOrEmpty(conference.Presentation.LocalInstance[0].ghost))
                            PresentationSource = 0;
                        else if (conference.Presentation.LocalInstance[0].Source != null)
                        {
                            PresentationSource = conference.Presentation.LocalInstance[0].Source.IntValue;
                        }
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
                                    Direction = CodecCallDirection.ConvertToDirectionEnum(call.Direction.Value)
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
                        List<CiscoCodecStatus.RoomPreset> existingRoomPresets = new List<CiscoCodecStatus.RoomPreset>();
                        // Add the existing items to the temporary list
                        existingRoomPresets.AddRange(CodecStatus.Status.RoomPreset);
                        // Populate the CodecStatus object (this will append new values to the RoomPreset collection
                        JsonConvert.PopulateObject(response, CodecStatus);

                        JObject jResponse = JObject.Parse(response);

                        IList<JToken> roomPresets = jResponse["Status"]["RoomPreset"].Children().ToList();
                        // Iterate the new items in this response agains the temporary list.  Overwrite any existing items and add new ones.
                        foreach (var preset in tempPresets)
                        {
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
                        NearEndPresets = RoomPresets.GetGenericPresets(CodecStatus.Status.RoomPreset);

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

                    if (!SyncState.InitialStatusMessageWasReceived)
                    {
                        SyncState.InitialStatusMessageReceived();

                        if (!SyncState.InitialConfigurationMessageWasReceived)
                            SendText("xConfiguration");
                    }
                }
                else if (response.IndexOf("\"Configuration\":{") > -1)
                {
                    // Configuration Message

                    JsonConvert.PopulateObject(response, CodecConfiguration);

                    if (!SyncState.InitialConfigurationMessageWasReceived)
                    {
                        SyncState.InitialConfigurationMessageReceived();
                        if (!SyncState.FeedbackWasRegistered)
                        {
                            SendText(CliFeedbackRegistrationExpression);
                        }
                    }

                }
                else if (response.IndexOf("\"Event\":{") > -1)
                {
                    if (response.IndexOf("\"CallDisconnect\":{") > -1)
                    {
                        CiscoCodecEvents.RootObject eventReceived = new CiscoCodecEvents.RootObject();

                        JsonConvert.PopulateObject(response, eventReceived);

                        EvalutateDisconnectEvent(eventReceived);
                    }
					else if (response.IndexOf("\"Bookings\":{") > -1) // The list has changed, reload it
					{
						GetBookings(null);
					}
                }
                else if (response.IndexOf("\"CommandResponse\":{") > -1)
                {
                    // CommandResponse Message

                    if (response.IndexOf("\"CallHistoryRecentsResult\":{") > -1)
                    {
                        var codecCallHistory = new CiscoCallHistory.RootObject();

                        JsonConvert.PopulateObject(response, codecCallHistory);

                        CallHistory.ConvertCiscoCallHistoryToGeneric(codecCallHistory.CommandResponse.CallHistoryRecentsResult.Entry);
                    }
                    else if (response.IndexOf("\"CallHistoryDeleteEntryResult\":{") > -1)
                    {
                        GetCallHistory();
                    }
                    else if (response.IndexOf("\"PhonebookSearchResult\":{") > -1)
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
			PresentationSourceKey = selector.ToString();
		}

		/// <summary>
		/// This is necessary for devices that are "routers" in the middle of the path, even though it only has one output and 
		/// may only have one input.
		/// </summary>
        public void ExecuteSwitch(object inputSelector, object outputSelector, eRoutingSignalType signalType)
        {
			ExecuteSwitch(inputSelector);
            PresentationSourceKey = inputSelector.ToString();
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
            SendText("xCommand CallHistory Recents Limit: 20 Order: OccurrenceTime");
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

            SendText("xCommand Bookings List Days: 1 DayOffset: 0");
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
            SendText(string.Format("xCommand Phonebook Search PhonebookType: {0} ContactType: Folder", PhonebookMode));
        }

        private void GetPhonebookContacts()
        {
            // Get Phonebook Folders (determine local/corporate from config, and set results limit)
            SendText(string.Format("xCommand Phonebook Search PhonebookType: {0} ContactType: Contact Limit: {1}", PhonebookMode, PhonebookResultsLimit));
        }

        /// <summary>
        /// Searches the codec phonebook for all contacts matching the search string
        /// </summary>
        /// <param name="searchString"></param>
        public void SearchDirectory(string searchString)
        {
            SendText(string.Format("xCommand Phonebook Search SearchString: \"{0}\" PhonebookType: {1} ContactType: Contact Limit: {2}", searchString, PhonebookMode, PhonebookResultsLimit));
        }

        /// <summary>
        /// // Get contents of a specific folder in the phonebook
        /// </summary>
        /// <param name="folderId"></param>
        public void GetDirectoryFolderContents(string folderId)
        {
            SendText(string.Format("xCommand Phonebook Search FolderId: {0} PhonebookType: {1} ContactType: Any Limit: {2}", folderId, PhonebookMode, PhonebookResultsLimit));
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
         	SendText(string.Format("xCommand Dial Number: \"{0}\"", number));
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
            SendText(string.Format("xCommand Dial Number: \"{0}\" Protocol: {1} CallRate: {2} CallType: {3} BookingId: {4}", number, protocol, callRate, callType, meetingId));
        }
 
        public override void EndCall(CodecActiveCallItem activeCall)
        {
            SendText(string.Format("xCommand Call Disconnect CallId: {0}", activeCall.Id));
        }

        public override void EndAllCalls()
        {
            foreach (CodecActiveCallItem activeCall in ActiveCalls)
            {
                SendText(string.Format("xCommand Call Disconnect CallId: {0}", activeCall.Id));
            }
        }

        public override void AcceptCall(CodecActiveCallItem item)
        {
            SendText("xCommand Call Accept");
        }

        public override void RejectCall(CodecActiveCallItem item)
        {
            SendText("xCommand Call Reject");
        }

        public override void SendDtmf(string s)
        {
            if (CallFavorites != null)
            {
                SendText(string.Format("xCommand Call DTMFSend CallId: {0} DTMFString: \"{1}\"", GetCallId(), s));
            }
        }

        public void SelectPresentationSource(int source)
        {
            PresentationSource = source;

            StartSharing();
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

            if(PresentationSource > 0)
                SendText(string.Format("xCommand Presentation Start PresentationSource: {0} SendingMode: {1}", PresentationSource, sendingMode));
        }

        /// <summary>
        /// Stops sharing the current presentation
        /// </summary>
        public override void StopSharing()
        {
            PresentationSource = 0;

            SendText("xCommand Presentation Stop");
        }

        public override void PrivacyModeOn()
        {
            SendText("xCommand Audio Microphones Mute");
        }

        public override void PrivacyModeOff()
        {
            SendText("xCommand Audio Microphones Unmute");
        }

        public override void PrivacyModeToggle()
        {
            SendText("xCommand Audio Microphones ToggleMute");
        }

        public override void MuteOff()
        {
            SendText("xCommand Audio Volume Unmute");
        }

        public override void MuteOn()
        {
            SendText("xCommand Audio Volume Mute");
        }

        public override void MuteToggle()
        {
            SendText("xCommand Audio Volume ToggleMute");
        }

        /// <summary>
        /// Increments the voluem
        /// </summary>
        /// <param name="pressRelease"></param>
        public override void VolumeUp(bool pressRelease)
        {
            SendText("xCommand Audio Volume Increase");
        }

        /// <summary>
        /// Decrements the volume
        /// </summary>
        /// <param name="pressRelease"></param>
        public override void VolumeDown(bool pressRelease)
        {
            SendText("xCommand Audio Volume Decrease");
        }

        /// <summary>
        /// Scales the level and sets the codec to the specified level within its range
        /// </summary>
        /// <param name="level">level from slider (0-65535 range)</param>
        public override void SetVolume(ushort level)
        {
            var scaledLevel = CrestronEnvironment.ScaleWithLimits(level, 65535, 0, 100, 0); 
            SendText(string.Format("xCommand Audio Volume Set Level: {0}", scaledLevel));
        }

        /// <summary>
        /// Recalls the default volume on the codec
        /// </summary>
        public void VolumeSetToDefault()
        {
            SendText("xCommand Audio Volume SetToDefault");
        }

        /// <summary>
        /// Puts the codec in standby mode
        /// </summary>
        public override void StandbyActivate()
        {
            SendText("xCommand Standby Activate");
        }

        /// <summary>
        /// Wakes the codec from standby
        /// </summary>
        public override void StandbyDeactivate()
        {
            SendText("xCommand Standby Deactivate");
        }

        /// <summary>
        /// Reboots the codec
        /// </summary>
        public void Reboot()
        {
            SendText("xCommand SystemUnit Boot Action: Restart");
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
            SendText("xCommand Video Selfview Set Mode: On");
        }

        /// <summary>
        /// Turns off Selfview Mode
        /// </summary>
        public void SelfViewModeOff()
        {
            SendText("xCommand Video Selfview Set Mode: Off");
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

            SendText(string.Format("xCommand Video Selfview Set Mode: {0}", mode));                    
        }

        /// <summary>
        /// Sets a specified position for the selfview PIP window
        /// </summary>
        /// <param name="position"></param>
        public void SelfviewPipPositionSet(CodecCommandWithLabel position)
        {
            SendText(string.Format("xCommand Video Selfview Set Mode: On PIPPosition: {0}", position.Command));
        }

        /// <summary>
        /// Toggles to the next selfview PIP position
        /// </summary>
        public void SelfviewPipPositionToggle()
        {
            if (CurrentSelfviewPipPosition != null)
            {
                var nextPipPositionIndex = SelfviewPipPositions.IndexOf(CurrentSelfviewPipPosition) + 1;

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
            SendText(string.Format("xCommand Video Layout LayoutFamily Set Target: local LayoutFamily: {0}", layout.Command));
        }

        /// <summary>
        /// Toggles to the next local layout
        /// </summary>
        public void LocalLayoutToggle()
        {
            if(CurrentLocalLayout != null)
            {
                var nextLocalLayoutIndex = LocalLayouts.IndexOf(CurrentLocalLayout) + 1;

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
            if (CurrentLocalLayout != null)
            {
                if (CurrentLocalLayout.Label != "Prominent")
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
				CurrentPresentationView = "Minimized";
			else
				CurrentPresentationView = "Maximized";

			SendText(string.Format("xCommand Video PresentationView Set View:  {0}", CurrentPresentationView));
			PresentationViewMaximizedFeedback.FireUpdate();
		}

        /// <summary>
        /// Calculates the current selfview PIP position
        /// </summary>
        void ComputeSelfviewPipStatus()
        {
            CurrentSelfviewPipPosition = SelfviewPipPositions.FirstOrDefault(p => p.Command.ToLower().Equals(CodecStatus.Status.Video.Selfview.PIPPosition.Value.ToLower()));

            if(CurrentSelfviewPipPosition != null)
                SelfviewIsOnFeedback.FireUpdate();
        }

        /// <summary>
        /// Calculates the current local Layout
        /// </summary>
        void ComputeLocalLayout()
        {
            CurrentLocalLayout = LocalLayouts.FirstOrDefault(l => l.Command.ToLower().Equals(CodecStatus.Status.Video.Layout.LayoutFamily.Local.Value.ToLower()));

            if (CurrentLocalLayout != null)
                LocalLayoutFeedback.FireUpdate();
        }

        public void RemoveCallHistoryEntry(CodecCallHistory.CallHistoryEntry entry)
        {
            SendText(string.Format("xCommand CallHistory DeleteEntry CallHistoryId: {0} AcknowledgeConsecutiveDuplicates: True", entry.OccurrenceHistoryId));
        }

        #region IHasCameraSpeakerTrack

        public void CameraAutoModeToggle()
        {
            if (!CameraAutoModeIsOnFeedback.BoolValue)
            {
                SendText("xCommand Cameras SpeakerTrack Activate");
            }
            else
            {
                SendText("xCommand Cameras SpeakerTrack Deactivate");
            }
        }

        public void CameraAutoModeOn()
        {
            SendText("xCommand Cameras SpeakerTrack Activate");
        }

        public void CameraAutoModeOff()
        {
            SendText("xCommand Cameras SpeakerTrack Deactivate");
        }

        #endregion

        /// <summary>
        /// Builds the cameras List.  Could later be modified to build from config data
        /// </summary>
        void SetUpCameras()
        {
            // Add the internal camera
            Cameras = new List<CameraBase>();

            var internalCamera = new CiscoSparkCamera(Key + "-camera1", "Near End", this, 1);

            if(CodecStatus.Status.Cameras.Camera.Count > 0)
                internalCamera.SetCapabilites(CodecStatus.Status.Cameras.Camera[0].Capabilities.Options.Value);
            else
                // Somehow subscribe to the event on the Options.Value property and update when it changes.

            Cameras.Add(internalCamera);

            // Add the far end camera
            var farEndCamera = new CiscoFarEndCamera(Key + "-cameraFar", "Far End", this);
            Cameras.Add(farEndCamera);

            SelectedCameraFeedback = new StringFeedback(() => SelectedCamera.Key);

            ControllingFarEndCameraFeedback = new BoolFeedback(() => SelectedCamera is IAmFarEndCamera);

            DeviceManager.AddDevice(internalCamera);
            DeviceManager.AddDevice(farEndCamera);

            NearEndPresets = new List<CodecRoomPreset>(15);

            FarEndRoomPresets = new List<CodecRoomPreset>(15);

            // Add the far end presets
            for (int i = 1; i <= FarEndRoomPresets.Capacity; i++)
            {
                var label = string.Format("Far End Preset {0}", i);
                FarEndRoomPresets.Add(new CodecRoomPreset(i, label, true, false));
            }

            SelectedCamera = internalCamera; ; // call the method to select the camera and ensure the feedbacks get updated.
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
                    if (CodecConfiguration.Configuration.Network != null)   
                    {
                        if (CodecConfiguration.Configuration.Network.Count > 0)
                            return CodecConfiguration.Configuration.Network[0].IPv4.Address.Value;
                    }
                    return string.Empty;
                }
            }
            public override string E164Alias
            {
                get 
                {
                    if (CodecConfiguration.Configuration.H323.H323Alias.E164 != null)
                        return CodecConfiguration.Configuration.H323.H323Alias.E164.Value;
                    else
                        return string.Empty;
                }
            }
            public override string H323Id
            {
                get
                {
                    if (CodecConfiguration.Configuration.H323.H323Alias.ID != null)
                        return CodecConfiguration.Configuration.H323.H323Alias.ID.Value;
                    else
                        return string.Empty;
                }
            }
            public override string SipPhoneNumber
            {
                get
                {
                    if (CodecStatus.Status.SIP.Registration.Count > 0)
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
                    if (CodecStatus.Status.SIP.AlternateURI.Primary.URI.Value != null)
                        return CodecStatus.Status.SIP.AlternateURI.Primary.URI.Value;
                    else
                        return string.Empty;
                }
            }
            public override bool AutoAnswerEnabled
            {
                get
                {
                    if (CodecConfiguration.Configuration.Conference.AutoAnswer.Mode.Value.ToLower() == "on")
                        return true;
                    else
                        return false;
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
                SendText(string.Format("xCommand RoomPreset Activate PresetId: {0}", preset));
        }

        public void CodecRoomPresetStore(int preset, string description)
        {
            SendText(string.Format("xCommand RoomPreset Store PresetId: {0} Description: \"{1}\" Type: All", preset, description));
        }

        #endregion

        public void SelectFarEndPreset(int preset)
        {
            SendText(string.Format("xCommand Call FarEndControl RoomPreset Activate CallId: {0} PresetId: {1}", GetCallId(), preset));
        }

    }

    /// <summary>
    /// Represents a codec command that might need to have a friendly label applied for UI feedback purposes
    /// </summary>
    public class CodecCommandWithLabel
    {
        public string Command { get; set; }
        public string Label { get; set; }

        public CodecCommandWithLabel(string command, string label)
        {
            Command = command;
            Label = label;
        }
    }

    /// <summary>
    /// Tracks the initial sycnronization state of the codec when making a connection
    /// </summary>
    public class CodecSyncState : IKeyed
    {
        bool _InitialSyncComplete;

        public event EventHandler<EventArgs> InitialSyncCompleted;

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

        public bool InitialStatusMessageWasReceived { get; private set; }

        public bool InitialConfigurationMessageWasReceived { get; private set; }

        public bool FeedbackWasRegistered { get; private set; }

        public CodecSyncState(string key)
        {
            Key = key;
            CodecDisconnected();
        }

        public void LoginMessageReceived()
        {
            LoginMessageWasReceived = true;
            Debug.Console(1, this, "Login Message Received.");
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
            LoginMessageWasReceived = false;
            InitialConfigurationMessageWasReceived = false;
            InitialStatusMessageWasReceived = false;
            FeedbackWasRegistered = false;
            InitialSyncComplete = false;
        }

        void CheckSyncStatus()
        {
            if (LoginMessageWasReceived && InitialConfigurationMessageWasReceived && InitialStatusMessageWasReceived && FeedbackWasRegistered)
            {
                InitialSyncComplete = true;
                Debug.Console(1, this, "Initial Codec Sync Complete!");
            }
            else
                InitialSyncComplete = false;
        }
    }

    public class CiscoSparkCodecFactory : EssentialsDeviceFactory<CiscoSparkCodec>
    {
        public CiscoSparkCodecFactory()
        {
            TypeNames = new List<string>() { "ciscospark", "ciscowebex", "ciscowebexpro", "ciscoroomkit" };
        }

        public override EssentialsDevice BuildDevice(DeviceConfig dc)
        {
            Debug.Console(1, "Factory Attempting to create new Cisco Codec Device");
            var comm = CommFactory.CreateCommForDevice(dc);
            return new VideoCodec.Cisco.CiscoSparkCodec(dc, comm);
        }
    }

}