using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Crestron.SimplSharp;
using Crestron.SimplSharp.Net.Https;
using Crestron.SimplSharp.CrestronXml;
using Crestron.SimplSharp.CrestronXml.Serialization;
using Newtonsoft.Json;
using Cisco_One_Button_To_Push;
using Cisco_SX80_Corporate_Phone_Book;

using PepperDash.Core;
using PepperDash.Essentials.Core;
using PepperDash.Essentials.Core.Routing;
using PepperDash.Essentials.Devices.Common.Codec;

namespace PepperDash.Essentials.Devices.Common.VideoCodec.Cisco
{
    enum eCommandType { SessionStart, SessionEnd, Command, GetStatus, GetConfiguration };

    public class CiscoCodec : VideoCodecBase, IHasCallHistory
    {
        public event EventHandler<EventArgs> RecentCallsListHasChanged;

        public IBasicCommunication Communication { get; private set; }
        public CommunicationGather PortGather { get; private set; }
        public CommunicationGather JsonGather { get; private set; }

        public StatusMonitorBase CommunicationMonitor { get; private set; }

        public BoolFeedback StandbyIsOnFeedback { get; private set; }

        public BoolFeedback RoomIsOccupiedFeedback { get; private set; }

        public IntFeedback PeopleCountFeedback { get; private set; }

        public BoolFeedback SpeakerTrackIsOnFeedback { get; private set; }

        private CiscoOneButtonToPush CodecObtp;

        private Corporate_Phone_Book PhoneBook;

        private CiscoCodecConfiguration.RootObject CodecConfiguration;

        private CiscoCodecStatus.RootObject CodecStatus;

        private CiscoCallHistory.RootObject CodecCallHistory;

        public List<CallHistory.CallHistoryEntry> RecentCalls { get; private set; }

        //private CiscoCodecEvents.RootObject CodecEvent;

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

