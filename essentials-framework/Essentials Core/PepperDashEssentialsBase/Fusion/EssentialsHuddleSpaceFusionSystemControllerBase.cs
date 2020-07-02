using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Crestron.SimplSharp;
using Crestron.SimplSharp.CrestronIO;
using Crestron.SimplSharp.CrestronXml;
using Crestron.SimplSharp.CrestronXml.Serialization;
using Crestron.SimplSharpPro;
using Crestron.SimplSharpPro.Fusion;
using Newtonsoft.Json;
using PepperDash.Core;
using PepperDash.Essentials.Core.Config;
using PepperDash.Essentials.Core.Rooms;

namespace PepperDash.Essentials.Core.Fusion
{
    public class EssentialsHuddleSpaceFusionSystemControllerBase : EssentialsDevice, IOccupancyStatusProvider
    {
        private const string RemoteOccupancyXml = "<Occupancy><Type>Local</Type><State>{0}</State></Occupancy>";
        private readonly bool _guidFileExists;

        private readonly Dictionary<Device, BoolInputSig> _sourceToFeedbackSigs =
            new Dictionary<Device, BoolInputSig>();

        private Event _currentMeeting;

        protected StringSigData CurrentRoomSourceNameSig;

        private RoomSchedule _currentSchedule;
        public FusionCustomPropertiesBridge CustomPropertiesBridge = new FusionCustomPropertiesBridge();
        private CTimer _dailyTimeRequestTimer;
        protected FusionOccupancySensorAsset FusionOccSensor;
        protected FusionRemoteOccupancySensor FusionRemoteOccSensor;
        protected FusionRoom FusionRoom;
        protected Dictionary<int, FusionAsset> FusionStaticAssets;

        private FusionRoomGuids _guiDs;
        private uint _ipId;

        private bool _isRegisteredForSchedulePushNotifications;
        private Event _nextMeeting;

        private CTimer _pollTimer;
        public long PushNotificationTimeout = 5000;

        private CTimer _pushNotificationTimer;
        protected EssentialsRoomBase Room;

        // Default poll time is 5 min unless overridden by config value
        public long SchedulePollInterval = 300000;
        private StatusMonitorCollection _errorMessageRollUp;

        private string _roomOccupancyRemoteString;

        #region System Info Sigs

        //StringSigData SystemName;
        //StringSigData Model;
        //StringSigData SerialNumber;
        //StringSigData Uptime;

        #endregion

        #region Processor Info Sigs

        private readonly StringSigData[] _program = new StringSigData[10];
        private StringSigData _dns1;
        private StringSigData _dns2;
        private StringSigData _domain;
        private StringSigData _firmware;
        private StringSigData _gateway;
        private StringSigData _hostname;
        private StringSigData _ip1;
        private StringSigData _ip2;
        private StringSigData _mac1;
        private StringSigData _mac2;
        private StringSigData _netMask1;
        private StringSigData _netMask2;

        #endregion

        #region Default Display Source Sigs

        private BooleanSigData[] _source = new BooleanSigData[10];

        #endregion

        public EssentialsHuddleSpaceFusionSystemControllerBase(EssentialsRoomBase room, uint ipId)
            : base(room.Key + "-fusion")
        {
            try
            {
                Room = room;

                _ipId = ipId;

                FusionStaticAssets = new Dictionary<int, FusionAsset>();

                _guiDs = new FusionRoomGuids();

                var mac =
                    CrestronEthernetHelper.GetEthernetParameter(
                        CrestronEthernetHelper.ETHERNET_PARAMETER_TO_GET.GET_MAC_ADDRESS, 0);

                var slot = Global.ControlSystem.ProgramNumber;

                var guidFilePath = Global.FilePathPrefix +
                                      string.Format(@"{0}-FusionGuids.json", InitialParametersClass.ProgramIDTag);

                _guidFileExists = File.Exists(guidFilePath);

                // Check if file exists
                if (!_guidFileExists)
                {
                    // Does not exist. Create GUIDs
                    _guiDs = new FusionRoomGuids(Room.Name, ipId, _guiDs.GenerateNewRoomGuid(slot, mac),
                        FusionStaticAssets);
                }
                else
                {
                    // Exists. Read GUIDs
                    ReadGuidFile(guidFilePath);
                }

                Initialize();

                if (Room.RoomOccupancy != null)
                {
                    if (Room.OccupancyStatusProviderIsRemote)
                    {
                        SetUpRemoteOccupancy();
                    }
                    else
                    {
                        SetUpLocalOccupancy();
                    }
                }

                // Make it so!   
                FusionRVI.GenerateFileForAllFusionDevices();

                GenerateGuidFile(guidFilePath);
            }
            catch (Exception e)
            {
                Debug.Console(0, this, Debug.ErrorLogLevel.Error, "Error Building Fusion System Controller: {0}", e);
            }
        }

        protected string RoomGuid
        {
            get { return _guiDs.RoomGuid; }
        }

        public StringFeedback RoomOccupancyRemoteStringFeedback { get; private set; }

        protected Func<bool> RoomIsOccupiedFeedbackFunc
        {
            get { return () => FusionRemoteOccSensor.RoomOccupied.OutputSig.BoolValue; }
        }

        #region IOccupancyStatusProvider Members

        public BoolFeedback RoomIsOccupiedFeedback { get; private set; }

        #endregion

        public event EventHandler<ScheduleChangeEventArgs> ScheduleChange;
        //public event EventHandler<MeetingChangeEventArgs> MeetingEndWarning;
        //public event EventHandler<MeetingChangeEventArgs> NextMeetingBeginWarning;

        public event EventHandler<EventArgs> RoomInfoChange;

        //ScheduleResponseEvent NextMeeting;

        /// <summary>
        /// Used for extension classes to execute whatever steps are necessary before generating the RVI and GUID files
        /// </summary>
        protected virtual void ExecuteCustomSteps()
        {
        }

        protected void Initialize()
        {
            CreateSymbolAndBasicSigs(_ipId);
            SetUpSources();
            SetUpCommunitcationMonitors();
            SetUpDisplay();
            SetUpError();
            ExecuteCustomSteps();
        }

        /// <summary>
        /// Generates the guid file in NVRAM.  If the file already exists it will be overwritten.
        /// </summary>
        /// <param name="filePath">path for the file</param>
        private void GenerateGuidFile(string filePath)
        {
            if (string.IsNullOrEmpty(filePath))
            {
                Debug.Console(0, this, "Error writing guid file.  No path specified.");
                return;
            }

            var fileLock = new CCriticalSection();

            try
            {
                if (fileLock.Disposed)
                {
                    return;
                }

                fileLock.Enter();

                Debug.Console(1, this, "Writing GUIDs to file");

                _guiDs = FusionOccSensor == null
                    ? new FusionRoomGuids(Room.Name, _ipId, RoomGuid, FusionStaticAssets)
                    : new FusionRoomGuids(Room.Name, _ipId, RoomGuid, FusionStaticAssets, FusionOccSensor);

                var json = JsonConvert.SerializeObject(_guiDs, Newtonsoft.Json.Formatting.Indented);

                using (var sw = new StreamWriter(filePath))
                {
                    sw.Write(json);
                    sw.Flush();
                }

                Debug.Console(1, this, "Guids successfully written to file '{0}'", filePath);
            }
            catch (Exception e)
            {
                Debug.Console(0, this, "Error writing guid file: {0}", e);
            }
            finally
            {
                if (!fileLock.Disposed)
                {
                    fileLock.Leave();
                }
            }
        }

