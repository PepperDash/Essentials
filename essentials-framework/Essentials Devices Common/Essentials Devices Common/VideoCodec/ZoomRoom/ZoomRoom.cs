using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Crestron.SimplSharp;
using Crestron.SimplSharpPro.CrestronThread;
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

namespace PepperDash.Essentials.Devices.Common.VideoCodec.ZoomRoom
{
    public class ZoomRoom : VideoCodecBase, IHasCodecSelfView, IHasDirectory, ICommunicationMonitor, IRouting, IHasScheduleAwareness, IHasCodecCameras
    {
        public CommunicationGather PortGather { get; private set; }

        public StatusMonitorBase CommunicationMonitor { get; private set; }

        private CrestronQueue<string> ReceiveQueue;
        
        private Thread ReceiveThread;

        string Delimiter = "\x0D\x0A";

        private ZoomRoomSyncState SyncState;

        public ZoomRoomStatus Status { get; private set; }

        public ZoomRoomConfiguration Configuration { get; private set; }

        private StringBuilder JsonMessage;

        private bool JsonFeedbackMessageIsIncoming;
        private uint JsonCurlyBraceCounter = 0;

        public bool CommDebuggingIsOn;

        //CTimer LoginMessageReceivedTimer;
        //CTimer RetryConnectionTimer;

        /// <summary>
        /// Gets and returns the scaled volume of the codec
        /// </summary>
        protected override Func<int> VolumeLevelFeedbackFunc
        {
            get
            {
                return () => CrestronEnvironment.ScaleWithLimits(Configuration.Audio.Output.Volume, 100, 0, 65535, 0);
            }
        }

        protected override Func<bool> PrivacyModeIsOnFeedbackFunc
        {
            get
            {
                return () => Configuration.Call.Microphone.Mute;
            }
        }

        protected override Func<bool> StandbyIsOnFeedbackFunc
        {
            get
            {
                return () => false;
            }
        }

        protected override Func<string> SharingSourceFeedbackFunc
        {
            get
            {
                return () => Status.Sharing.dispState;
            }
        }

        protected override Func<bool> SharingContentIsOnFeedbackFunc
        {
            get
            {
                return () => Status.Call.Sharing.IsSharing;
            }
        }

        protected Func<bool> FarEndIsSharingContentFeedbackFunc
        {
            get
            {
                return () => false;
            }
        }

        protected override Func<bool> MuteFeedbackFunc
        {
            get
            {
                return () => Configuration.Audio.Output.Volume == 0;
            }
        }

        //protected Func<bool> RoomIsOccupiedFeedbackFunc
        //{
        //    get
        //    {
        //        return () => false;
        //    }
        //}

        //protected Func<int> PeopleCountFeedbackFunc
        //{
        //    get
        //    {
        //        return () => 0;
        //    }
        //}

        protected Func<bool> SelfViewIsOnFeedbackFunc
        {
            get
            {
                return () => !Configuration.Video.HideConfSelfVideo;
            }
        }

        protected Func<string> SelfviewPipPositionFeedbackFunc
        {
            get
            {
                return () => "";
            }
        }

        protected Func<string> LocalLayoutFeedbackFunc
        {
            get
            {
                return () => "";
            }
        }

        protected Func<bool> LocalLayoutIsProminentFeedbackFunc
        {
            get
            {
                return () => false;
            }
        }


        public RoutingInputPort CodecOsdIn { get; private set; }
        public RoutingOutputPort Output1 { get; private set; }

        uint DefaultMeetingDurationMin = 30;

        int PreviousVolumeLevel = 0;

        public ZoomRoom(DeviceConfig config, IBasicCommunication comm)
            : base(config)
        {
            var props = JsonConvert.DeserializeObject<ZoomRoomPropertiesConfig>(config.Properties.ToString());

            // The queue that will collect the repsonses in the order they are received
            ReceiveQueue = new CrestronQueue<string>(25);

            // The thread responsible for dequeuing and processing the messages
            ReceiveThread = new Thread((o) => ProcessQueue(), null);

            Communication = comm;

            if (props.CommunicationMonitorProperties != null)
            {
                CommunicationMonitor = new GenericCommunicationMonitor(this, Communication, props.CommunicationMonitorProperties);
            }
            else
            {
                CommunicationMonitor = new GenericCommunicationMonitor(this, Communication, 30000, 120000, 300000, "zStatus SystemUnit\r");
            }

            DeviceManager.AddDevice(CommunicationMonitor);

            Status = new ZoomRoomStatus();
            
            Configuration = new ZoomRoomConfiguration();

            CodecInfo = new ZoomRoomInfo(Status, Configuration);

            SyncState = new ZoomRoomSyncState(Key + "--Sync", this);

            SyncState.InitialSyncCompleted += new EventHandler<EventArgs>(SyncState_InitialSyncCompleted);

            PhonebookSyncState = new CodecPhonebookSyncState(Key + "--PhonebookSync");

            PortGather = new CommunicationGather(Communication, "\x0A");
            PortGather.IncludeDelimiter = true;
            PortGather.LineReceived += this.Port_LineReceived;

            CodecOsdIn = new RoutingInputPort(RoutingPortNames.CodecOsd, eRoutingSignalType.Audio | eRoutingSignalType.Video,
            eRoutingPortConnectionType.Hdmi, new Action(StopSharing), this);
 
            Output1 = new RoutingOutputPort(RoutingPortNames.AnyVideoOut, eRoutingSignalType.Audio | eRoutingSignalType.Video,
                eRoutingPortConnectionType.Hdmi, null, this);
 
            SelfviewIsOnFeedback = new BoolFeedback(SelfViewIsOnFeedbackFunc);

            CodecSchedule = new CodecScheduleAwareness();

            SetUpFeedbackActions();

            Cameras = new List<CameraBase>();

            SetUpDirectory();            
        }

