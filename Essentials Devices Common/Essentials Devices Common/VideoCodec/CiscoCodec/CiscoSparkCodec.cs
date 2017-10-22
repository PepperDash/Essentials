using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Crestron.SimplSharp;
using Crestron.SimplSharp.Net.Https;
using Crestron.SimplSharp.CrestronXml;
using Crestron.SimplSharp.CrestronXml.Serialization;
using Newtonsoft.Json;
//using Cisco_One_Button_To_Push;
//using Cisco_SX80_Corporate_Phone_Book;

using PepperDash.Core;
using PepperDash.Essentials.Core;
using PepperDash.Essentials.Core.Routing;
using PepperDash.Essentials.Devices.Common.Codec;
using PepperDash.Essentials.Devices.Common.Occupancy;
using PepperDash.Essentials.Devices.Common.VideoCodec;

namespace PepperDash.Essentials.Devices.Common.VideoCodec.Cisco
{
    enum eCommandType { SessionStart, SessionEnd, Command, GetStatus, GetConfiguration };

    public class CiscoSparkCodec : VideoCodecBase, IHasCallHistory, IHasCallFavorites, IHasDirectory,
        IHasScheduleAwareness, IOccupancyStatusProvider, IHasCodecLayouts, IHasCodecSelfview,
		ICommunicationMonitor, IRouting
    {
        public event EventHandler<DirectoryEventArgs> DirectoryResultReturned;

        public CommunicationGather PortGather { get; private set; }
        public CommunicationGather JsonGather { get; private set; }

        public StatusMonitorBase CommunicationMonitor { get; private set; }

        public BoolFeedback RoomIsOccupiedFeedback { get; private set; }

        public IntFeedback PeopleCountFeedback { get; private set; }

        public BoolFeedback SpeakerTrackIsOnFeedback { get; private set; }

        public BoolFeedback SelfviewIsOnFeedback { get; private set; }

        public StringFeedback SelfviewPipPositionFeedback { get; private set; }

        public StringFeedback LocalLayoutFeedback { get; private set; }

		/// <summary>
		/// An internal pseudo-source that is routable and connected to the osd input
		/// </summary>
		public DummyRoutingInputsDevice OsdSource { get; private set; }

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
            new CodecCommandWithLabel("auto", "Auto"),
            //new CiscoCodecLocalLayout("custom", "Custom"),    // Left out for now
            new CodecCommandWithLabel("equal","Equal"),
            new CodecCommandWithLabel("overlay","Overlay"),
            new CodecCommandWithLabel("prominent","Prominent"),
            new CodecCommandWithLabel("single","Single")
        };
        
        private CiscoCodecConfiguration.RootObject CodecConfiguration;

        private CiscoCodecStatus.RootObject CodecStatus = new CiscoCodecStatus.RootObject();

        public CodecCallHistory CallHistory { get; private set; }

        public CodecCallFavorites CallFavorites { get; private set; }

        public CodecDirectory DirectoryRoot { get; private set; }

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


        private string CliFeedbackRegistrationExpression;

        private CodecSyncState SyncState;

        public CodecPhonebookSyncState PhonebookSyncState { get; private set; }

        private StringBuilder JsonMessage;

        private bool JsonFeedbackMessageIsIncoming;

        public bool CommDebuggingIsOn;


        string Delimiter = "\r\n";

        int PresentationSource;

        string PresentationSourceKey;

        string PhonebookMode = "Local"; // Default to Local

        int PhonebookResultsLimit = 255; // Could be set later by config.

        CTimer LoginMessageReceived;

        // **___________________________________________________________________**
        //  Timers to be moved to the global system timer at a later point....
        CTimer BookingsRefreshTimer;
        CTimer PhonebookRefreshTimer;
        // **___________________________________________________________________**