        /// <summary>
        /// Reads the guid file from NVRAM
        /// </summary>
        /// <param name="filePath">path for te file</param>
        private void ReadGuidFile(string filePath)
        {
            if (string.IsNullOrEmpty(filePath))
            {
                Debug.Console(0, this, Debug.ErrorLogLevel.Notice, "Error reading guid file.  No path specified.");
                return;
            }

            var fileLock = new CCriticalSection();

            try
            {
                if (fileLock.Disposed)
                {
                    return;
                }

                fileLock.Enter();

                if (File.Exists(filePath))
                {
                    var json = File.ReadToEnd(filePath, Encoding.ASCII);

                    _guiDs = JsonConvert.DeserializeObject<FusionRoomGuids>(json);

                    _ipId = _guiDs.IpId;

                    FusionStaticAssets = _guiDs.StaticAssets;
                }

                Debug.Console(0, this, Debug.ErrorLogLevel.Notice, "Fusion Guids successfully read from file: {0}",
                    filePath);

                Debug.Console(1, this, "\nRoom Name: {0}\nIPID: {1:x}\n RoomGuid: {2}", Room.Name, _ipId, RoomGuid);

                foreach (var item in FusionStaticAssets)
                {
                    Debug.Console(1, this, "\nAsset Name: {0}\nAsset No: {1}\n Guid: {2}", item.Value.Name,
                        item.Value.SlotNumber, item.Value.InstanceId);
                }
            }
            catch (Exception e)
            {
                Debug.Console(0, this, Debug.ErrorLogLevel.Notice, "Error reading guid file: {0}", e);
            }
            finally
            {
                if (!fileLock.Disposed)
                {
                    fileLock.Leave();
                }
            }
        }

        protected virtual void CreateSymbolAndBasicSigs(uint ipId)
        {
            Debug.Console(0, this, Debug.ErrorLogLevel.Notice, "Creating Fusion Room symbol with GUID: {0}", RoomGuid);

            FusionRoom = new FusionRoom(ipId, Global.ControlSystem, Room.Name, RoomGuid);
            FusionRoom.ExtenderRoomViewSchedulingDataReservedSigs.Use();
            FusionRoom.ExtenderFusionRoomDataReservedSigs.Use();

            FusionRoom.Register();

            FusionRoom.FusionStateChange += FusionRoom_FusionStateChange;

            FusionRoom.ExtenderRoomViewSchedulingDataReservedSigs.DeviceExtenderSigChange +=
                FusionRoomSchedule_DeviceExtenderSigChange;
            FusionRoom.ExtenderFusionRoomDataReservedSigs.DeviceExtenderSigChange +=
                ExtenderFusionRoomDataReservedSigs_DeviceExtenderSigChange;
            FusionRoom.OnlineStatusChange += FusionRoom_OnlineStatusChange;

            CrestronConsole.AddNewConsoleCommand(RequestFullRoomSchedule, "FusReqRoomSchedule",
                "Requests schedule of the room for the next 24 hours", ConsoleAccessLevelEnum.AccessOperator);
            CrestronConsole.AddNewConsoleCommand(ModifyMeetingEndTimeConsoleHelper, "FusReqRoomSchMod",
                "Ends or extends a meeting by the specified time", ConsoleAccessLevelEnum.AccessOperator);
            CrestronConsole.AddNewConsoleCommand(CreateAdHocMeeting, "FusCreateMeeting",
                "Creates and Ad Hoc meeting for on hour or until the next meeting",
                ConsoleAccessLevelEnum.AccessOperator);

            // Room to fusion room
            Room.OnFeedback.LinkInputSig(FusionRoom.SystemPowerOn.InputSig);

            // Moved to 
            CurrentRoomSourceNameSig = FusionRoom.CreateOffsetStringSig(84, "Display 1 - Current Source",
                eSigIoMask.InputSigOnly);
            // Don't think we need to get current status of this as nothing should be alive yet. 
            var hasCurrentSourceInfoChange = Room as IHasCurrentSourceInfoChange;
            if (hasCurrentSourceInfoChange != null)
            {
                hasCurrentSourceInfoChange.CurrentSourceChange +=
                    Room_CurrentSourceInfoChange;
            }


            FusionRoom.SystemPowerOn.OutputSig.SetSigFalseAction(
                Room.PowerOnToDefaultOrLastSource);
            FusionRoom.SystemPowerOff.OutputSig.SetSigFalseAction(
                () =>
                {
                    var runRouteAction = Room as IRunRouteAction;
                    if (runRouteAction != null)
                          {
                              runRouteAction.RunRouteAction("roomOff", Room.SourceListKey);
                          }
                });
            // NO!! room.RoomIsOn.LinkComplementInputSig(FusionRoom.SystemPowerOff.InputSig);
            FusionRoom.ErrorMessage.InputSig.StringValue =
                "3: 7 Errors: This is a really long error message;This is a really long error message;This is a really long error message;This is a really long error message;This is a really long error message;This is a really long error message;This is a really long error message;";

            SetUpEthernetValues();

            GetProcessorEthernetValues();

            GetSystemInfo();

            GetProcessorInfo();

            CrestronEnvironment.EthernetEventHandler +=
                CrestronEnvironment_EthernetEventHandler;
        }

        protected void CrestronEnvironment_EthernetEventHandler(EthernetEventArgs ethernetEventArgs)
        {
            if (ethernetEventArgs.EthernetEventType == eEthernetEventType.LinkUp)
            {
                GetProcessorEthernetValues();
            }
        }

        protected void GetSystemInfo()
        {
            //SystemName.InputSig.StringValue = Room.Name;
            //Model.InputSig.StringValue = InitialParametersClass.ControllerPromptName;
            //SerialNumber.InputSig.StringValue = InitialParametersClass.

            var response = string.Empty;

            var systemReboot = FusionRoom.CreateOffsetBoolSig(74, "Processor - Reboot", eSigIoMask.OutputSigOnly);
            systemReboot.OutputSig.SetSigFalseAction(
                () => CrestronConsole.SendControlSystemCommand("reboot", ref response));
        }

        protected void SetUpEthernetValues()
        {
            _ip1 = FusionRoom.CreateOffsetStringSig(50, "Info - Processor - IP 1", eSigIoMask.InputSigOnly);
            _ip2 = FusionRoom.CreateOffsetStringSig(51, "Info - Processor - IP 2", eSigIoMask.InputSigOnly);
            _gateway = FusionRoom.CreateOffsetStringSig(52, "Info - Processor - Gateway", eSigIoMask.InputSigOnly);
            _hostname = FusionRoom.CreateOffsetStringSig(53, "Info - Processor - Hostname", eSigIoMask.InputSigOnly);
            _domain = FusionRoom.CreateOffsetStringSig(54, "Info - Processor - Domain", eSigIoMask.InputSigOnly);
            _dns1 = FusionRoom.CreateOffsetStringSig(55, "Info - Processor - DNS 1", eSigIoMask.InputSigOnly);
            _dns2 = FusionRoom.CreateOffsetStringSig(56, "Info - Processor - DNS 2", eSigIoMask.InputSigOnly);
            _mac1 = FusionRoom.CreateOffsetStringSig(57, "Info - Processor - MAC 1", eSigIoMask.InputSigOnly);
            _mac2 = FusionRoom.CreateOffsetStringSig(58, "Info - Processor - MAC 2", eSigIoMask.InputSigOnly);
            _netMask1 = FusionRoom.CreateOffsetStringSig(59, "Info - Processor - Net Mask 1", eSigIoMask.InputSigOnly);
            _netMask2 = FusionRoom.CreateOffsetStringSig(60, "Info - Processor - Net Mask 2", eSigIoMask.InputSigOnly);
        }