        void SyncState_InitialSyncCompleted(object sender, EventArgs e)
        {
            SetUpRouting();

            SetIsReady();
        }


        /// <summary>
        /// Subscribes to the PropertyChanged events on the state objects and fires the corresponding feedbacks.
        /// </summary>
        void SetUpFeedbackActions()
        {
            Configuration.Audio.Output.PropertyChanged += new System.ComponentModel.PropertyChangedEventHandler(
                (o, a) =>
                {
                    if (a.PropertyName == "Volume")
                    {
                        VolumeLevelFeedback.FireUpdate();
                        MuteFeedback.FireUpdate();
                    }
                });

            Configuration.Call.Microphone.PropertyChanged += new System.ComponentModel.PropertyChangedEventHandler(
                (o, a) =>
                {
                    if (a.PropertyName == "Mute")
                    {
                        PrivacyModeIsOnFeedback.FireUpdate();
                    }
                });

            Configuration.Video.PropertyChanged += new System.ComponentModel.PropertyChangedEventHandler(
                (o, a) =>
                {
                    if (a.PropertyName == "HideConfSelfVideo")
                    {
                        SelfviewIsOnFeedback.FireUpdate();
                    }
                });
            Configuration.Video.Camera.PropertyChanged += new System.ComponentModel.PropertyChangedEventHandler(
                (o, a) =>
                {
                    if (a.PropertyName == "SelectedId")
                    {
                        SelectCamera(Configuration.Video.Camera.SelectedId);    // this will in turn fire the affected feedbacks
                    }
                });

            Status.Call.Sharing.PropertyChanged += new System.ComponentModel.PropertyChangedEventHandler(
                (o, a) =>
                {
                    if (a.PropertyName == "State")
                    {
                        SharingContentIsOnFeedback.FireUpdate();
                    }
                });

            Status.Sharing.PropertyChanged += new System.ComponentModel.PropertyChangedEventHandler(
                (o, a) =>
                {
                    if (a.PropertyName == "dispState")
                    {
                        SharingSourceFeedback.FireUpdate();
                    }
                    else if (a.PropertyName == "password")
                    {
                        //TODO: Fire Sharing Password Update
                    }
                });
        }

        void SetUpDirectory()
        {
            DirectoryRoot = new CodecDirectory();

            DirectoryBrowseHistory = new List<CodecDirectory>();

            CurrentDirectoryResultIsNotDirectoryRoot = new BoolFeedback(() => DirectoryBrowseHistory.Count > 0);

            CurrentDirectoryResultIsNotDirectoryRoot.FireUpdate();
        }

        void SetUpRouting()
        {
            // Set up input ports
            CreateOsdSource();
            InputPorts.Add(CodecOsdIn);

            // Set up output ports
            OutputPorts.Add(Output1);
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

            //foreach(var input in Status.Video.
        }

        /// <summary>
        /// Starts the HTTP feedback server and syncronizes state of codec
        /// </summary>
        /// <returns></returns>
        public override bool CustomActivate()
        {
            CrestronConsole.AddNewConsoleCommand(SetCommDebug, "SetCodecCommDebug", "0 for Off, 1 for on", ConsoleAccessLevelEnum.AccessOperator);
            CrestronConsole.AddNewConsoleCommand((s) => SendText("zCommand Phonebook List Offset: 0 Limit: 512"), "GetZoomRoomContacts", "Triggers a refresh of the codec phonebook", ConsoleAccessLevelEnum.AccessOperator);
            CrestronConsole.AddNewConsoleCommand((s) => GetBookings(), "GetZoomRoomBookings", "Triggers a refresh of the booking data for today", ConsoleAccessLevelEnum.AccessOperator);

            var socket = Communication as ISocketStatus;
            if (socket != null)
            {
                socket.ConnectionChange += new EventHandler<GenericSocketStatusChageEventArgs>(socket_ConnectionChange);
            }

            // TODO: Turn this off when done initial development
            CommDebuggingIsOn = true;

            Communication.Connect();

            CommunicationMonitor.Start();

            return base.CustomActivate();
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

            }
            else
            {
                SyncState.CodecDisconnected();
                PhonebookSyncState.CodecDisconnected();
            }
        }

        public void SendText(string command)
        {
            if (CommDebuggingIsOn)
                Debug.Console(1, this, "Sending: '{0}'", command);

            Communication.SendText(command + Delimiter);
        }

        /// <summary>
        /// Gathers responses and enqueues them.
        /// </summary>
        /// <param name="dev"></param>
        /// <param name="args"></param>
        void Port_LineReceived(object dev, GenericCommMethodReceiveTextArgs args)
        {
            //if (CommDebuggingIsOn)
            //    Debug.Console(1, this, "Gathered: '{0}'", args.Text);

            ReceiveQueue.Enqueue(args.Text);

            // If the receive thread has for some reason stopped, this will restart it
            if (ReceiveThread.ThreadState != Thread.eThreadStates.ThreadRunning)
                ReceiveThread.Start();
        }


        /// <summary>
        /// Runs in it's own thread to dequeue messages in the order they were received to be processed
        /// </summary>
        /// <returns></returns>
        object ProcessQueue()
        {
            try
            {
                while (true)
                {
                    var message = ReceiveQueue.Dequeue();

                    ProcessMessage(message);
                }
            }
            catch (Exception e)
            {
                Debug.Console(1, this, "Error Processing Queue: {0}", e);
            }

            return null;
        }