        protected Func<bool> StandbyStateFeedbackFunc
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
#warning figure out how to return the key of the shared source somehow
            get 
            {
                return () => "todo";
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

        protected override Func<int> ActiveCallCountFeedbackFunc
        {
            get { return () => ActiveCalls.Count; }
        }

        //private HttpsClient Client;

        //private HttpApiServer Server;

        //private int ServerPort;

        //private string CodecUrl;

        //private string HttpSessionId;

        //private string FeedbackRegistrationExpression;

        private string CliFeedbackRegistrationExpression;

        private CodecSyncState SyncState;

        private StringBuilder JsonMessage;

        private bool JsonFeedbackMessageIsIncoming;

        string Delimiter = "\r\n";

        int PresentationSource;

        public bool CommDebuggingIsOn;

        // Constructor for IBasicCommunication
        public CiscoCodec(string key, string name, IBasicCommunication comm, int serverPort)
            : base(key, name)
        {
            StandbyIsOnFeedback = new BoolFeedback(StandbyStateFeedbackFunc);
            RoomIsOccupiedFeedback = new BoolFeedback(RoomIsOccupiedFeedbackFunc);
            PeopleCountFeedback = new IntFeedback(PeopleCountFeedbackFunc);
            SpeakerTrackIsOnFeedback = new BoolFeedback(SpeakerTrackIsOnFeedbackFunc);


            Communication = comm;

            SyncState = new CodecSyncState(key + "--sync");

            SyncState.InitialSyncCompleted += new EventHandler<EventArgs>(SyncState_InitialSyncCompleted);

            PortGather = new CommunicationGather(Communication, Delimiter);
            PortGather.IncludeDelimiter = true;
            PortGather.LineReceived += this.Port_LineReceived;

            CodecObtp = new CiscoOneButtonToPush();

            PhoneBook = new Corporate_Phone_Book();

            CodecConfiguration = new CiscoCodecConfiguration.RootObject();

            CodecStatus = new CiscoCodecStatus.RootObject();

            CodecCallHistory = new CiscoCallHistory.RootObject();
 
            CodecStatus.Status.Audio.Volume.ValueChangedAction = VolumeLevelFeedback.FireUpdate;
            CodecStatus.Status.Audio.VolumeMute.ValueChangedAction = MuteFeedback.FireUpdate;
            CodecStatus.Status.Audio.Microphones.Mute.ValueChangedAction = PrivacyModeIsOnFeedback.FireUpdate;
            CodecStatus.Status.Standby.State.ValueChangedAction = StandbyIsOnFeedback.FireUpdate;
            CodecStatus.Status.RoomAnalytics.PeoplePresence.ValueChangedAction = RoomIsOccupiedFeedback.FireUpdate;
            CodecStatus.Status.RoomAnalytics.PeopleCount.Current.ValueChangedAction = PeopleCountFeedback.FireUpdate;
            CodecStatus.Status.Cameras.SpeakerTrack.Status.ValueChangedAction = SpeakerTrackIsOnFeedback.FireUpdate;

            //ServerPort = serverPort;

            //Client = new HttpsClient();

            //Server = new HttpApiServer();      
        }

        /// <summary>
        /// Fires when initial codec sync is completed.  Used to then send commands to get call history, phonebook, bookings, etc.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void SyncState_InitialSyncCompleted(object sender, EventArgs e)
        {
            SendText("xCommand CallHistory Recents Limit: 20 Order: OccurrenceTime");

            // Get bookings for the day
            //SendText("xCommand Bookings List Days: 1 DayOffset: 0");

            // Get Phonebook (determine local/corporate from config, and set results limit)
            //SendText("xCommand Phonebook Search PhonebookType: {0} ContactType: Folder Limit: {0}", PhonebookType, PhonebookResultsLimit);
        }

        /// <summary>
        /// Starts the HTTP feedback server and syncronizes state of codec
        /// </summary>
        /// <returns></returns>
        public override bool CustomActivate()
        {
            CrestronConsole.AddNewConsoleCommand(SendText, "send" + Key, "", ConsoleAccessLevelEnum.AccessOperator);
            CrestronConsole.AddNewConsoleCommand(SetCommDebug, "SetCiscoCommDebug", "0 for Off, 1 for on", ConsoleAccessLevelEnum.AccessOperator);

            

            Communication.Connect();
            var socket = Communication as ISocketStatus;
            if (socket != null)
            {
                socket.ConnectionChange += new EventHandler<GenericSocketStatusChageEventArgs>(socket_ConnectionChange);
            }

            InputPorts.Add(new RoutingInputPort(RoutingPortNames.HdmiIn1, eRoutingSignalType.AudioVideo, eRoutingPortConnectionType.Hdmi, new Action(SelectPresentationSource1), this));
            InputPorts.Add(new RoutingInputPort(RoutingPortNames.HdmiIn2, eRoutingSignalType.AudioVideo, eRoutingPortConnectionType.Hdmi, new Action(SelectPresentationSource2), this));

            //Debug.Console(1, this, "Starting Cisco API Server");

            //Server.Start(ServerPort);

            //Server.ApiRequest += new EventHandler<Crestron.SimplSharp.Net.Http.OnHttpRequestArgs>(Server_ApiRequest);

            //CodecUrl = string.Format("http://{0}", (Communication as GenericSshClient).Hostname);

            CommunicationMonitor = new GenericCommunicationMonitor(this, Communication, 2000, 120000, 300000, "xStatus SystemUnit Software Version\r");
            DeviceManager.AddDevice(CommunicationMonitor);

            //Client = new HttpsClient();

            //Client.Verbose = true;
            //Client.KeepAlive = true;


            // Temp feedback registration

            //FeedbackRegistrationExpression =
            //    "<Command><HttpFeedback><Register command=\"True\"><FeedbackSlot>1</FeedbackSlot>" +
            //    string.Format("<ServerUrl>http://{0}:{1}/cisco/api</ServerUrl>", CrestronEthernetHelper.GetEthernetParameter(CrestronEthernetHelper.ETHERNET_PARAMETER_TO_GET.GET_CURRENT_IP_ADDRESS, 0), ServerPort) +
            //    "<Format>JSON</Format>" +
            //    "<Expression item=\"1\">/Configuration</Expression>" +
            //    "<Expression item=\"2\">/Event/CallDisconnect</Expression>" +
            //    "<Expression item=\"3\">/Status/Call</Expression>" +
            //    "</Register>" +
            //    "</HttpFeedback>" +
            //    "</Command>";

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
                prefix + "/Bookings" + Delimiter +
                prefix + "/Event/CallDisconnect" + Delimiter;

            //StartHttpsSession();

            //CodecObtp.Initialize();

            //CodecObtp.GetMeetings();

            //PhoneBook.DownloadPhoneBook(Corporate_Phone_Book.ePhoneBookLocation.Corporate);         

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
            // Reset sync status on disconnect
            if (!e.Client.IsConnected)
                SyncState.CodecDisconnected();
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

        //private void StartHttpsSession()
        //{
        //    SendHttpCommand("", eCommandType.SessionStart);
        //}

        //private void EndHttpsSession()
        //{
        //    SendHttpCommand("", eCommandType.SessionEnd);
        //}

        //private void SendHttpCommand(string command, eCommandType commandType)
        //{
        //    //HttpsClientRequest request = new HttpsClientRequest();

        //    //string urlSuffix = null;

        //    //Client.UserName = null;
        //    //Client.Password = null;

        //    //Client.PeerVerification = false;
        //    //Client.HostVerification = false;

        //    //request.RequestType = RequestType.Post;

        //    //if(!string.IsNullOrEmpty(HttpSessionId))
        //    //    request.Header.SetHeaderValue("Cookie", HttpSessionId);

        //    //switch (commandType)
        //    //{
        //    //    case eCommandType.Command:
        //    //        {
        //    //            urlSuffix = "/putxml";
        //    //            request.ContentString = command;
        //    //            request.Header.SetHeaderValue("Content-Type", "text/xml");
        //    //            break;
        //    //        }
        //    //    case eCommandType.SessionStart:
        //    //        {
                        
        //    //            urlSuffix = "/xmlapi/session/begin";

        //    //            Client.UserName = (Communication as GenericSshClient).Username;
        //    //            Client.Password = (Communication as GenericSshClient).Password;

        //    //            break;
        //    //        }
        //    //    case eCommandType.SessionEnd:
        //    //        {
        //    //            urlSuffix = "/xmlapi/session/end";
        //    //            request.Header.SetHeaderValue("Cookie", HttpSessionId);
        //    //            break;
        //    //        }
        //    //    case eCommandType.GetStatus:
        //    //        {
        //    //            request.RequestType = RequestType.Get;
        //    //            request.Header.SetHeaderValue("Content-Type", "text/xml");
        //    //            urlSuffix = "/getxml?location=/Status";
        //    //            break;
        //    //        }
        //    //    case eCommandType.GetConfiguration:
        //    //        {
        //    //            request.RequestType = RequestType.Get;
        //    //            request.Header.SetHeaderValue("Content-Type", "text/xml");
        //    //            urlSuffix = "/getxml?location=/Configuration";
        //    //            break;
        //    //        }
        //    //}

        //    //var requestUrl = CodecUrl + urlSuffix;
        //    //request.Header.RequestVersion = "HTTP/1.1";
        //    //request.Url.Parse(requestUrl);

        //    //Debug.Console(1, this, "Sending HTTP request to Cisco Codec at {0}\nHeader:\n{1}\nContent:\n{2}", requestUrl, request.Header, request.ContentString);

        //    //Client.DispatchAsync(request, PostConnectionCallback);
        //}

        //void PostConnectionCallback(HttpsClientResponse resp, HTTPS_CALLBACK_ERROR err)
        //{
        //    //try
        //    //{
        //    //    if (resp != null)
        //    //    {
        //    //        if (resp.Code == 200)
        //    //        {
        //    //            Debug.Console(1, this, "Http Post to Cisco Codec Successful. Code: {0}\nContent: {1}", resp.Code, resp.ContentString);

        //    //            if (resp.ContentString.IndexOf("<HttpFeedbackRegisterResult status=\"OK\">") > 1)
        //    //            {
        //    //                // Get the initial configruation for sync purposes
        //    //                SendHttpCommand("", eCommandType.GetConfiguration);
        //    //            }
        //    //            else
        //    //            {
        //    //                try
        //    //                {                               
        //    //                    if (resp.ContentString.IndexOf("</Configuration>") > -1)
        //    //                    {
        //    //                        XmlReaderSettings settings = new XmlReaderSettings();

        //    //                        XmlReader reader = new XmlReader(resp.ContentString, settings);

        //    //                        CodecConfiguration = CrestronXMLSerialization.DeSerializeObject<CiscoCodecConfiguration.RootObject>(reader);

        //    //                        //Debug.Console(1, this, "Product Name: {0} Software Version: {1} ApiVersion: {2}", CodecConfiguration.Configuration.Product, CodecConfiguration.Version, CodecConfiguration.ApiVersion);

        //    //                        // Get the initial status for sync purposes
        //    //                        SendHttpCommand("", eCommandType.GetStatus);
        //    //                    }
        //    //                    else if (resp.ContentString.IndexOf("</Status>") > -1)
        //    //                    {
        //    //                        XmlReaderSettings settings = new XmlReaderSettings();

        //    //                        XmlReader reader = new XmlReader(resp.ContentString, settings);

        //    //                        CodecStatus = CrestronXMLSerialization.DeSerializeObject<CiscoCodecStatus.RootObject>(reader);
        //    //                        //Debug.Console(1, this, "Product Name: {0} Software Version: {1} ApiVersion: {2} Volume: {3}", CodecStatus.Product, CodecStatus.Version, CodecStatus.ApiVersion, CodecStatus.Audio.Volume);
        //    //                    }
        //    //                }
        //    //                catch (Exception ex)
        //    //                {
        //    //                    Debug.Console(1, this, "Error Deserializing XML document from codec: {0}", ex);
        //    //                }
        //    //            }
        //    //        }
        //    //        else if (resp.Code == 204)
        //    //        {
        //    //            Debug.Console(1, this, "Response Code: {0}\nHeader:\n{1}Content:\n{1}", resp.Code, resp.Header, resp.ContentString);

        //    //            HttpSessionId = resp.Header.GetHeaderValue("Set-Cookie");
        //    //            //var chunks = HttpSessionId.Split(';');
        //    //            //HttpSessionId = chunks[0];
        //    //            //HttpSessionId = HttpSessionId.Substring(HttpSessionId.IndexOf("=") + 1);


        //    //            // Register for feedbacks once we have a valid session
        //    //            SendHttpCommand(FeedbackRegistrationExpression, eCommandType.Command);
        //    //        }
        //    //        else
        //    //        {
        //    //            Debug.Console(1, this, "Response Code: {0}\nHeader:\n{1}Content:\n{1}Err:\n{2}", resp.Code, resp.Header, resp.ContentString, err);
        //    //        }
        //    //    }
        //    //    else
        //    //        Debug.Console(1, this, "Null response received from server");
        //    //}
        //    //catch (Exception e)
        //    //{
        //    //    Debug.Console(1, this, "Error Initializing HTTPS Client: {0}", e);
        //    //}
        //}

        //void Server_ApiRequest(object sender, Crestron.SimplSharp.Net.Http.OnHttpRequestArgs e)
        //{
        //    Debug.Console(1, this, "Api Reqeust from Codec: {0}", e.Request.ContentString);
        //    e.Response.Code = 200;
        //    e.Response.ContentString = "OK";

        //    DeserializeResponse(e.Request.ContentString);        
        //}

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

                    // Check to see if this is a call status message received after the initial status message
                    if(SyncState.InitialStatusMessageWasReceived && response.IndexOf("\"Call\":{") > -1)
                    {
                        CiscoCodecStatus.RootObject callStatus = new CiscoCodecStatus.RootObject();

                        JsonConvert.PopulateObject(response, callStatus);

                        // Iterate through the call objects in the response
                        foreach (CiscoCodecStatus.Call call in callStatus.Status.Call)
                        {
                            // Look for a matching call id in the existing status structure
                            var existingCall = CodecStatus.Status.Call.FirstOrDefault(c => c.id.Equals(call.id));

                            if (existingCall != null)
                            {
                                // If an existing call object is found with a matching ID, populate the existing call with the new data. 
                                // (must reserialize the object so that we can use PopulateObject() to overlay the new or updated properties on the existing object)
                                JsonConvert.PopulateObject(JsonConvert.SerializeObject(call), existingCall);

                                var tempActiveCall = ActiveCalls.FirstOrDefault(c => c.Id.Equals(call.id));

                                // store previous status to pass to event handler
                                var previousStatus = tempActiveCall.Status; 

                                // Update properties of ActiveCallItem
                                tempActiveCall.Status = CodecCallStatus.ConvertToStatusEnum(call.Status.Value);
                                tempActiveCall.Type = CodecCallType.ConvertToTypeEnum(call.CallType.Value);
                                tempActiveCall.Name = call.DisplayName.Value;

                                SetNewCallStatusAndFireCallStatusChange(previousStatus, tempActiveCall);
                            }
                            else
                            {
                                // Add the call
                                callStatus.Status.Call.Add(call);

                                var newCallItem = new CodecActiveCallItem() 
                                { 
                                    Id = call.id, 
                                    Status = CodecCallStatus.ConvertToStatusEnum(call.Status.Value), 
                                    Name = call.DisplayName.Value, 
                                    Number = call.RemoteNumber.Value,
                                    Type = CodecCallType.ConvertToTypeEnum(call.CallType.Value) 
                                };

                                // Add a call to the ActiveCalls List
                                ActiveCalls.Add(newCallItem);

                                SetNewCallStatusAndFireCallStatusChange(newCallItem.Status, newCallItem);
                            }

                            // Handle call.status to determine if we need to fire an event notifying of an incoming call or a call disconnect
                        }
                    }

                    JsonConvert.PopulateObject(response, CodecStatus);

                    if (!SyncState.InitialStatusMessageWasReceived)
                    {
                        SyncState.InitialStatusMessageReceived();
                        if(!SyncState.InitialConfigurationMessageWasReceived)
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
                        JsonConvert.PopulateObject(response, CodecCallHistory);

                        RecentCalls = CallHistory.ConvertCiscoCallHistoryToGeneric(CodecCallHistory.CommandResponse.CallHistoryRecentsResult.Entry);

                        if (Debug.Level == 1)
                        {
                            
                            Debug.Console(1, this, "RecentCalls:\n");

                            foreach (CallHistory.CallHistoryEntry entry in RecentCalls)
                            {
                                Debug.Console(1, this, "\nName: {0}\nNumber: {1}\nStartTime: {2}\nType: {3}\n", entry.Name, entry.Number, entry.StartTime.ToString(), entry.OccurenceType);
                            }
                        }
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
                var tempCall = CodecStatus.Status.Call.FirstOrDefault(c => c.id.Equals(eventReceived.Event.CallDisconnect.CallId.Value));

                if(tempCall != null)
                {
                    // Remove the call from the xStatus object
                    CodecStatus.Status.Call.Remove(tempCall);

                    var tempActiveCall = ActiveCalls.FirstOrDefault(c => c.Id.Equals(eventReceived.Event.CallDisconnect.CallId.Value));

                    // Remove the call from the Active calls list
                    if (tempActiveCall != null)
                        ActiveCalls.Remove(tempActiveCall);

                    // Notify of the call disconnection
                    SetNewCallStatusAndFireCallStatusChange(eCodecCallStatus.Disconnected, tempActiveCall);
                }
            }
        }

        public override void ExecuteSwitch(object selector)
        {
            (selector as Action)();
        }

        //protected Func<bool> InCallFeedbackFunc { get { return () => false; } }

        protected override Func<bool> IncomingCallFeedbackFunc { get { return () => false; } }

        /// <summary>
        /// Gets the first CallId or returns null
        /// </summary>
        /// <returns></returns>
        private string GetCallId()
        {
            string callId = null;

            if (CodecStatus.Status.Call.Count > 0)
                callId = CodecStatus.Status.Call[0].id;

            return callId;

        }

        public override void  Dial(string s)
        {
         	SendText(string.Format("xCommand Dial Number: \"{0}\"", s));
        }

        public void DialBookingId(string s)
        {
            SendText(string.Format("xCommand Dial BookingId: {0}", s));
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
            SendText(string.Format("xCommand Call DTMFSend CallId: {0} DTMFString: \"{1}\"", GetCallId(), s));
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

        public override void StartSharing()
        {
            string sendingMode = string.Empty;

            if (IsInCall)
                sendingMode = "LocalRemote";
            else
                sendingMode = "LocalOnly";

            SendText(string.Format("xCommand Presentation Start PresentationSource: {0}", PresentationSource));
        }

        public override void StopSharing()
        {
            SendText(string.Format("xCommand Presentation Stop PresentationSource: {0}", PresentationSource));
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
        public void StandbyActivate()
        {
            SendText("xCommand Standby Activate");
        }

        /// <summary>
        /// Wakes the codec from standby
        /// </summary>
        public void StandbyDeactivate()
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

        public void RemoveEntry(CallHistory.CallHistoryEntry entry)
        {
            if (RecentCalls != null)
            {
                RecentCalls.Remove(entry);
            }
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