        protected void GetProcessorEthernetValues()
        {
            _ip1.InputSig.StringValue =
                CrestronEthernetHelper.GetEthernetParameter(
                    CrestronEthernetHelper.ETHERNET_PARAMETER_TO_GET.GET_CURRENT_IP_ADDRESS, 0);
            _gateway.InputSig.StringValue =
                CrestronEthernetHelper.GetEthernetParameter(
                    CrestronEthernetHelper.ETHERNET_PARAMETER_TO_GET.GET_CURRENT_ROUTER, 0);
            _hostname.InputSig.StringValue =
                CrestronEthernetHelper.GetEthernetParameter(
                    CrestronEthernetHelper.ETHERNET_PARAMETER_TO_GET.GET_HOSTNAME, 0);
            _domain.InputSig.StringValue =
                CrestronEthernetHelper.GetEthernetParameter(
                    CrestronEthernetHelper.ETHERNET_PARAMETER_TO_GET.GET_DOMAIN_NAME, 0);

            var dnsServers =
                CrestronEthernetHelper.GetEthernetParameter(
                    CrestronEthernetHelper.ETHERNET_PARAMETER_TO_GET.GET_DNS_SERVER, 0).Split(',');
            _dns1.InputSig.StringValue = dnsServers[0];
            if (dnsServers.Length > 1)
            {
                _dns2.InputSig.StringValue = dnsServers[1];
            }

            _mac1.InputSig.StringValue =
                CrestronEthernetHelper.GetEthernetParameter(
                    CrestronEthernetHelper.ETHERNET_PARAMETER_TO_GET.GET_MAC_ADDRESS, 0);
            _netMask1.InputSig.StringValue =
                CrestronEthernetHelper.GetEthernetParameter(
                    CrestronEthernetHelper.ETHERNET_PARAMETER_TO_GET.GET_CURRENT_IP_MASK, 0);

            // Interface 1

            if (InitialParametersClass.NumberOfEthernetInterfaces > 1)
                // Only get these values if the processor has more than 1 NIC
            {
                _ip2.InputSig.StringValue =
                    CrestronEthernetHelper.GetEthernetParameter(
                        CrestronEthernetHelper.ETHERNET_PARAMETER_TO_GET.GET_CURRENT_IP_ADDRESS, 1);
                _mac2.InputSig.StringValue =
                    CrestronEthernetHelper.GetEthernetParameter(
                        CrestronEthernetHelper.ETHERNET_PARAMETER_TO_GET.GET_MAC_ADDRESS, 1);
                _netMask2.InputSig.StringValue =
                    CrestronEthernetHelper.GetEthernetParameter(
                        CrestronEthernetHelper.ETHERNET_PARAMETER_TO_GET.GET_CURRENT_IP_MASK, 1);
            }
        }

        protected void GetProcessorInfo()
        {
            _firmware = FusionRoom.CreateOffsetStringSig(61, "Info - Processor - Firmware", eSigIoMask.InputSigOnly);

            if (CrestronEnvironment.DevicePlatform != eDevicePlatform.Server)
            {
                for (var i = 0; i < Global.ControlSystem.NumProgramsSupported; i++)
                {
                    var join = 62 + i;
                    var progNum = i + 1;
                    _program[i] = FusionRoom.CreateOffsetStringSig((uint) join,
                        string.Format("Info - Processor - Program {0}", progNum), eSigIoMask.InputSigOnly);
                }
            }

            _firmware.InputSig.StringValue = InitialParametersClass.FirmwareVersion;
        }

        protected void GetCustomProperties()
        {
            if (FusionRoom.IsOnline)
            {
                const string fusionRoomCustomPropertiesRequest = @"<RequestRoomConfiguration><RequestID>RoomConfigurationRequest</RequestID><CustomProperties><Property></Property></CustomProperties></RequestRoomConfiguration>";

                FusionRoom.ExtenderFusionRoomDataReservedSigs.RoomConfigQuery.StringValue =
                    fusionRoomCustomPropertiesRequest;
            }
        }

        private void GetTouchpanelInfo()
        {
            // TODO: Get IP and Project Name from TP
        }

        protected void FusionRoom_OnlineStatusChange(GenericBase currentDevice, OnlineOfflineEventArgs args)
        {
            if (args.DeviceOnLine)
            {
                CrestronEnvironment.Sleep(200);

                // Send Push Notification Action request:

                const string requestId = "InitialPushRequest";


                var actionRequest =
                    string.Format("<RequestAction>\n<RequestID>{0}</RequestID>\n", requestId) +
                    "<ActionID>RegisterPushModel</ActionID>\n" +
                    "<Parameters>\n" +
                    "<Parameter ID='Enabled' Value='1' />\n" +
                    "<Parameter ID='RequestID' Value='PushNotification' />\n" +
                    "<Parameter ID='Start' Value='00:00:00' />\n" +
                    "<Parameter ID='HourSpan' Value='24' />\n" +
                    "<Parameter ID='Field' Value='MeetingID' />\n" +
                    "<Parameter ID='Field' Value='RVMeetingID' />\n" +
                    "<Parameter ID='Field' Value='InstanceID' />\n" +
                    "<Parameter ID='Field' Value='dtStart' />\n" +
                    "<Parameter ID='Field' Value='dtEnd' />\n" +
                    "<Parameter ID='Field' Value='Subject' />\n" +
                    "<Parameter ID='Field' Value='Organizer' />\n" +
                    "<Parameter ID='Field' Value='IsEvent' />\n" +
                    "<Parameter ID='Field' Value='IsPrivate' />\n" +
                    "<Parameter ID='Field' Value='IsExchangePrivate' />\n" +
                    "<Parameter ID='Field' Value='LiveMeeting' />\n" +
                    "<Parameter ID='Field' Value='ShareDocPath' />\n" +
                    "<Parameter ID='Field' Value='PhoneNo' />\n" +
                    "<Parameter ID='Field' Value='ParticipantCode' />\n" +
                    "</Parameters>\n" +
                    "</RequestAction>\n";

                Debug.Console(2, this, "Sending Fusion ActionRequest: \n{0}", actionRequest);

                FusionRoom.ExtenderFusionRoomDataReservedSigs.ActionQuery.StringValue = actionRequest;

                GetCustomProperties();

                // Request current Fusion Server Time
                RequestLocalDateTime(null);

                // Setup timer to request time daily
                if (_dailyTimeRequestTimer != null && !_dailyTimeRequestTimer.Disposed)
                {
                    _dailyTimeRequestTimer.Stop();
                    _dailyTimeRequestTimer.Dispose();
                }

                _dailyTimeRequestTimer = new CTimer(RequestLocalDateTime, null, 86400000, 86400000);

                _dailyTimeRequestTimer.Reset(86400000, 86400000);
            }
        }

        /// <summary>
        /// Requests the local date and time from the Fusion Server
        /// </summary>
        /// <param name="callbackObject"></param>
        public void RequestLocalDateTime(object callbackObject)
        {
            const string timeRequestId = "TimeRequest";

            var timeRequest = string.Format("<LocalTimeRequest><RequestID>{0}</RequestID></LocalTimeRequest>",
                timeRequestId);

            FusionRoom.ExtenderFusionRoomDataReservedSigs.LocalDateTimeQuery.StringValue = timeRequest;
        }

        /// <summary>
        /// Generates a room schedule request for this room for the next 24 hours.
        /// </summary>
        public void RequestFullRoomSchedule(object callbackObject)
        {
            var now = DateTime.Today;

            var currentTime = now.ToString("s");

            var requestTest =
                string.Format(
                    "<RequestSchedule><RequestID>FullSchedleRequest</RequestID><RoomID>{0}</RoomID><Start>{1}</Start><HourSpan>24</HourSpan></RequestSchedule>",
                    RoomGuid, currentTime);

            Debug.Console(2, this, "Sending Fusion ScheduleQuery: \n{0}", requestTest);

            FusionRoom.ExtenderRoomViewSchedulingDataReservedSigs.ScheduleQuery.StringValue = requestTest;

            if (_isRegisteredForSchedulePushNotifications)
            {
                _pushNotificationTimer.Stop();
            }
        }

        /// <summary>
        /// Wrapper method to allow console commands to modify the current meeting end time
        /// </summary>
        /// <param name="command">meetingID extendTime</param>
        public void ModifyMeetingEndTimeConsoleHelper(string command)
        {
            var extendMinutes = -1;

            const string requestId = "ModifyMeetingTest12345";

            try
            {
                var tokens = command.Split(' ');

                //var meetingId = tokens[0]; never used, commenting out for now
                extendMinutes = Int32.Parse(tokens[1]);
            }
            catch (Exception e)
            {
                Debug.Console(1, this, "Error parsing console command: {0}", e);
            }

            ModifyMeetingEndTime(requestId, extendMinutes);
        }