        /// <summary>
        /// Queues the initial queries to be sent upon connection
        /// </summary>
        void SetUpSyncQueries()
        {
            // zStatus
            SyncState.AddQueryToQueue("zStatus Call Status");
            SyncState.AddQueryToQueue("zStatus Audio Input Line");
            SyncState.AddQueryToQueue("zStatus Audio Output Line");
            SyncState.AddQueryToQueue("zStatus Video Camera Line");
            SyncState.AddQueryToQueue("zStatus Video Optimizable");
            SyncState.AddQueryToQueue("zStatus Capabilities");
            SyncState.AddQueryToQueue("zStatus Sharing");
            SyncState.AddQueryToQueue("zStatus CameraShare");
            SyncState.AddQueryToQueue("zStatus Call Layout");
            SyncState.AddQueryToQueue("zStatus Call ClosedCaption Available");
            SyncState.AddQueryToQueue("zStatus NumberOfScreens");

            // zConfiguration

            SyncState.AddQueryToQueue("zConfiguration Call Sharing optimize_video_sharing");
            SyncState.AddQueryToQueue("zConfiguration Call Microphone Mute");
            SyncState.AddQueryToQueue("zConfiguration Call Camera Mute");
            SyncState.AddQueryToQueue("zConfiguration Audio Input SelectedId");
            SyncState.AddQueryToQueue("zConfiguration Audio Input is_sap_disabled");
            SyncState.AddQueryToQueue("zConfiguration Audio Input reduce_reverb");
            SyncState.AddQueryToQueue("zConfiguration Audio Input volume");
            SyncState.AddQueryToQueue("zConfiguration Audio Output selectedId");
            SyncState.AddQueryToQueue("zConfiguration Audio Output volume");
            SyncState.AddQueryToQueue("zConfiguration Video hide_conf_self_video");
            SyncState.AddQueryToQueue("zConfiguration Video Camera selectedId");
            SyncState.AddQueryToQueue("zConfiguration Video Camera Mirror");
            SyncState.AddQueryToQueue("zConfiguration Client appVersion");
            SyncState.AddQueryToQueue("zConfiguration Client deviceSystem");
            SyncState.AddQueryToQueue("zConfiguration Call Layout ShareThumb");
            SyncState.AddQueryToQueue("zConfiguration Call Layout Style");
            SyncState.AddQueryToQueue("zConfiguration Call Layout Size");
            SyncState.AddQueryToQueue("zConfiguration Call Layout Position");
            SyncState.AddQueryToQueue("zConfiguration Call Lock Enable");
            SyncState.AddQueryToQueue("zConfiguration Call MuteUserOnEntry Enable");
            SyncState.AddQueryToQueue("zConfiguration Call ClosedCaption FontSize ");
            SyncState.AddQueryToQueue("zConfiguration Call ClosedCaption Visible");

            // zCommand

            SyncState.AddQueryToQueue("zCommand Phonebook List Offset: 0 Limit: 512");
            SyncState.AddQueryToQueue("zCommand Bookings List");


            SyncState.StartSync();
        }

        /// <summary>
        /// Processes messages as they are dequeued
        /// </summary>
        /// <param name="message"></param>
        void ProcessMessage(string message)
        {          
            // Counts the curly braces
            if(message.Contains('{'))
                JsonCurlyBraceCounter++;

            if (message.Contains('}'))
                JsonCurlyBraceCounter--;

            Debug.Console(2, this, "JSON Curly Brace Count: {0}", JsonCurlyBraceCounter);

            if (!JsonFeedbackMessageIsIncoming && message.Trim('\x20') == "{" + Delimiter)        // Check for the beginning of a new JSON message
            {
                JsonFeedbackMessageIsIncoming = true;
                JsonCurlyBraceCounter = 1;  // reset the counter for each new message

                JsonMessage = new StringBuilder();

                JsonMessage.Append(message);

                if (CommDebuggingIsOn)
                    Debug.Console(2, this, "Incoming JSON message...");

                return;
            }
            else if (JsonFeedbackMessageIsIncoming && message.Trim('\x20') == "}" + Delimiter)  // Check for the end of a JSON message
            {
                JsonMessage.Append(message);

                if(JsonCurlyBraceCounter == 0)
                {
                    JsonFeedbackMessageIsIncoming = false;

                    if (CommDebuggingIsOn)
                        Debug.Console(2, this, "Complete JSON Received:\n{0}", JsonMessage.ToString());

                    // Forward the complete message to be deserialized
                    DeserializeResponse(JsonMessage.ToString());
                }

                //JsonMessage = new StringBuilder();
                return;
            }

            // NOTE: This must happen after the above conditions have been checked
            // Append subsequent partial JSON fragments to the string builder
            if (JsonFeedbackMessageIsIncoming)
            {
                JsonMessage.Append(message);

                //Debug.Console(1, this, "Building JSON:\n{0}", JsonMessage.ToString());
                return;
            }

            if (CommDebuggingIsOn)
                Debug.Console(1, this, "Non-JSON response: '{0}'", message);

            JsonCurlyBraceCounter = 0;  // reset on non-JSON response

            if (!SyncState.InitialSyncComplete)
            {
                switch (message.Trim().ToLower()) // remove the whitespace
                {
                    case "*r login successful":
                        {
                            SyncState.LoginMessageReceived();

                            // Fire up a thread to send the intial commands.
                            CrestronInvoke.BeginInvoke((o) =>
                                {
                                    Thread.Sleep(100);
                                    // disable echo of commands
                                    SendText("echo off");
                                    Thread.Sleep(100);
                                    // set feedback exclusions
                                    SendText("zFeedback Register Op: ex Path: /Event/InfoResult/info/callin_country_list");
                                    Thread.Sleep(100);
                                    SendText("zFeedback Register Op: ex Path: /Event/InfoResult/info/callout_country_list");
                                    Thread.Sleep(100);
                                    // switch to json format
                                    SendText("format json");
                                });

                            break;
                        }
                }
            }
        }