        public RoutingInputPort CodecOsdIn { get; private set; }
        public RoutingInputPort HdmiIn1 { get; private set; }
        public RoutingInputPort HdmiIn2 { get; private set; }
        public RoutingOutputPort HdmiOut { get; private set; }

        // Constructor for IBasicCommunication
        public CiscoSparkCodec(string key, string name, IBasicCommunication comm, CiscoSparkCodecPropertiesConfig props )
            : base(key, name)
        {
            RoomIsOccupiedFeedback = new BoolFeedback(RoomIsOccupiedFeedbackFunc);
            PeopleCountFeedback = new IntFeedback(PeopleCountFeedbackFunc);
            SpeakerTrackIsOnFeedback = new BoolFeedback(SpeakerTrackIsOnFeedbackFunc);
            SelfviewIsOnFeedback = new BoolFeedback(SelfViewIsOnFeedbackFunc);
            SelfviewPipPositionFeedback = new StringFeedback(SelfviewPipPositionFeedbackFunc);
            LocalLayoutFeedback = new StringFeedback(LocalLayoutFeedbackFunc);

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

            SyncState = new CodecSyncState(key + "--Sync");

            PhonebookSyncState = new CodecPhonebookSyncState(key + "--PhonebookSync");

            SyncState.InitialSyncCompleted += new EventHandler<EventArgs>(SyncState_InitialSyncCompleted);

            PortGather = new CommunicationGather(Communication, Delimiter);
            PortGather.IncludeDelimiter = true;
            PortGather.LineReceived += this.Port_LineReceived;

            CodecConfiguration = new CiscoCodecConfiguration.RootObject();
            //CodecStatus = new CiscoCodecStatus.RootObject();

            CodecInfo = new CiscoCodecInfo(CodecStatus, CodecConfiguration);

            CallHistory = new CodecCallHistory();

            if (props.Favorites != null)
            {
                CallFavorites = new CodecCallFavorites();
                CallFavorites.Favorites = props.Favorites;
            }

            DirectoryRoot = new CodecDirectory();

            CodecSchedule = new CodecScheduleAwareness();
 
            //Set Feedback Actions
            CodecStatus.Status.Audio.Volume.ValueChangedAction = VolumeLevelFeedback.FireUpdate;
            CodecStatus.Status.Audio.VolumeMute.ValueChangedAction = MuteFeedback.FireUpdate;
            CodecStatus.Status.Audio.Microphones.Mute.ValueChangedAction = PrivacyModeIsOnFeedback.FireUpdate;
            CodecStatus.Status.Standby.State.ValueChangedAction = StandbyIsOnFeedback.FireUpdate;
            CodecStatus.Status.RoomAnalytics.PeoplePresence.ValueChangedAction = RoomIsOccupiedFeedback.FireUpdate;
            CodecStatus.Status.RoomAnalytics.PeopleCount.Current.ValueChangedAction = PeopleCountFeedback.FireUpdate;
            CodecStatus.Status.Cameras.SpeakerTrack.Status.ValueChangedAction = SpeakerTrackIsOnFeedback.FireUpdate;
            CodecStatus.Status.Video.Selfview.Mode.ValueChangedAction = SelfviewIsOnFeedback.FireUpdate;
            CodecStatus.Status.Video.Selfview.PIPPosition.ValueChangedAction = ComputeSelfviewPipStatus;
            CodecStatus.Status.Video.Layout.LayoutFamily.Local.ValueChangedAction = ComputeLocalLayout;
            CodecStatus.Status.Conference.Presentation.Mode.ValueChangedAction = SharingContentIsOnFeedback.FireUpdate;

			CodecOsdIn = new RoutingInputPort(RoutingPortNames.CodecOsd, eRoutingSignalType.AudioVideo, 
				eRoutingPortConnectionType.Hdmi, new Action(StopSharing), this);
			HdmiIn1 = new RoutingInputPort(RoutingPortNames.HdmiIn1, eRoutingSignalType.AudioVideo, 
				eRoutingPortConnectionType.Hdmi, new Action(SelectPresentationSource1), this);
			HdmiIn2 = new RoutingInputPort(RoutingPortNames.HdmiIn2, eRoutingSignalType.AudioVideo, 
				eRoutingPortConnectionType.Hdmi, new Action(SelectPresentationSource2), this);

			HdmiOut = new RoutingOutputPort(RoutingPortNames.HdmiOut, eRoutingSignalType.AudioVideo, 
				eRoutingPortConnectionType.Hdmi, null, this);

			InputPorts.Add(CodecOsdIn);
			InputPorts.Add(HdmiIn1);
			InputPorts.Add(HdmiIn2);
			OutputPorts.Add(HdmiOut);

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

            Communication.Connect();
            LoginMessageReceived = new CTimer(DisconnectClientAndReconnect, 5000);

            var socket = Communication as ISocketStatus;
            if (socket != null)
            {
                socket.ConnectionChange += new EventHandler<GenericSocketStatusChageEventArgs>(socket_ConnectionChange);
            }

			CommunicationMonitor.Start();

            string prefix = "xFeedback register ";

            CliFeedbackRegistrationExpression =
                prefix + "/Configuration" + Delimiter +
                prefix + "/Status/Audio" + Delimiter +
                prefix + "/Status/Call" + Delimiter +
                prefix + "/Status/Conference/Presentation" + Delimiter +
                prefix + "/Status/Cameras/SpeakerTrack" + Delimiter +
                prefix + "/Status/RoomAnalytics" + Delimiter +
                prefix + "/Status/Standby" + Delimiter +
                prefix + "/Status/Video/Selfview" + Delimiter +
                prefix + "/Status/Video/Layout" + Delimiter +
                prefix + "/Bookings" + Delimiter +
                prefix + "/Event/CallDisconnect" + Delimiter;        

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
            if (e.Client.IsConnected)
            {
                LoginMessageReceived.Reset(5000);
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

        void DisconnectClientAndReconnect(object o)
        {
            Debug.Console(0, this, "Disconnecting and Reconnecting to codec.");

            Communication.Disconnect();

            CrestronEnvironment.Sleep(2000);

            Communication.Connect();
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
                            LoginMessageReceived.Stop();
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
            // Serializer settings.  We want to ignore null values and mising members
            JsonSerializerSettings settings = new JsonSerializerSettings();
            settings.NullValueHandling = NullValueHandling.Ignore;
            settings.MissingMemberHandling = MissingMemberHandling.Ignore;
            settings.ObjectCreationHandling = ObjectCreationHandling.Auto;

                if (response.IndexOf("\"Status\":{") > -1)
                {
                    // Status Message

                    // Temp object so we can inpsect for call data before simply deserializing
                    CiscoCodecStatus.RootObject tempCodecStatus = new CiscoCodecStatus.RootObject();

                    JsonConvert.PopulateObject(response, tempCodecStatus);

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

                    JsonConvert.PopulateObject(response, CodecStatus);

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
                    // Event Message

                    CiscoCodecEvents.RootObject eventReceived = new CiscoCodecEvents.RootObject();

                    JsonConvert.PopulateObject(response, eventReceived);

                    EvalutateEvent(eventReceived);
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

                            PrintPhonebook(DirectoryRoot);
                        }
                        else if (PhonebookSyncState.InitialSyncComplete)
                        {
                            var directoryResults = new CodecDirectory();

                            directoryResults = CiscoCodecPhonebook.ConvertCiscoPhonebookToGeneric(codecPhonebookResponse.CommandResponse.PhonebookSearchResult);

                            PrintPhonebook(directoryResults);

                            // This will return the latest results to all UIs.  Multiple indendent UI Directory browsing will require a different methodology
                            var handler = DirectoryResultReturned;
                            if (handler != null)
                                handler(this, new DirectoryEventArgs() { Directory = directoryResults });

                            // Fire some sort of callback delegate to the UI that requested the directory search results
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
        /// Evaluates an event received from the codec
        /// </summary>
        /// <param name="eventReceived"></param>
        void EvalutateEvent(CiscoCodecEvents.RootObject eventReceived)
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
        }


        /// <summary>
        /// Gets the first CallId or returns null
        /// </summary>
        /// <returns></returns>
        private string GetCallId()
        {
            string callId = null;

            if (ActiveCalls.Count > 1)
            {
                foreach (CodecActiveCallItem call in ActiveCalls) ;
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
            SendText(string.Format("xCommand Phonebook Search PhonebookType: {0} ContactType: Contact", PhonebookMode));
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

        void PrintPhonebook(CodecDirectory directory)
        {
            if (Debug.Level > 0)
            {
                Debug.Console(1, this, "Directory Results:\n");

                foreach (DirectoryItem item in directory.DirectoryResults)
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
            SelectPresentationSource(1);
        }

        /// <summary>
        /// Select source 2 as the presetnation source
        /// </summary>
        public void SelectPresentationSource2()
        {
            SelectPresentationSource(2);
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

            SendText(string.Format("xCommand Presentation Start PresentationSource: {0} SendingMode: {1}", PresentationSource, sendingMode));
        }

        /// <summary>
        /// Stops sharing the current presentation
        /// </summary>
        public override void StopSharing()
        {
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
                SelfviewModeOff();
            }
            else
            {
                if (ShowSelfViewByDefault)
                    SelfviewModeOn();
                else
                    SelfviewModeOff();
            }
        }

        /// <summary>
        /// Turns on Selfview Mode
        /// </summary>
        public void SelfviewModeOn()
        {
            SendText("xCommand Video Selfview Set Mode: On");
        }

        /// <summary>
        /// Turns off Selfview Mode
        /// </summary>
        public void SelfviewModeOff()
        {
            SendText("xCommand Video Selfview Set Mode: Off");
        }

        /// <summary>
        /// Toggles Selfview mode on/off
        /// </summary>
        public void SelfviewModeToggle()
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

        public class CiscoCodecInfo : VideoCodecInfo
        {
            public CiscoCodecStatus.RootObject CodecStatus { get; private set; }

            public CiscoCodecConfiguration.RootObject CodecConfiguration { get; private set; }

            public override bool MultiSiteOptionIsEnabled
            {
                                get
                {
                    if (CodecStatus.Status.SystemUnit.Software.OptionKeys.MultiSite.Value.ToLower() == "true")
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
            public override string PhoneNumber
            {
                get
                {
                    if (CodecConfiguration.Configuration.H323.H323Alias.E164 != null)           
                        return CodecConfiguration.Configuration.H323.H323Alias.E164.Value;
                    else
                        return string.Empty;
                }
            }
            public override string SipUri
            {
                get
                {
                    if (CodecConfiguration.Configuration.H323.H323Alias.ID != null)          
                        return CodecConfiguration.Configuration.H323.H323Alias.ID.Value;
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

        public bool InitialStatusMessageWasReceived { get; private set; }

        public bool InitialConfigurationMessageWasReceived { get; private set; }

        public bool FeedbackWasRegistered { get; private set; }

        public CodecSyncState(string key)
        {
            Key = key;
            CodecDisconnected();
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
            InitialConfigurationMessageWasReceived = false;
            InitialStatusMessageWasReceived = false;
            FeedbackWasRegistered = false;
            InitialSyncComplete = false;
        }

        void CheckSyncStatus()
        {
            if (InitialConfigurationMessageWasReceived && InitialStatusMessageWasReceived && FeedbackWasRegistered)
            {
                InitialSyncComplete = true;
                Debug.Console(1, this, "Initial Codec Sync Complete!");
            }
            else
                InitialSyncComplete = false;
        }
    }

}