        /// <summary>
        /// Ends or Extends the current meeting by the specified number of minutes.
        /// </summary>
        /// <param name="requestId"></param>
        /// <param name="extendMinutes">Number of minutes to extend the meeting.  A value of 0 will end the meeting.</param>
        public void ModifyMeetingEndTime(string requestId, int extendMinutes)
        {
            if (_currentMeeting == null)
            {
                Debug.Console(1, this, "No meeting in progress.  Unable to modify end time.");
                return;
            }

            if (extendMinutes > -1)
            {
                if (extendMinutes > 0)
                {
                    var extendTime = _currentMeeting.dtEnd - DateTime.Now;
                    var extendMinutesRaw = extendTime.TotalMinutes;

                    extendMinutes = extendMinutes + (int) Math.Round(extendMinutesRaw);
                }


                var requestTest = string.Format(
                    "<RequestAction><RequestID>{0}</RequestID><RoomID>{1}</RoomID><ActionID>MeetingChange</ActionID><Parameters><Parameter ID = 'MeetingID' Value = '{2}' /><Parameter ID = 'EndTime' Value = '{3}' /></Parameters></RequestAction>"
                    , requestId, RoomGuid, _currentMeeting.MeetingID, extendMinutes);

                Debug.Console(1, this, "Sending MeetingChange Request: \n{0}", requestTest);

                FusionRoom.ExtenderFusionRoomDataReservedSigs.ActionQuery.StringValue = requestTest;
            }
            else
            {
                Debug.Console(1, this, "Invalid time specified");
            }
        }

        /// <summary>
        /// Creates and Ad Hoc meeting with a duration of 1 hour, or until the next meeting if in less than 1 hour.
        /// </summary>
        public void CreateAdHocMeeting(string command)
        {
            const string requestId = "CreateAdHocMeeting";

            var now = DateTime.Now.AddMinutes(1);

            now.AddSeconds(-now.Second);

            // Assume 1 hour meeting if possible
            var dtEnd = now.AddHours(1);

            // Check if room is available for 1 hour before next meeting
            if (_nextMeeting != null)
            {
                var roomAvailable = _nextMeeting.dtEnd.Subtract(dtEnd);

                if (roomAvailable.TotalMinutes < 60)
                {
                    // Room not available for full hour, book until next meeting starts
                    dtEnd = _nextMeeting.dtEnd;
                }
            }

            var createMeetingRequest =
                "<CreateSchedule>" +
                string.Format("<RequestID>{0}</RequestID>", requestId) +
                string.Format("<RoomID>{0}</RoomID>", RoomGuid) +
                "<Event>" +
                string.Format("<dtStart>{0}</dtStart>", now.ToString("s")) +
                string.Format("<dtEnd>{0}</dtEnd>", dtEnd.ToString("s")) +
                "<Subject>AdHoc Meeting</Subject>" +
                "<Organizer>Room User</Organizer>" +
                "<WelcomMsg>Example Message</WelcomMsg>" +
                "</Event>" +
                "</CreateSchedule>";

            Debug.Console(2, this, "Sending CreateMeeting Request: \n{0}", createMeetingRequest);

            FusionRoom.ExtenderRoomViewSchedulingDataReservedSigs.CreateMeeting.StringValue = createMeetingRequest;

            //Debug.Console(1, this, "Sending CreateMeeting Request: \n{0}", command);

            //FusionRoom.ExtenderRoomViewSchedulingDataReservedSigs.CreateMeeting.StringValue = command;
        }

        /// <summary>
        /// Event handler method for Device Extender sig changes
        /// </summary>
        /// <param name="currentDeviceExtender"></param>
        /// <param name="args"></param>
        protected void ExtenderFusionRoomDataReservedSigs_DeviceExtenderSigChange(DeviceExtender currentDeviceExtender,
            SigEventArgs args)
        {
            Debug.Console(2, this, "Event: {0}\n Sig: {1}\nFusionResponse:\n{2}", args.Event, args.Sig.Name,
                args.Sig.StringValue);


            if (args.Sig == FusionRoom.ExtenderFusionRoomDataReservedSigs.ActionQueryResponse)
            {
                try
                {
                    var message = new XmlDocument();

                    message.LoadXml(args.Sig.StringValue);

                    var actionResponse = message["ActionResponse"];

                    if (actionResponse == null)
                    {
                        return;
                    }
                    var requestId = actionResponse["RequestID"];

                    if (requestId.InnerText != "InitialPushRequest")
                    {
                        return;
                    }
                    if (actionResponse["ActionID"].InnerText != "RegisterPushModel")
                    {
                        return;
                    }
                    var parameters = actionResponse["Parameters"];

                    foreach (var isRegistered in from XmlElement parameter in parameters
                        where parameter.HasAttributes
                        select parameter.Attributes
                        into attributes
                        where attributes["ID"].Value == "Registered"
                        select Int32.Parse(attributes["Value"].Value))
                    {
                        switch (isRegistered)
                        {
                            case 1:
                                _isRegisteredForSchedulePushNotifications = true;
                                if (_pollTimer != null && !_pollTimer.Disposed)
                                {
                                    _pollTimer.Stop();
                                    _pollTimer.Dispose();
                                }
                                _pushNotificationTimer = new CTimer(RequestFullRoomSchedule, null,
                                    PushNotificationTimeout, PushNotificationTimeout);
                                _pushNotificationTimer.Reset(PushNotificationTimeout,
                                    PushNotificationTimeout);
                                break;
                            case 0:
                                _isRegisteredForSchedulePushNotifications = false;
                                if (_pushNotificationTimer != null && !_pushNotificationTimer.Disposed)
                                {
                                    _pushNotificationTimer.Stop();
                                    _pushNotificationTimer.Dispose();
                                }
                                _pollTimer = new CTimer(RequestFullRoomSchedule, null,
                                    SchedulePollInterval, SchedulePollInterval);
                                _pollTimer.Reset(SchedulePollInterval, SchedulePollInterval);
                                break;
                        }
                    }
                }
                catch (Exception e)
                {
                    Debug.Console(1, this, "Error parsing ActionQueryResponse: {0}", e);
                }
            }
            else if (args.Sig == FusionRoom.ExtenderFusionRoomDataReservedSigs.LocalDateTimeQueryResponse)
            {
                try
                {
                    var message = new XmlDocument();

                    message.LoadXml(args.Sig.StringValue);

                    var localDateTimeResponse = message["LocalTimeResponse"];

                    if (localDateTimeResponse != null)
                    {
                        var localDateTime = localDateTimeResponse["LocalDateTime"];

                        if (localDateTime != null)
                        {
                            var tempLocalDateTime = localDateTime.InnerText;

                            var currentTime = DateTime.Parse(tempLocalDateTime);

                            Debug.Console(1, this, "DateTime from Fusion Server: {0}", currentTime);

                            // Parse time and date from response and insert values
                            CrestronEnvironment.SetTimeAndDate((ushort) currentTime.Hour, (ushort) currentTime.Minute,
                                (ushort) currentTime.Second, (ushort) currentTime.Month, (ushort) currentTime.Day,
                                (ushort) currentTime.Year);

                            Debug.Console(1, this, "Processor time set to {0}", CrestronEnvironment.GetLocalTime());
                        }
                    }
                }
                catch (Exception e)
                {
                    Debug.Console(1, this, "Error parsing LocalDateTimeQueryResponse: {0}", e);
                }
            }
            else if (args.Sig == FusionRoom.ExtenderFusionRoomDataReservedSigs.RoomConfigResponse)
            {
                // Room info response with custom properties

                var roomConfigResponseArgs = args.Sig.StringValue.Replace("&", "and");

                Debug.Console(2, this, "Fusion Response: \n {0}", roomConfigResponseArgs);

                try
                {
                    var roomConfigResponse = new XmlDocument();

                    roomConfigResponse.LoadXml(roomConfigResponseArgs);

                    var requestRoomConfiguration = roomConfigResponse["RoomConfigurationResponse"];

                    if (requestRoomConfiguration != null)
                    {
                        var roomInformation = new RoomInformation();

                        foreach (XmlElement e in roomConfigResponse.FirstChild.ChildNodes)
                        {
                            if (e.Name == "RoomInformation")
                            {
                                var roomInfo = new XmlReader(e.OuterXml);

                                roomInformation = CrestronXMLSerialization.DeSerializeObject<RoomInformation>(roomInfo);
                            }
                            else if (e.Name == "CustomFields")
                            {
                                foreach (XmlElement el in e)
                                {
                                    var customProperty = new FusionCustomProperty();

                                    if (el.Name == "CustomField")
                                    {
                                        customProperty.Id = el.Attributes["ID"].Value;
                                    }

                                    foreach (XmlElement elm in el)
                                    {
                                        if (elm.Name == "CustomFieldName")
                                        {
                                            customProperty.CustomFieldName = elm.InnerText;
                                        }
                                        if (elm.Name == "CustomFieldType")
                                        {
                                            customProperty.CustomFieldType = elm.InnerText;
                                        }
                                        if (elm.Name == "CustomFieldValue")
                                        {
                                            customProperty.CustomFieldValue = elm.InnerText;
                                        }
                                    }

                                    roomInformation.FusionCustomProperties.Add(customProperty);
                                }
                            }
                        }

                        var handler = RoomInfoChange;
                        if (handler != null)
                        {
                            handler(this, new EventArgs());
                        }

                        CustomPropertiesBridge.EvaluateRoomInfo(Room.Key, roomInformation);
                    }
                }
                catch (Exception e)
                {
                    Debug.Console(1, this, "Error parsing Custom Properties response: {0}", e);
                }
                //PrintRoomInfo();
                //getRoomInfoBusy = false;
                //_DynFusion.API.EISC.BooleanInput[Constants.GetRoomInfo].BoolValue = getRoomInfoBusy;
            }
        }