        /// <summary>
        /// Deserializes a JSON formatted response
        /// </summary>
        /// <param name="response"></param>
        void DeserializeResponse(string response)
        {
            try
            {
                var trimmedResponse = response.Trim();

                if (trimmedResponse.Length <= 0)
                    return;

                var message = JObject.Parse(trimmedResponse);

                eZoomRoomResponseType eType = (eZoomRoomResponseType)Enum.Parse(typeof(eZoomRoomResponseType), message["type"].Value<string>(), true);

                var topKey = message["topKey"].Value<string>();

                var responseObj = message[topKey];

                Debug.Console(1, "{0} Response Received. topKey: '{1}'\n{2}", eType, topKey, responseObj.ToString());

                switch (eType)
                {
                    case eZoomRoomResponseType.zConfiguration:
                        {
                            switch (topKey.ToLower())
                            {
                                case "call":
                                    {
                                        JsonConvert.PopulateObject(responseObj.ToString(), Configuration.Call);

                                        break;
                                    }
                                case "audio":
                                    {
                                        JsonConvert.PopulateObject(responseObj.ToString(), Configuration.Audio);

                                        break;
                                    }
                                case "video":
                                    {
                                        JsonConvert.PopulateObject(responseObj.ToString(), Configuration.Video);

                                        break;
                                    }
                                case "client":
                                    {
                                        JsonConvert.PopulateObject(responseObj.ToString(), Configuration.Client);

                                        break;
                                    }
                                default:
                                    {
                                        break;
                                    }
                            }
                            break;
                        }
                    case eZoomRoomResponseType.zCommand:
                        {
                            switch (topKey.ToLower())
                            {
                                case "phonebooklistresult":
                                    {
                                        JsonConvert.PopulateObject(responseObj.ToString(), Status.Phonebook);

                                        if(!PhonebookSyncState.InitialSyncComplete)
                                        {
                                            PhonebookSyncState.InitialPhonebookFoldersReceived();
                                            PhonebookSyncState.PhonebookRootEntriesReceived();
                                            PhonebookSyncState.SetPhonebookHasFolders(false);
                                            PhonebookSyncState.SetNumberOfContacts(Status.Phonebook.Contacts.Count);
                                        }

                                        var directoryResults = new CodecDirectory();

                                        directoryResults = zStatus.Phonebook.ConvertZoomContactsToGeneric(Status.Phonebook.Contacts);

                                        DirectoryRoot = directoryResults;

                                        OnDirectoryResultReturned(directoryResults);

                                        break;
                                    }
                                case "listparticipantsresult":
                                    {
                                        Debug.Console(1, this, "JTokenType: {0}", responseObj.Type);

                                        if (responseObj.Type == JTokenType.Array)
                                        {
                                            // if the type is array this must be the complete list
                                            Status.Call.Participants = JsonConvert.DeserializeObject<List<zCommand.ListParticipant>>(responseObj.ToString());
                                        }
                                        else if (responseObj.Type == JTokenType.Object)
                                        {
                                            // this is a single participant event notification

                                            var participant = JsonConvert.DeserializeObject<zCommand.ListParticipant>(responseObj.ToString());

                                            if (participant != null)
                                            {
                                                if (participant.Event == "ZRCUserChangedEventLeftMeeting" || participant.Event == "ZRCUserChangedEventUserInfoUpdated")
                                                {
                                                    var existingParticipant = Status.Call.Participants.FirstOrDefault(p => p.UserId.Equals(participant.UserId));

                                                    if (existingParticipant != null)
                                                    {
                                                        if (participant.Event == "ZRCUserChangedEventLeftMeeting")
                                                        {
                                                            // Remove participant
                                                            Status.Call.Participants.Remove(existingParticipant);
                                                        }
                                                        else if (participant.Event == "ZRCUserChangedEventUserInfoUpdated")
                                                        {
                                                            // Update participant
                                                            JsonConvert.PopulateObject(responseObj.ToString(), existingParticipant);
                                                        }
                                                    }
                                                }
                                                else if(participant.Event == "ZRCUserChangedEventJoinedMeeting")
                                                {
                                                    Status.Call.Participants.Add(participant);
                                                }
                                            }
                                        }

                                        PrintCurrentCallParticipants();

                                        break;
                                    }
                                default:
                                    {
                                        break;
                                    }
                            }
                            break;
                        }
                    case eZoomRoomResponseType.zEvent:
                        {
                            switch (topKey.ToLower())
                            {
                                case "phonebook":
                                    {
                                        if (responseObj["Updated Contact"] != null)
                                        {
                                            var updatedContact = JsonConvert.DeserializeObject<zStatus.Contact>(responseObj["Updated Contact"].ToString());

                                            var existingContact = Status.Phonebook.Contacts.FirstOrDefault(c => c.Jid.Equals(updatedContact.Jid));

                                            if (existingContact != null)
                                            {
                                                // Update existing contact
                                                JsonConvert.PopulateObject(responseObj["Updated Contact"].ToString(), existingContact);
                                            }
                                        }
                                        else if (responseObj["Added Contact"] != null)
                                        {
                                            var newContact = JsonConvert.DeserializeObject<zStatus.Contact>(responseObj["Updated Contact"].ToString());

                                            // Add a new contact
                                            Status.Phonebook.Contacts.Add(newContact);
                                        }

                                        break;
                                    }
                                case "bookingslistresult":
                                    {
                                        if (!SyncState.InitialSyncComplete)
                                            SyncState.LastQueryResponseReceived();

                                        var codecBookings = new List<zCommand.BookingsListResult>();

                                        codecBookings = JsonConvert.DeserializeObject < List<zCommand.BookingsListResult>>(responseObj.ToString());

                                        if (codecBookings != null && codecBookings.Count > 0)
                                        {
                                            CodecSchedule.Meetings = zCommand.GetGenericMeetingsFromBookingResult(codecBookings);
                                        }

                                        break;
                                    }
                                case "bookings":
                                    {
                                        // Bookings have been updated, trigger a query to retreive the new bookings
                                        if (responseObj["Updated"] != null)
                                            GetBookings();

                                        break;
                                    }
                                case "sharingstate":
                                    {
                                        JsonConvert.PopulateObject(responseObj.ToString(), Status.Call.Sharing);

                                        break;
                                    }
                                case "incomingcallindication":
                                    {
                                        var incomingCall = JsonConvert.DeserializeObject<zEvent.IncomingCallIndication>(responseObj.ToString());

                                        if (incomingCall != null)
                                        {
                                            var newCall = new CodecActiveCallItem();

                                            newCall.Direction = eCodecCallDirection.Incoming;
                                            newCall.Status = eCodecCallStatus.Ringing;
                                            newCall.Type = eCodecCallType.Unknown;
                                            newCall.Name = incomingCall.callerName;
                                            newCall.Id = incomingCall.callerJID;

                                            ActiveCalls.Add(newCall);

                                            OnCallStatusChange(newCall);
                                        }

                                        break;
                                    }
                                case "treatedincomingcallindication":
                                    {
                                        var incomingCall = JsonConvert.DeserializeObject<zEvent.IncomingCallIndication>(responseObj.ToString());

                                        if (incomingCall != null)
                                        {
                                            var existingCall = ActiveCalls.FirstOrDefault(c => c.Id.Equals(incomingCall.callerJID));

                                            if (existingCall != null)
                                            {
                                                if (!incomingCall.accepted)
                                                {
                                                    existingCall.Status = eCodecCallStatus.Disconnected;
                                                }
                                                else
                                                {
                                                    existingCall.Status = eCodecCallStatus.Connecting;
                                                }

                                                OnCallStatusChange(existingCall);
                                            }

                                            UpdateCallStatus();
                                        }

                                        break;
                                    }
                                case "calldisconnect":
                                    {
                                        var disconnectEvent = JsonConvert.DeserializeObject<zEvent.CallDisconnect>(responseObj.ToString());

                                        if (disconnectEvent.Successful)
                                        {
                                            if (ActiveCalls.Count > 0)
                                            {
                                                var activeCall = ActiveCalls.FirstOrDefault(c => c.IsActiveCall);

                                                if (activeCall != null)
                                                {
                                                    activeCall.Status = eCodecCallStatus.Disconnected;

                                                    OnCallStatusChange(activeCall);
                                                }
                                            }
                                        }

                                        UpdateCallStatus();
                                        break;
                                    }
                                case "callconnecterror":
                                    {
                                        UpdateCallStatus();
                                        break;
                                    }
                                case "videounmuterequest":
                                    {
                                        // TODO: notify room of a request to unmute video
                                        break;
                                    }
                                case "meetingneedspassword":
                                    {
                                        // TODO: notify user to enter a password
                                        break;
                                    }
                                case "needwaitforhost":
                                    {
                                        var needWait = JsonConvert.DeserializeObject<zEvent.NeedWaitForHost>(responseObj.ToString());

                                        if (needWait.Wait)
                                        {
                                            // TODO: notify user to wait for host
                                        }

                                        break;
                                    }
                                case "openvideofailforhoststop":
                                    {
                                        // TODO: notify user that host has disabled unmuting video
                                        break;
                                    }
                                case "updatedcallrecordinfo":
                                    {
                                        JsonConvert.PopulateObject(responseObj.ToString(), Status.Call.CallRecordInfo);

                                        break;
                                    }
                                default:
                                    {
                                        break;
                                    }
                            }
                            break;
                        }
                    case eZoomRoomResponseType.zStatus:
                        {
                            switch (topKey.ToLower())
                            {
                                case "login":
                                    {
                                        SyncState.LoginMessageReceived();

                                        if (!SyncState.InitialQueryMessagesWereSent)
                                            SetUpSyncQueries();

                                        JsonConvert.PopulateObject(responseObj.ToString(), Status.Login);

                                        break;
                                    }
                                case "systemunit":
                                    {
                                        JsonConvert.PopulateObject(responseObj.ToString(), Status.SystemUnit);

                                        break;
                                    }
                                case "call":
                                    {
                                        JsonConvert.PopulateObject(responseObj.ToString(), Status.Call);

                                        UpdateCallStatus();

                                        break;
                                    }
                                case "capabilities":
                                    {
                                        JsonConvert.PopulateObject(responseObj.ToString(), Status.Capabilities);
                                        break;
                                    }
                                case "sharing":
                                    {
                                        JsonConvert.PopulateObject(responseObj.ToString(), Status.Sharing);

                                        break;
                                    }
                                case "numberofscreens":
                                    {
                                        JsonConvert.PopulateObject(responseObj.ToString(), Status.NumberOfScreens);
                                        break;
                                    }
                                case "video":
                                    {
                                        JsonConvert.PopulateObject(responseObj.ToString(), Status.Video);
                                        break;
                                    }
                                case "camerashare":
                                    {
                                        JsonConvert.PopulateObject(responseObj.ToString(), Status.CameraShare);
                                        break;
                                    }
                                case "layout":
                                    {
                                        JsonConvert.PopulateObject(responseObj.ToString(), Status.Layout);
                                        break;
                                    }
                                case "audio input line":
                                    {
                                        JsonConvert.PopulateObject(responseObj.ToString(), Status.AudioInputs);
                                        break;
                                    }
                                case "audio output line":
                                    {
                                        JsonConvert.PopulateObject(responseObj.ToString(), Status.AudioOuputs);
                                        break;
                                    }
                                case "video camera line":
                                    {
                                        JsonConvert.PopulateObject(responseObj.ToString(), Status.Cameras);

                                        if(!SyncState.CamerasHaveBeenSetUp)
                                            SetUpCameras();

                                        break;
                                    }
                                default:
                                    {
                                        break;
                                    }
                            }

                            break;
                        }
                    default:
                        {
                            Debug.Console(1, "Unknown Response Type:");
                            break;
                        }
                }

            }
            catch (Exception ex)
            {
                Debug.Console(1, this, "Error Deserializing feedback: {0}", ex);
            }
        }