        /// <summary>
        /// Event handler method for Device Extender sig changes
        /// </summary>
        /// <param name="currentDeviceExtender"></param>
        /// <param name="args"></param>
        protected void FusionRoomSchedule_DeviceExtenderSigChange(DeviceExtender currentDeviceExtender,
            SigEventArgs args)
        {
            Debug.Console(2, this, "Scehdule Response Event: {0}\n Sig: {1}\nFusionResponse:\n{2}", args.Event,
                args.Sig.Name, args.Sig.StringValue);


            if (args.Sig == FusionRoom.ExtenderRoomViewSchedulingDataReservedSigs.ScheduleResponse)
            {
                try
                {
                    var scheduleResponse = new ScheduleResponse();

                    var message = new XmlDocument();

                    message.LoadXml(args.Sig.StringValue);

                    var response = message["ScheduleResponse"];

                    if (response != null)
                    {
                        // Check for push notification
                        if (response["RequestID"].InnerText == "RVRequest")
                        {
                            var action = response["Action"];

                            if (action.OuterXml.IndexOf("RequestSchedule", StringComparison.Ordinal) > -1)
                            {
                                _pushNotificationTimer.Reset(PushNotificationTimeout, PushNotificationTimeout);
                            }
                        }
                        else // Not a push notification
                        {
                            _currentSchedule = new RoomSchedule(); // Clear Current Schedule
                            _currentMeeting = null; // Clear Current Meeting
                            _nextMeeting = null; // Clear Next Meeting

                            var isNextMeeting = false;

                            foreach (XmlElement element in message.FirstChild.ChildNodes)
                            {
                                if (element.Name == "RequestID")
                                {
                                    scheduleResponse.RequestID = element.InnerText;
                                }
                                else if (element.Name == "RoomID")
                                {
                                    scheduleResponse.RoomID = element.InnerText;
                                }
                                else if (element.Name == "RoomName")
                                {
                                    scheduleResponse.RoomName = element.InnerText;
                                }
                                else if (element.Name == "Event")
                                {
                                    Debug.Console(2, this, "Event Found:\n{0}", element.OuterXml);

                                    var reader = new XmlReader(element.OuterXml);

                                    var tempEvent = CrestronXMLSerialization.DeSerializeObject<Event>(reader);

                                    scheduleResponse.Events.Add(tempEvent);

                                    // Check is this is the current event
                                    if (tempEvent.dtStart <= DateTime.Now && tempEvent.dtEnd >= DateTime.Now)
                                    {
                                        _currentMeeting = tempEvent; // Set Current Meeting
                                        isNextMeeting = true; // Flag that next element is next meeting
                                    }

                                    if (isNextMeeting)
                                    {
                                        _nextMeeting = tempEvent; // Set Next Meeting
                                        isNextMeeting = false;
                                    }

                                    _currentSchedule.Meetings.Add(tempEvent);
                                }
                            }

                            PrintTodaysSchedule();

                            if (!_isRegisteredForSchedulePushNotifications)
                            {
                                _pollTimer.Reset(SchedulePollInterval, SchedulePollInterval);
                            }

                            // Fire Schedule Change Event 
                            var handler = ScheduleChange;

                            if (handler != null)
                            {
                                handler(this, new ScheduleChangeEventArgs {Schedule = _currentSchedule});
                            }
                        }
                    }
                }
                catch (Exception e)
                {
                    Debug.Console(1, this, "Error parsing ScheduleResponse: {0}", e);
                }
            }
            else if (args.Sig == FusionRoom.ExtenderRoomViewSchedulingDataReservedSigs.CreateResponse)
            {
                Debug.Console(2, this, "Create Meeting Response Event: {0}\n Sig: {1}\nFusionResponse:\n{2}", args.Event,
                    args.Sig.Name, args.Sig.StringValue);
            }
        }

        /// <summary>
        /// Prints today's schedule to console for debugging
        /// </summary>
        private void PrintTodaysSchedule()
        {
            if (Debug.Level > 1)
            {
                if (_currentSchedule.Meetings.Count > 0)
                {
                    Debug.Console(1, this, "Today's Schedule for '{0}'\n", Room.Name);

                    foreach (var e in _currentSchedule.Meetings)
                    {
                        Debug.Console(1, this, "Subject: {0}", e.Subject);
                        Debug.Console(1, this, "Organizer: {0}", e.Organizer);
                        Debug.Console(1, this, "MeetingID: {0}", e.MeetingID);
                        Debug.Console(1, this, "Start Time: {0}", e.dtStart);
                        Debug.Console(1, this, "End Time: {0}", e.dtEnd);
                        Debug.Console(1, this, "Duration: {0}\n", e.DurationInMinutes);
                    }
                }
            }
        }

        protected virtual void SetUpSources()
        {
            // Sources
            var dict = ConfigReader.ConfigObject.GetSourceListForKey(Room.SourceListKey);
            if (dict != null)
            {
                // NEW PROCESS:
                // Make these lists and insert the fusion attributes by iterating these
                var setTopBoxes = dict.Where(d => d.Value.SourceDevice is ISetTopBoxControls);
                uint i = 1;
                foreach (var kvp in setTopBoxes)
                {
                    TryAddRouteActionSigs("Display 1 - Source TV " + i, 188 + i, kvp.Key, kvp.Value.SourceDevice);
                    i++;
                    if (i > 5) // We only have five spots
                    {
                        break;
                    }
                }

                var discPlayers = dict.Where(d => d.Value.SourceDevice is IDiscPlayerControls);
                i = 1;
                foreach (var kvp in discPlayers)
                {
                    TryAddRouteActionSigs("Display 1 - Source DVD " + i, 181 + i, kvp.Key, kvp.Value.SourceDevice);
                    i++;
                    if (i > 5) // We only have five spots
                    {
                        break;
                    }
                }

                var laptops = dict.Where(d => d.Value.SourceDevice is Devices.Laptop);
                i = 1;
                foreach (var kvp in laptops)
                {
                    TryAddRouteActionSigs("Display 1 - Source Laptop " + i, 166 + i, kvp.Key, kvp.Value.SourceDevice);
                    i++;
                    if (i > 10) // We only have ten spots???
                    {
                        break;
                    }
                }

                foreach (var usageDevice in dict.Select(kvp => kvp.Value.SourceDevice).OfType<IUsageTracking>())
                {
                    usageDevice.UsageTracker = new UsageTracking(usageDevice as Device) {UsageIsTracked = true};
                    usageDevice.UsageTracker.DeviceUsageEnded +=
                        UsageTracker_DeviceUsageEnded;
                }
            }
            else
            {
                Debug.Console(1, this, "WARNING: Config source list '{0}' not found for room '{1}'",
                    Room.SourceListKey, Room.Key);
            }
        }