        public void PrintCurrentCallParticipants()
        {
            if (Debug.Level > 0)
            {
                Debug.Console(1, this, "****************************Call Participants***************************");
                foreach (var participant in Status.Call.Participants)
                {
                    Debug.Console(1, this, "Name: {0} Audio: {1} IsHost: {2}", participant.UserName, participant.AudioStatusState, participant.IsHost);
                }
                Debug.Console(1, this, "************************************************************************");
            }
        }

        /// <summary>
        /// Retrieves bookings list
        /// </summary>
        void GetBookings()
        {
            SendText("zCommand Bookings List");
        }


        /// <summary>
        /// Updates the current call status
        /// </summary>
        void UpdateCallStatus()
        {
            zStatus.eCallStatus callStatus;

            if (Status.Call != null)
            {
                callStatus = Status.Call.Status;

                // If not currently in a meeting, intialize the call object
                if (callStatus != zStatus.eCallStatus.IN_MEETING || callStatus != zStatus.eCallStatus.CONNECTING_MEETING)
                {
                    Status.Call = new zStatus.Call();
                    Status.Call.Status = callStatus; // set the status after initializing the object
                }

                if (ActiveCalls.Count == 0)
                {
                    if(callStatus == zStatus.eCallStatus.CONNECTING_MEETING)
                    {
                        var newCall = new CodecActiveCallItem();

                        newCall.Status = eCodecCallStatus.Connecting;

                        ActiveCalls.Add(newCall);

                        OnCallStatusChange(newCall);
                    }
                }
                else
                {
                    var existingCall = ActiveCalls.FirstOrDefault(c => !c.Status.Equals(eCodecCallStatus.Ringing));

                    if (callStatus == zStatus.eCallStatus.IN_MEETING)
                    {
                        existingCall.Status = eCodecCallStatus.Connected;
                    }
                    else if (callStatus == zStatus.eCallStatus.NOT_IN_MEETING)
                    {
                        existingCall.Status = eCodecCallStatus.Disconnected;
                    }

                    OnCallStatusChange(existingCall);
                }

            }

            Debug.Console(1, this, "****************************Active Calls*********************************");

            // Clean up any disconnected calls left in the list
            for (int i = 0; i < ActiveCalls.Count; i++)
            {
                var call = ActiveCalls[i];

                Debug.Console(1, this, 
                    @"Name: {0}
                    ID: {1}
                    IsActive: {2}
                    Status: {3}
                    Direction: {4}", call.Name, call.Id, call.IsActiveCall, call.Status, call.Direction);

                if (!call.IsActiveCall)
                {
                    Debug.Console(1, this, "******Removing Inactive Call: {0}******", call.Name);
                    ActiveCalls.Remove(call);
                }
            }

            Debug.Console(1, this, "**************************************************************************");

        }

        public override void StartSharing()
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Stops sharing the current presentation
        /// </summary>
        public override void StopSharing()
        {
            SendText("zCommand Call Sharing Disconnect");
        }

        public override void PrivacyModeOn()
        {
            SendText("zConfiguration Call Microphone Mute: on");
        }

        public override void PrivacyModeOff()
        {
            SendText("zConfiguration Call Microphone Mute: off");
        }

        public override void PrivacyModeToggle()
        {
            if (PrivacyModeIsOnFeedback.BoolValue)
                PrivacyModeOff();
            else
                PrivacyModeOn();
        }

        public override void MuteOff()
        {
            SetVolume((ushort)PreviousVolumeLevel);
        }

        public override void MuteOn()
        {
            PreviousVolumeLevel = Configuration.Audio.Output.Volume;    // Store the previous level for recall

            SetVolume(0);
        }

        public override void MuteToggle()
        {
            if (MuteFeedback.BoolValue)
                MuteOff();
            else
                MuteOn();
        }

        /// <summary>
        /// Increments the voluem
        /// </summary>
        /// <param name="pressRelease"></param>
        public override void VolumeUp(bool pressRelease)
        {
            // TODO: Implment volume increment that calls SetVolume()
        }

        /// <summary>
        /// Decrements the volume
        /// </summary>
        /// <param name="pressRelease"></param>
        public override void VolumeDown(bool pressRelease)
        {
            // TODO: Implment volume decrement that calls SetVolume()
        }

        /// <summary>
        /// Scales the level and sets the codec to the specified level within its range
        /// </summary>
        /// <param name="level">level from slider (0-65535 range)</param>
        public override void SetVolume(ushort level)
        {
            var scaledLevel = CrestronEnvironment.ScaleWithLimits(level, 65535, 0, 100, 0);
            SendText(string.Format("zConfiguration Audio Output volume: {0}", scaledLevel));
        }

        /// <summary>
        /// Recalls the default volume on the codec
        /// </summary>
        public void VolumeSetToDefault()
        {
            
        }

        /// <summary>
        /// 
        /// </summary>
        public override void StandbyActivate()
        {
            // No corresponding function on device
        }

        /// <summary>
        /// 
        /// </summary>
        public override void StandbyDeactivate()
        {
            // No corresponding function on device
        }

        public override void ExecuteSwitch(object selector)
        {
            (selector as Action)();
        }

        public void ExecuteSwitch(object inputSelector, object outputSelector, eRoutingSignalType signalType)
        {
            ExecuteSwitch(inputSelector);
        }

        public override void AcceptCall(CodecActiveCallItem call)
        {
            var incomingCall = ActiveCalls.FirstOrDefault(c => c.Status.Equals(eCodecCallStatus.Ringing) && c.Direction.Equals(eCodecCallDirection.Incoming));
            SendText(string.Format("zCommand Call Accept callerJID: {0}", incomingCall.Id));
        }

        public override void RejectCall(CodecActiveCallItem call)
        {
            var incomingCall = ActiveCalls.FirstOrDefault(c => c.Status.Equals(eCodecCallStatus.Ringing) && c.Direction.Equals(eCodecCallDirection.Incoming));
            SendText(string.Format("zCommand Call Reject callerJID: {0}", incomingCall.Id));
        }

        public override void Dial(Meeting meeting)
        {
            SendText(string.Format("zCommand Dial Start meetingNumber: {0}", meeting.Id));
        }

        public override void Dial(string number)
        {
            SendText(string.Format("zCommand Dial Join meetingNumber: {0}", number));
        }

        /// <summary>
        /// Invites a contact to either a new meeting (if not already in a meeting) or the current meeting.
        /// Currently only invites a single user
        /// </summary>
        /// <param name="contact"></param>
        public override void Dial(IInvitableContact contact)
        {
            var ic = contact as zStatus.ZoomDirectoryContact;

            if (ic != null)
            {
                Debug.Console(1, this, "Attempting to Dial (Invite): {0}", ic.Name);

                if (!IsInCall)
                    SendText(string.Format("zCommand Invite Duration: {0} user: {1}", DefaultMeetingDurationMin, ic.ContactId));
                else
                    SendText(string.Format("zCommand Call invite user: {0}", ic.ContactId));
            }
        }

        public override void EndCall(CodecActiveCallItem call)
        {
            SendText("zCommand Call Disconnect");
        }

        public override void EndAllCalls()
        {
            SendText("zCommand Call Disconnect");
        }

        public override void SendDtmf(string s)
        {
            throw new NotImplementedException();
        }


        #region IHasCodecSelfView Members

        public BoolFeedback SelfviewIsOnFeedback { get; private set; }

        public void SelfViewModeOn()
        {
            SendText("zConfiguration Video hide_conf_self_video: off");
        }

        public void SelfViewModeOff()
        {
            SendText("zConfiguration Video hide_conf_self_video: on");
        }

        public void SelfViewModeToggle()
        {
            if (SelfviewIsOnFeedback.BoolValue)
                SelfViewModeOff();
            else
                SelfViewModeOn();
        }

        #endregion

        #region IHasDirectory Members

        public event EventHandler<DirectoryEventArgs> DirectoryResultReturned;

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

            //PrintDirectory(result);
        }