        /// <summary>
        /// Collects usage data from source and sends to Fusion
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        protected void UsageTracker_DeviceUsageEnded(object sender, DeviceUsageEventArgs e)
        {
            var deviceTracker = sender as UsageTracking;

            //var configDevice = ConfigReader.ConfigObject.Devices.Where(d => d.Key.Equals(deviceTracker.Parent)); never used...

            if (deviceTracker == null)
            {
                return;
            }

            var group = ConfigReader.GetGroupForDeviceKey(deviceTracker.Parent.Key);

            var currentMeetingId = "-";

            if (_currentMeeting != null)
            {
                currentMeetingId = _currentMeeting.MeetingID;
            }

            //String Format:  "USAGE||[Date YYYY-MM-DD]||[Time HH-mm-ss]||TIME||[Asset_Type]||[Asset_Name]||[Minutes_used]||[Asset_ID]||[Meeting_ID]"
            // [Asset_ID] property does not appear to be used in Crestron SSI examples.  They are sending "-" instead so that's what is replicated here
            var deviceUsage = string.Format("USAGE||{0}||{1}||TIME||{2}||{3}||-||{4}||-||{5}||{6}||\r\n",
                e.UsageEndTime.ToString("yyyy-MM-dd"), e.UsageEndTime.ToString("HH:mm:ss"),
                @group, deviceTracker.Parent.Name, e.MinutesUsed, "-", currentMeetingId);

            Debug.Console(1, this, "Device usage for: {0} ended at {1}. In use for {2} minutes",
                deviceTracker.Parent.Name, e.UsageEndTime, e.MinutesUsed);

            FusionRoom.DeviceUsage.InputSig.StringValue = deviceUsage;

            Debug.Console(1, this, "Device usage string: {0}", deviceUsage);
        }


        protected void TryAddRouteActionSigs(string attrName, uint attrNum, string routeKey, Device pSrc)
        {
            Debug.Console(2, this, "Creating attribute '{0}' with join {1} for source {2}",
                attrName, attrNum, pSrc.Key);
            try
            {
                var sigD = FusionRoom.CreateOffsetBoolSig(attrNum, attrName, eSigIoMask.InputOutputSig);
                // Need feedback when this source is selected
                // Event handler, added below, will compare source changes with this sig dict
                _sourceToFeedbackSigs.Add(pSrc, sigD.InputSig);

                // And respond to selection in Fusion
                sigD.OutputSig.SetSigFalseAction(
                    () =>
                    {
                        var runRouteAction = Room as IRunRouteAction;
                        if (runRouteAction != null)
                              {
                                  runRouteAction.RunRouteAction(routeKey, Room.SourceListKey);
                              }
                    });
            }
            catch (Exception)
            {
                Debug.Console(2, this, "Error creating Fusion signal {0} {1} for device '{2}'. THIS NEEDS REWORKING",
                    attrNum, attrName, pSrc.Key);
            }
        }

        /// <summary>
        /// 
        /// </summary>
        private void SetUpCommunitcationMonitors()
        {
            uint displayNum = 0;
            uint touchpanelNum = 0;
            uint xpanelNum = 0;

            // Attach to all room's devices with monitors.
            //foreach (var dev in DeviceManager.Devices)
            foreach (var dev in DeviceManager.GetDevices())
            {
                if (!(dev is ICommunicationMonitor))
                {
                    continue;
                }

                string attrName = null;
                uint attrNum = 1;

                //var keyNum = ExtractNumberFromKey(dev.Key);
                //if (keyNum == -1)
                //{
                //    Debug.Console(1, this, "WARNING: Cannot link device '{0}' to numbered Fusion monitoring attributes",
                //        dev.Key);
                //    continue;
                //}
                //uint attrNum = Convert.ToUInt32(keyNum);

                // Check for UI devices
                var uiDev = dev as IHasBasicTriListWithSmartObject;
                if (uiDev != null)
                {
                    if (uiDev.Panel is Crestron.SimplSharpPro.UI.XpanelForSmartGraphics)
                    {
                        attrNum = attrNum + touchpanelNum;

                        if (attrNum > 10)
                        {
                            continue;
                        }
                        attrName = "Online - XPanel " + attrNum;
                        attrNum += 160;

                        touchpanelNum++;
                    }
                    else
                    {
                        attrNum = attrNum + xpanelNum;

                        if (attrNum > 10)
                        {
                            continue;
                        }
                        attrName = "Online - Touch Panel " + attrNum;
                        attrNum += 150;

                        xpanelNum++;
                    }
                }

                //else 
                if (dev is DisplayBase)
                {
                    attrNum = attrNum + displayNum;
                    if (attrNum > 10)
                    {
                        continue;
                    }
                    attrName = "Online - Display " + attrNum;
                    attrNum += 170;

                    displayNum++;
                }
                //else if (dev is DvdDeviceBase)
                //{
                //    if (attrNum > 5)
                //        continue;
                //    attrName = "Device Ok - DVD " + attrNum;
                //    attrNum += 260;
                //}
                // add set top box

                // add Cresnet roll-up

                // add DM-devices roll-up

                if (attrName != null)
                {
                    // Link comm status to sig and update
                    var sigD = FusionRoom.CreateOffsetBoolSig(attrNum, attrName, eSigIoMask.InputSigOnly);
                    var smd = dev as ICommunicationMonitor;
                    sigD.InputSig.BoolValue = smd.CommunicationMonitor.Status == MonitorStatus.IsOk;
                    smd.CommunicationMonitor.StatusChange +=
                        (o, a) => { sigD.InputSig.BoolValue = a.Status == MonitorStatus.IsOk; };
                    Debug.Console(0, this, "Linking '{0}' communication monitor to Fusion '{1}'", dev.Key, attrName);
                }
            }
        }

        protected virtual void SetUpDisplay()
        {
            try
            {
                //Setup Display Usage Monitoring

                var displays = DeviceManager.AllDevices.Where(d => d is DisplayBase);

                //  Consider updating this in multiple display systems

                foreach (var display in displays.Cast<DisplayBase>())
                {
                    display.UsageTracker = new UsageTracking(display) {UsageIsTracked = true};
                    display.UsageTracker.DeviceUsageEnded +=
                        UsageTracker_DeviceUsageEnded;
                }

                var hasDefaultDisplay = Room as IHasDefaultDisplay;
                if (hasDefaultDisplay == null)
                {
                    return;
                }

                var defaultDisplay = hasDefaultDisplay.DefaultDisplay as DisplayBase;
                if (defaultDisplay == null)
                {
                    Debug.Console(1, this, "Cannot link null display to Fusion because default display is null");
                    return;
                }

                var dispPowerOnAction = new Action<bool>(b =>
                {
                    if (!b)
                    {
                        defaultDisplay.PowerOn();
                    }
                });
                var dispPowerOffAction = new Action<bool>(b =>
                {
                    if (!b)
                    {
                        defaultDisplay.PowerOff();
                    }
                });

                // Display to fusion room sigs
                FusionRoom.DisplayPowerOn.OutputSig.UserObject = dispPowerOnAction;
                FusionRoom.DisplayPowerOff.OutputSig.UserObject = dispPowerOffAction;
                defaultDisplay.PowerIsOnFeedback.LinkInputSig(FusionRoom.DisplayPowerOn.InputSig);
                if (defaultDisplay is IDisplayUsage)
                {
                    (defaultDisplay as IDisplayUsage).LampHours.LinkInputSig(FusionRoom.DisplayUsage.InputSig);
                }


                MapDisplayToRoomJoins(1, 158, defaultDisplay);


                
            }
            catch (Exception e)
            {
                Debug.Console(1, this, "Error setting up display in Fusion: {0}", e);
            }
        }

        /// <summary>
        /// Maps room attributes to a display at a specified index
        /// </summary>
        /// <param name="joinOffset"></param>
        /// <param name="display"></param>
        /// <param name="displayIndex"></param>
        /// a
        protected virtual void MapDisplayToRoomJoins(int displayIndex, int joinOffset, DisplayBase display)
        {
            var displayName = string.Format("Display {0} - ", displayIndex);


            var hasDefaultDisplay = Room as IHasDefaultDisplay;
            if (hasDefaultDisplay == null || display != hasDefaultDisplay.DefaultDisplay)
            {
                return;
            }
            // Display volume
            var defaultDisplayVolume = FusionRoom.CreateOffsetUshortSig(50, "Volume - Fader01",
                eSigIoMask.InputOutputSig);
            defaultDisplayVolume.OutputSig.UserObject =
                new Action<ushort>(b =>
                {
                    var basicVolumeWithFeedback = display as IBasicVolumeWithFeedback;
                    if (basicVolumeWithFeedback != null)
                                            {
                                                basicVolumeWithFeedback.SetVolume(b);
                                            }
                });
            var volumeWithFeedback = display as IBasicVolumeWithFeedback;
            if (volumeWithFeedback != null)
            {
                volumeWithFeedback.VolumeLevelFeedback.LinkInputSig(defaultDisplayVolume.InputSig);
            }

            // Power on
            var defaultDisplayPowerOn = FusionRoom.CreateOffsetBoolSig((uint) joinOffset, displayName + "Power On",
                eSigIoMask.InputOutputSig);
            defaultDisplayPowerOn.OutputSig.UserObject = new Action<bool>(b =>
            {
                if (!b)
                {
                    display.PowerOn();
                }
            });
            display.PowerIsOnFeedback.LinkInputSig(defaultDisplayPowerOn.InputSig);

            // Power Off
            //var defaultDisplayPowerOff = FusionRoom.CreateOffsetBoolSig((uint) joinOffset + 1,displayName + "Power Off", eSigIoMask.InputOutputSig); //not used
            defaultDisplayPowerOn.OutputSig.UserObject = new Action<bool>(b =>
            {
                if (!b)
                {
                    display.PowerOff();
                }
            });
            
            display.PowerIsOnFeedback.LinkInputSig(defaultDisplayPowerOn.InputSig);

            // Current Source
            var defaultDisplaySourceNone = FusionRoom.CreateOffsetBoolSig((uint) joinOffset + 8,
                displayName + "Source None", eSigIoMask.InputOutputSig);
            defaultDisplaySourceNone.OutputSig.UserObject = new Action<bool>(b =>
            {
                if (b)
                {
                    return;
                }
                var runRouteAction = Room as IRunRouteAction;
                if (runRouteAction != null)
                {
                    runRouteAction.RunRouteAction("roomOff", Room.SourceListKey);
                }
            });
            
        }

        private void SetUpError()
        {
            // Roll up ALL device errors
            _errorMessageRollUp = new StatusMonitorCollection(this);
            foreach (var md in DeviceManager.GetDevices().OfType<ICommunicationMonitor>())
            {
                _errorMessageRollUp.AddMonitor(md.CommunicationMonitor);
                Debug.Console(2, this, "Adding '{0}' to room's overall error monitor",
                    md.CommunicationMonitor.Parent.Key);
            }
            _errorMessageRollUp.Start();
            FusionRoom.ErrorMessage.InputSig.StringValue = _errorMessageRollUp.Message;
            _errorMessageRollUp.StatusChange +=
                (o, a) => { FusionRoom.ErrorMessage.InputSig.StringValue = _errorMessageRollUp.Message; };
        }

        /// <summary>
        /// Sets up a local occupancy sensor, such as one attached to a Fusion Scheduling panel.  The occupancy status of the room will be read from Fusion
        /// </summary>
        private void SetUpLocalOccupancy()
        {
            RoomIsOccupiedFeedback = new BoolFeedback(RoomIsOccupiedFeedbackFunc);

            FusionRoom.FusionAssetStateChange += FusionRoom_FusionAssetStateChange;

            // Build Occupancy Asset?
            // Link sigs?

            //Room.SetRoomOccupancy(this as IOccupancyStatusProvider, 0);
        }

        private void FusionRoom_FusionAssetStateChange(FusionBase device, FusionAssetStateEventArgs args)
        {
            if (args.EventId == FusionAssetEventId.RoomOccupiedReceivedEventId ||
                args.EventId == FusionAssetEventId.RoomUnoccupiedReceivedEventId)
            {
                RoomIsOccupiedFeedback.FireUpdate();
            }
        }

        /// <summary>
        /// Sets up remote occupancy that will relay the occupancy status determined by local system devices to Fusion
        /// </summary>
        private void SetUpRemoteOccupancy()
        {
            //  Need to have the room occupancy object first and somehow determine the slot number of the Occupancy asset but will not be able to use the UID from config likely.
            //  Consider defining an object just for Room Occupancy (either eAssetType.Occupancy Sensor (local) or eAssetType.RemoteOccupancySensor (from Fusion sched. panel)) and reserving slot 4 for that asset (statics would start at 5)

            //if (Room.OccupancyObj != null)
            //{ 

            var tempOccAsset = _guiDs.OccupancyAsset;

            if (tempOccAsset == null)
            {
                FusionOccSensor = new FusionOccupancySensorAsset(eAssetType.OccupancySensor);
                tempOccAsset = FusionOccSensor;
            }

            var occSensorAsset = FusionRoom.CreateOccupancySensorAsset(tempOccAsset.SlotNumber, tempOccAsset.Name,
                "Occupancy Sensor", tempOccAsset.InstanceId);

            occSensorAsset.RoomOccupied.AddSigToRVIFile = true;

            //var occSensorShutdownMinutes = FusionRoom.CreateOffsetUshortSig(70, "Occ Shutdown - Minutes",eSigIoMask.InputOutputSig); //not used

            // Tie to method on occupancy object
            //occSensorShutdownMinutes.OutputSig.UserObject(new Action(ushort)(b => Room.OccupancyObj.SetShutdownMinutes(b));


            RoomOccupancyRemoteStringFeedback = new StringFeedback(() => _roomOccupancyRemoteString);
            Room.RoomOccupancy.RoomIsOccupiedFeedback.LinkInputSig(occSensorAsset.RoomOccupied.InputSig);
            Room.RoomOccupancy.RoomIsOccupiedFeedback.OutputChange += RoomIsOccupiedFeedback_OutputChange;
            RoomOccupancyRemoteStringFeedback.LinkInputSig(occSensorAsset.RoomOccupancyInfo.InputSig);

            //}
        }

        private void RoomIsOccupiedFeedback_OutputChange(object sender, FeedbackEventArgs e)
        {
            _roomOccupancyRemoteString = String.Format(RemoteOccupancyXml, e.BoolValue ? "Occupied" : "Unoccupied");
            RoomOccupancyRemoteStringFeedback.FireUpdate();
        }

        /// <summary>
        /// Helper to get the number from the end of a device's key string
        /// </summary>
        /// <returns>-1 if no number matched</returns>
        private int ExtractNumberFromKey(string key)
        {
            var capture = System.Text.RegularExpressions.Regex.Match(key, @"\b(\d+)");
            if (!capture.Success)
            {
                return -1;
            }

            return Convert.ToInt32(capture.Groups[1].Value);
        }