        public CodecDirectory DirectoryRoot { get; private set; }

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

        public CodecPhonebookSyncState PhonebookSyncState { get; private set; }

        public void SearchDirectory(string searchString)
        {
            var directoryResults = new CodecDirectory();

            directoryResults.AddContactsToDirectory(DirectoryRoot.CurrentDirectoryResults.FindAll(c => c.Name.IndexOf(searchString, 0, StringComparison.OrdinalIgnoreCase) > -1));

            DirectoryBrowseHistory.Add(directoryResults);

            OnDirectoryResultReturned(directoryResults);
        }

        public void GetDirectoryFolderContents(string folderId)
        {
            var directoryResults = new CodecDirectory();

            directoryResults.ResultsFolderId = folderId;
            directoryResults.AddContactsToDirectory(DirectoryRoot.CurrentDirectoryResults.FindAll(c => c.FolderId.Equals(folderId)));

            DirectoryBrowseHistory.Add(directoryResults);

            OnDirectoryResultReturned(directoryResults);
        }

        public void SetCurrentDirectoryToRoot()
        {
            DirectoryBrowseHistory.Clear();

            OnDirectoryResultReturned(DirectoryRoot);
        }

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

        public BoolFeedback CurrentDirectoryResultIsNotDirectoryRoot { get; private set; }

        public List<CodecDirectory> DirectoryBrowseHistory { get; private set; }

        #endregion

        #region IHasScheduleAwareness Members

        public CodecScheduleAwareness CodecSchedule { get; private set; }

        public void GetSchedule()
        {
            GetBookings();
        }

        #endregion

        /// <summary>
        /// Builds the cameras List by using the Zoom Room zStatus.Cameras data.  Could later be modified to build from config data
        /// </summary>
        void SetUpCameras()
        {
            SelectedCameraFeedback = new StringFeedback(() => Configuration.Video.Camera.SelectedId);

            ControllingFarEndCameraFeedback = new BoolFeedback(() => SelectedCamera is IAmFarEndCamera);

            foreach (var cam in Status.Cameras)
            {
                var camera = new ZoomRoomCamera(cam.id, cam.Name, this);

                Cameras.Add(camera);

                if (cam.Selected)
                    SelectedCamera = camera;              
            }

            if (IsInCall)
                UpdateFarEndCameras();

            SyncState.CamerasSetUp();
        }

        /// <summary>
        /// Dynamically creates far end cameras for call participants who have far end control enabled.
        /// </summary>
        void UpdateFarEndCameras()
        {
            // TODO: set up far end cameras for the current call
        }

        #region IHasCameras Members

        public event EventHandler<CameraSelectedEventArgs> CameraSelected;

        public List<CameraBase> Cameras { get; private set; }

        private CameraBase _selectedCamera;

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


        public StringFeedback SelectedCameraFeedback { get; private set; }