        /// <summary>
        /// Event handler for when room source changes
        /// </summary>
        protected void Room_CurrentSourceInfoChange(SourceListItem info, ChangeType type)
        {
            // Handle null. Nothing to do when switching from or to null
            if (info == null || info.SourceDevice == null)
            {
                return;
            }

            var dev = info.SourceDevice;
            if (type == ChangeType.WillChange)
            {
                if (_sourceToFeedbackSigs.ContainsKey(dev))
                {
                    _sourceToFeedbackSigs[dev].BoolValue = false;
                }
            }
            else
            {
                if (_sourceToFeedbackSigs.ContainsKey(dev))
                {
                    _sourceToFeedbackSigs[dev].BoolValue = true;
                }
                //var name = (room == null ? "" : room.Name);
                CurrentRoomSourceNameSig.InputSig.StringValue = info.SourceDevice.Name;
            }
        }

        protected void FusionRoom_FusionStateChange(FusionBase device, FusionStateEventArgs args)
        {
            // The sig/UO method: Need separate handlers for fixed and user sigs, all flavors, 
            // even though they all contain sigs.

            var sigData = (args.UserConfiguredSigDetail as BooleanSigDataFixedName);
            if (sigData != null)
            {
                var outSig = sigData.OutputSig;
                if (outSig.UserObject is Action<bool>)
                {
                    (outSig.UserObject as Action<bool>).Invoke(outSig.BoolValue);
                }
                else if (outSig.UserObject is Action<ushort>)
                {
                    (outSig.UserObject as Action<ushort>).Invoke(outSig.UShortValue);
                }
                else if (outSig.UserObject is Action<string>)
                {
                    (outSig.UserObject as Action<string>).Invoke(outSig.StringValue);
                }
                return;
            }

            var attrData = (args.UserConfiguredSigDetail as BooleanSigData);
            if (attrData == null)
            {
                return;
            }

               var aOutSig = attrData.OutputSig;
            if (aOutSig.UserObject is Action<bool>)
            {
                (aOutSig.UserObject as Action<bool>).Invoke(aOutSig.BoolValue);
            }
            else if (aOutSig.UserObject is Action<ushort>)
            {
                (aOutSig.UserObject as Action<ushort>).Invoke(aOutSig.UShortValue);
            }
            else if (aOutSig.UserObject is Action<string>)
            {
                (aOutSig.UserObject as Action<string>).Invoke(aOutSig.StringValue);
            }
        }
    }


    public static class FusionRoomExtensions
    {
        /// <summary>
        /// Creates and returns a fusion attribute. The join number will match the established Simpl
        /// standard of 50+, and will generate a 50+ join in the RVI. It calls
        /// FusionRoom.AddSig with join number - 49
        /// </summary>
        /// <returns>The new attribute</returns>
        public static BooleanSigData CreateOffsetBoolSig(this FusionRoom fr, uint number, string name, eSigIoMask mask)
        {
            if (number < 50)
            {
                throw new ArgumentOutOfRangeException("number", "Cannot be less than 50");
            }
            number -= 49;
            fr.AddSig(eSigType.Bool, number, name, mask);
            return fr.UserDefinedBooleanSigDetails[number];
        }

        /// <summary>
        /// Creates and returns a fusion attribute. The join number will match the established Simpl
        /// standard of 50+, and will generate a 50+ join in the RVI. It calls
        /// FusionRoom.AddSig with join number - 49
        /// </summary>
        /// <returns>The new attribute</returns>
        public static UShortSigData CreateOffsetUshortSig(this FusionRoom fr, uint number, string name, eSigIoMask mask)
        {
            if (number < 50)
            {
                throw new ArgumentOutOfRangeException("number", "Cannot be less than 50");
            }
            number -= 49;
            fr.AddSig(eSigType.UShort, number, name, mask);
            return fr.UserDefinedUShortSigDetails[number];
        }

        /// <summary>
        /// Creates and returns a fusion attribute. The join number will match the established Simpl
        /// standard of 50+, and will generate a 50+ join in the RVI. It calls
        /// FusionRoom.AddSig with join number - 49
        /// </summary>
        /// <returns>The new attribute</returns>
        public static StringSigData CreateOffsetStringSig(this FusionRoom fr, uint number, string name, eSigIoMask mask)
        {
            if (number < 50)
            {
                throw new ArgumentOutOfRangeException("number", "Cannot be less than 50");
            }
            number -= 49;
            fr.AddSig(eSigType.String, number, name, mask);
            return fr.UserDefinedStringSigDetails[number];
        }

        /// <summary>
        /// Creates and returns a static asset
        /// </summary>
        /// <returns>the new asset</returns>
        public static FusionStaticAsset CreateStaticAsset(this FusionRoom fr, uint number, string name, string type,
            string instanceId)
        {
            Debug.Console(0, "Adding Fusion Static Asset '{0}' to slot {1} with GUID: '{2}'", name, number, instanceId);

            fr.AddAsset(eAssetType.StaticAsset, number, name, type, instanceId);
            return fr.UserConfigurableAssetDetails[number].Asset as FusionStaticAsset;
        }

        public static FusionOccupancySensor CreateOccupancySensorAsset(this FusionRoom fr, uint number, string name,
            string type, string instanceId)
        {
            Debug.Console(0, "Adding Fusion Occupancy Sensor Asset '{0}' to slot {1} with GUID: '{2}'", name, number,
                instanceId);

            fr.AddAsset(eAssetType.OccupancySensor, number, name, type, instanceId);
            return fr.UserConfigurableAssetDetails[number].Asset as FusionOccupancySensor;
        }
    }

    //************************************************************************************************
    /// <summary>
    /// Extensions to enhance Fusion room, asset and signal creation.
    /// </summary>
    public static class FusionStaticAssetExtensions
    {
        /// <summary>
        /// Tries to set a Fusion asset with the make and model of a device.
        /// If the provided Device is IMakeModel, will set the corresponding parameters on the fusion static asset.
        /// Otherwise, does nothing.
        /// </summary>
        public static void TrySetMakeModel(this FusionStaticAsset asset, Device device)
        {
            var mm = device as IMakeModel;
            if (mm != null)
            {
                asset.ParamMake.Value = mm.DeviceMake;
                asset.ParamModel.Value = mm.DeviceModel;
            }
        }

        /// <summary>
        /// Tries to attach the AssetError input on a Fusion asset to a Device's
        /// CommunicationMonitor.StatusChange event. Does nothing if the device is not 
        /// IStatusMonitor
        /// </summary>
        /// <param name="asset"></param>
        /// <param name="device"></param>
        public static void TryLinkAssetErrorToCommunication(this FusionStaticAsset asset, Device device)
        {
            if (device is ICommunicationMonitor)
            {
                var monitor = (device as ICommunicationMonitor).CommunicationMonitor;
                monitor.StatusChange += (o, a) =>
                {
                    // Link connected and error inputs on asset
                    asset.Connected.InputSig.BoolValue = a.Status == MonitorStatus.IsOk;
                    asset.AssetError.InputSig.StringValue = a.Status.ToString();
                };
                // set current value
                asset.Connected.InputSig.BoolValue = monitor.Status == MonitorStatus.IsOk;
                asset.AssetError.InputSig.StringValue = monitor.Status.ToString();
            }
        }
    }

    public class RoomInformation
    {
        public RoomInformation()
        {
            FusionCustomProperties = new List<FusionCustomProperty>();
        }

        public string Id { get; set; }
        public string Name { get; set; }
        public string Location { get; set; }
        public string Description { get; set; }
        public string TimeZone { get; set; }
        public string WebcamUrl { get; set; }
        public string BacklogMsg { get; set; }
        public string SubErrorMsg { get; set; }
        public string EmailInfo { get; set; }
        public List<FusionCustomProperty> FusionCustomProperties { get; set; }
    }

    public class FusionCustomProperty
    {
        public FusionCustomProperty()
        {
        }

        public FusionCustomProperty(string id)
        {
            Id = id;
        }

        public string Id { get; set; }
        public string CustomFieldName { get; set; }
        public string CustomFieldType { get; set; }
        public string CustomFieldValue { get; set; }
    }
}