        public void SelectCamera(string key)
        {
            if(Cameras != null)
            {
                var camera = Cameras.FirstOrDefault(c => c.Key.IndexOf(key, StringComparison.OrdinalIgnoreCase) > -1);
                if (camera != null)
                {
                    Debug.Console(1, this, "Selected Camera with key: '{0}'", camera.Key);
                    SelectedCamera = camera;
                }
                else
                    Debug.Console(1, this, "Unable to select camera with key: '{0}'", key);
            }
        }

        #endregion

        #region IHasFarEndCameraControl Members

        public CameraBase FarEndCamera { get; private set; }

        public BoolFeedback ControllingFarEndCameraFeedback { get; private set; }

        #endregion
    }

    /// <summary>
    /// Zoom Room specific info object
    /// </summary>
    public class ZoomRoomInfo : VideoCodecInfo
    {
        public ZoomRoomStatus Status { get; private set; }
        public ZoomRoomConfiguration Configuration { get; private set; }

        public override bool AutoAnswerEnabled
        {
            get 
            {
                return Status.SystemUnit.RoomInfo.AutoAnswerIsEnabled;
            }
        }

        public override string E164Alias
        {
            get
            {
                if (!string.IsNullOrEmpty(Status.SystemUnit.MeetingNumber))
                    return Status.SystemUnit.MeetingNumber;
                else
                    return string.Empty;
            }
        }

        public override string H323Id
        {
            get
            {
                if (!string.IsNullOrEmpty(Status.Call.Info.meeting_list_item.third_party.h323_address))
                    return Status.Call.Info.meeting_list_item.third_party.h323_address;
                else
                    return string.Empty;
            }
        }

        public override string IpAddress
        {
            get
            {
                if (!string.IsNullOrEmpty(Status.SystemUnit.RoomInfo.AccountEmail))
                    return Status.SystemUnit.RoomInfo.AccountEmail;
                else
                    return string.Empty;
            }
        }

        public override bool MultiSiteOptionIsEnabled
        {
            get { return true; }
        }

        public override string SipPhoneNumber
        {
            get
            {
                if (!string.IsNullOrEmpty(Status.Call.Info.dialIn))
                    return Status.Call.Info.dialIn;
                else
                    return string.Empty;
            }
        }

        public override string SipUri
        {
            get
            {
                if (!string.IsNullOrEmpty(Status.Call.Info.meeting_list_item.third_party.sip_address))
                    return Status.Call.Info.meeting_list_item.third_party.sip_address;
                else
                    return string.Empty;
            }
        }

        public ZoomRoomInfo(ZoomRoomStatus status, ZoomRoomConfiguration configuration)
        {
            Status = status;
            Configuration = configuration;
        }
    }

    /// <summary>
    /// Tracks the initial sycnronization state when establishing a new connection
    /// </summary>
    public class ZoomRoomSyncState : IKeyed
    {
        bool _InitialSyncComplete;

        public event EventHandler<EventArgs> InitialSyncCompleted;

        private CrestronQueue<string> SyncQueries;

        private ZoomRoom Parent;

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

        public bool InitialQueryMessagesWereSent { get; private set; }

        public bool LastQueryResponseWasReceived { get; private set; }

        public bool CamerasHaveBeenSetUp { get; private set;}

        public ZoomRoomSyncState(string key, ZoomRoom parent)
        {
            Parent = parent;
            Key = key;
            SyncQueries = new CrestronQueue<string>(50);
            CodecDisconnected();           
        }

        public void StartSync()
        {
            DequeueQueries();
        }

        void DequeueQueries()
        {
            while (!SyncQueries.IsEmpty)
            {
                var query = SyncQueries.Dequeue();

                Parent.SendText(query);                
            }

            InitialQueryMessagesSent();
        }

        public void AddQueryToQueue(string query)
        {
            SyncQueries.Enqueue(query);
        }

        public void LoginMessageReceived()
        {
            LoginMessageWasReceived = true;
            Debug.Console(1, this, "Login Message Received.");
            CheckSyncStatus();
        }

        public void InitialQueryMessagesSent()
        {
            InitialQueryMessagesWereSent = true;
            Debug.Console(1, this, "Query Messages Sent.");
            CheckSyncStatus();
        }

        public void LastQueryResponseReceived()
        {
            LastQueryResponseWasReceived = true;
            Debug.Console(1, this, "Last Query Response Received.");
            CheckSyncStatus();
        }

        public void CamerasSetUp()
        {
            CamerasHaveBeenSetUp = true;
            Debug.Console(1, this, "Cameras Set Up.");
            CheckSyncStatus();
        }

        public void CodecDisconnected()
        {
            SyncQueries.Clear();
            LoginMessageWasReceived = false;
            InitialQueryMessagesWereSent = false;
            LastQueryResponseWasReceived = false;
            CamerasHaveBeenSetUp = false;
            InitialSyncComplete = false;
        }

        void CheckSyncStatus()
        {
            if (LoginMessageWasReceived && InitialQueryMessagesWereSent && LastQueryResponseWasReceived && CamerasHaveBeenSetUp)
            {
                InitialSyncComplete = true;
                Debug.Console(1, this, "Initial Codec Sync Complete!");
            }
            else
                InitialSyncComplete = false;
        }
    }

    public class ZoomRoomFactory : EssentialsDeviceFactory<ZoomRoom>
    {
        public ZoomRoomFactory()
        {
            TypeNames = new List<string>() { "zoomroom" };
        }

        public override EssentialsDevice BuildDevice(DeviceConfig dc)
        {
            Debug.Console(1, "Factory Attempting to create new ZoomRoom Device");
            var comm = CommFactory.CreateCommForDevice(dc);
            return new VideoCodec.ZoomRoom.ZoomRoom(dc, comm);
        }
    }

}