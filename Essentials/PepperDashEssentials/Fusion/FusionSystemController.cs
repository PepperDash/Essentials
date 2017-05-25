using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Crestron.SimplSharp;
using Crestron.SimplSharp.CrestronIO;
using Crestron.SimplSharp.CrestronXml;
using Crestron.SimplSharp.CrestronXml.Serialization;
using Crestron.SimplSharp.CrestronXmlLinq;
using Crestron.SimplSharpPro;
using Crestron.SimplSharpPro.DeviceSupport;
using Crestron.SimplSharpPro.Fusion;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

using PepperDash.Essentials.Core;

using PepperDash.Core;
using PepperDash.Essentials;
using PepperDash.Essentials.Devices.Common;



namespace PepperDash.Essentials.Fusion
{
	public class EssentialsHuddleSpaceFusionSystemController : Device
	{
        public event EventHandler<ScheduleChangeEventArgs> ScheduleChange;
        public event EventHandler<MeetingChangeEventArgs> MeetingEndWarning;
        public event EventHandler<MeetingChangeEventArgs> NextMeetingBeginWarning;

		FusionRoom FusionRoom;
		EssentialsHuddleSpaceRoom Room;
		Dictionary<Device, BoolInputSig> SourceToFeedbackSigs = 
			new Dictionary<Device, BoolInputSig>();

		StatusMonitorCollection ErrorMessageRollUp;

		StringSigData SourceNameSig;

        RoomSchedule CurrentSchedule;

        Event NextMeeting;

        Event CurrentMeeting;

        string RoomGuid
        {
            get
            {
                return GUIDs.RoomGuid;
            }
  
        }

        uint IpId;

        FusionRoomGuids GUIDs;

        bool GuidFileExists;

        bool IsRegisteredForSchedulePushNotifications = false;

        CTimer PollTimer = null;

        CTimer PushNotificationTimer = null;

        // Default poll time is 5 min unless overridden by config value
        public long SchedulePollInterval = 300000;

        public long PushNotificationTimeout = 5000;

        List<StaticAsset> StaticAssets;

        //ScheduleResponseEvent NextMeeting;

        public EssentialsHuddleSpaceFusionSystemController(EssentialsHuddleSpaceRoom room, uint ipId)
			: base(room.Key + "-fusion")
		{

			Room = room;

            IpId = ipId;

            StaticAssets = new List<StaticAsset>();

            GUIDs = new FusionRoomGuids();

            var mac = CrestronEthernetHelper.GetEthernetParameter(CrestronEthernetHelper.ETHERNET_PARAMETER_TO_GET.GET_MAC_ADDRESS, 0);

            var slot = Global.ControlSystem.ProgramNumber;

            string guidFilePath = string.Format(@"\NVRAM\Program{0}\{1}-FusionGuids.json", Global.ControlSystem.ProgramNumber, InitialParametersClass.ProgramIDTag);

            GuidFileExists = File.Exists(guidFilePath);

            if (GuidFileExists)
            {
                ReadGuidFile(guidFilePath);
            }
            else
            {
                IpId = ipId;

                Guid roomGuid = Guid.NewGuid();

                GUIDs.RoomGuid = string.Format("{0}-{1}-{2}", slot, mac, roomGuid.ToString());
            }

			CreateSymbolAndBasicSigs(IpId);
			SetUpSources();
			SetUpCommunitcationMonitors();
			SetUpDisplay();
			SetUpError();

			// test assets --- THESE ARE BOTH WIRED TO AssetUsage somewhere internally.
            var tempAsset1 = new StaticAsset();
            var tempAsset2 = new StaticAsset();


            //Check for existing GUID
            if (GuidFileExists)
            {
                tempAsset1 = StaticAssets.FirstOrDefault(a => a.Number.Equals(1));

                tempAsset2 = StaticAssets.FirstOrDefault(a => a.Number.Equals(2));
            }
            else
            {
                tempAsset1 = new StaticAsset(1, "Test Asset 1", "Test Asset 1", "");
                StaticAssets.Add(tempAsset1);

                tempAsset2 = new StaticAsset(2, "Test Asset 2", "Test Asset 2", "");
                StaticAssets.Add(tempAsset2);
            }

            var ta1 = FusionRoom.CreateStaticAsset(tempAsset1.Number, tempAsset1.Name, tempAsset1.Type, tempAsset1.InstanceID);
            ta1.AssetError.InputSig.StringValue = "This should be error";

            var ta2 = FusionRoom.CreateStaticAsset(tempAsset2.Number, tempAsset2.Name, tempAsset2.Type, tempAsset2.InstanceID);
            ta2.AssetUsage.InputSig.StringValue = "This should be usage";
            
            // Make it so!   
            FusionRVI.GenerateFileForAllFusionDevices();

            GenerateGuidFile(guidFilePath);      
		}

        /// <summary>
        /// Generates the guid file in NVRAM
        /// </summary>
        /// <param name="filePath">path for the file</param>
        void GenerateGuidFile(string filePath)
        {
            if (string.IsNullOrEmpty(filePath))
            {
                Debug.Console(0, this, "Error writing guid file.  No path specified.");
                return;
            }

            CCriticalSection _fileLock = new CCriticalSection();

            try
            {
                if (_fileLock == null || _fileLock.Disposed)
                    return;

                _fileLock.Enter();

                Debug.Console(1, this, "Writing GUIDs to file");

                GUIDs = new FusionRoomGuids(Room.Name, IpId, RoomGuid, StaticAssets);

                var JSON = JsonConvert.SerializeObject(GUIDs, Newtonsoft.Json.Formatting.Indented);

                using (StreamWriter sw = new StreamWriter(filePath))
                {
                    sw.Write(JSON);
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
                if (_fileLock != null && !_fileLock.Disposed)
                    _fileLock.Leave();
            }
        }

        /// <summary>
        /// Reads the guid file from NVRAM
        /// </summary>
        /// <param name="filePath">path for te file</param>
        void ReadGuidFile(string filePath)
        {
            if(string.IsNullOrEmpty(filePath))
            {
                Debug.Console(0, this, "Error reading guid file.  No path specified.");
                return;
            }

            CCriticalSection _fileLock = new CCriticalSection();

            try
            {
                if(_fileLock == null || _fileLock.Disposed)
                    return;

                _fileLock.Enter();

                if(File.Exists(filePath))
                {
                    var JSON = File.ReadToEnd(filePath, Encoding.ASCII);

                    GUIDs = JsonConvert.DeserializeObject<FusionRoomGuids>(JSON);

                    IpId = GUIDs.IpId;

                    StaticAssets = GUIDs.StaticAssets;

                }

                Debug.Console(0, this, "Fusion Guids successfully read from file:");

                Debug.Console(1, this, "\nRoom Name: {0}\nIPID: {1:x}\n RoomGuid: {2}", Room.Name, IpId, RoomGuid);

                foreach (StaticAsset asset in StaticAssets)
                {
                    Debug.Console(1, this, "\nAsset Name: {0}\nAsset No: {1}\n Guid: {2}", asset.Name, asset.Number, asset.InstanceID);
                }
            }
            catch (Exception e)
            {
                Debug.Console(0, this, "Error reading guid file: {0}", e);
            }
            finally
            {
                if(_fileLock != null && !_fileLock.Disposed)
                   _fileLock.Leave();
            }

        }

		void CreateSymbolAndBasicSigs(uint ipId)
		{
            Debug.Console(1, this, "Creating Fusion Room symbol with GUID: {0}", RoomGuid);

            FusionRoom = new FusionRoom(ipId, Global.ControlSystem, Room.Name, RoomGuid);
            FusionRoom.ExtenderRoomViewSchedulingDataReservedSigs.Use();
            FusionRoom.ExtenderFusionRoomDataReservedSigs.Use();

			FusionRoom.Register();

            FusionRoom.FusionStateChange += new FusionStateEventHandler(FusionRoom_FusionStateChange);

            FusionRoom.ExtenderRoomViewSchedulingDataReservedSigs.DeviceExtenderSigChange += new DeviceExtenderJoinChangeEventHandler(FusionRoomSchedule_DeviceExtenderSigChange);
            FusionRoom.ExtenderFusionRoomDataReservedSigs.DeviceExtenderSigChange += new DeviceExtenderJoinChangeEventHandler(ExtenderFusionRoomDataReservedSigs_DeviceExtenderSigChange);
            FusionRoom.OnlineStatusChange += new OnlineStatusChangeEventHandler(FusionRoom_OnlineStatusChange);

            CrestronConsole.AddNewConsoleCommand(RequestFullRoomSchedule, "FusReqRoomSchedule", "Requests schedule of the room for the next 24 hours", ConsoleAccessLevelEnum.AccessOperator);
            CrestronConsole.AddNewConsoleCommand(ModifyMeetingEndTimeConsoleHelper, "FusReqRoomSchMod", "Ends or extends a meeting by the specified time", ConsoleAccessLevelEnum.AccessOperator);
            CrestronConsole.AddNewConsoleCommand(CreateAsHocMeeting, "FusCreateMeeting", "Creates and Ad Hoc meeting for on hour or until the next meeting", ConsoleAccessLevelEnum.AccessOperator);

			// Room to fusion room
			Room.OnFeedback.LinkInputSig(FusionRoom.SystemPowerOn.InputSig);
			SourceNameSig = FusionRoom.CreateOffsetStringSig(50, "Source - Name", eSigIoMask.InputSigOnly);
			// Don't think we need to get current status of this as nothing should be alive yet. 
			Room.CurrentSingleSourceChange += new SourceInfoChangeHandler(Room_CurrentSourceInfoChange);


			FusionRoom.SystemPowerOn.OutputSig.SetSigFalseAction(Room.PowerOnToDefaultOrLastSource);
			FusionRoom.SystemPowerOff.OutputSig.SetSigFalseAction(() => Room.RunRouteAction("roomOff"));
			// NO!! room.RoomIsOn.LinkComplementInputSig(FusionRoom.SystemPowerOff.InputSig);
			FusionRoom.ErrorMessage.InputSig.StringValue =
				"3: 7 Errors: This is a really long error message;This is a really long error message;This is a really long error message;This is a really long error message;This is a really long error message;This is a really long error message;This is a really long error message;";


		}

        void FusionRoom_OnlineStatusChange(GenericBase currentDevice, OnlineOfflineEventArgs args)
        {
            if (args.DeviceOnLine)
            {
                CrestronEnvironment.Sleep(200);

                // Send Push Notification Action request:

                string requestID = "InitialPushRequest";


                string actionRequest =
                    string.Format("<RequestAction>\n<RequestID>{0}</RequestID>\n", requestID) +
                    "<ActionID>RegisterPushModel</ActionID>\n" +
                    "<Parameters>\n" +
                        "<Parameter ID='Enabled' Value='1' />\n" +
                        "<Parameter ID='RequestID' Value='PushNotification' />\n" +
                        "<Parameter ID='Start' Value='00:00:00' />\n" +
                        "<Parameter ID='HourSpan' Value='24' />\n" +
                        "<Parameter ID='Field' Value='MeetingID' />\n" +
                        "<Parameter ID='Field' Value='RVMeetingID' />\n" +
                        "<Parameter ID='Field' Value='InstanceID' />\n" +
                        //"<Parameter ID='Field' Value='Recurring' />\n" +
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

                Debug.Console(1, this, "Sending Fusion ActionRequest: \n{0}", actionRequest);

                FusionRoom.ExtenderFusionRoomDataReservedSigs.ActionQuery.StringValue = actionRequest;
            }

        }

        /// <summary>
        /// Generates a room schedule request for this room for the next 24 hours.
        /// </summary>
        /// <param name="requestID">string identifying this request. Used with a corresponding ScheduleResponse value</param>
        public void RequestFullRoomSchedule(object callbackObject)
        {
            DateTime now = DateTime.Today;

            //string currentTime = string.Format("Current time: {0:D4}-{1:D2}-{2:D2}T{3:D2}:{4:D2}:{5:D2}", now.Year, now.Month, now.Day, now.Hour, now.Minute, now.Second);

            string currentTime = now.ToString("s");

            Debug.Console(1, this, "Current time: {0}", currentTime);

            //Debug.Console(1, this, "Current time: {0}", now.ToString("d"));

            //string requestTest =
            //    string.Format("<RequestSchedule><RequestID>{0}</RequestID><RoomID>{1}</RoomID><Start>2017-05-02T00:00:00</Start><HourSpan>24</HourSpan></RequestSchedule>", requestID, GUID);

            string requestTest =
                string.Format("<RequestSchedule><RequestID>FullSchedleRequest</RequestID><RoomID>{0}</RoomID><Start>{1}</Start><HourSpan>24</HourSpan></RequestSchedule>", RoomGuid, currentTime);

            Debug.Console(1, this, "Sending Fusion ScheduleQuery: \n{0}", requestTest);

            FusionRoom.ExtenderRoomViewSchedulingDataReservedSigs.ScheduleQuery.StringValue = requestTest;

            if (IsRegisteredForSchedulePushNotifications)
                PushNotificationTimer.Stop();
        }
        
        /// <summary>
        /// Wrapper method to allow console commands to modify the current meeting end time
        /// </summary>
        /// <param name="command">meetingID extendTime</param>
        public void ModifyMeetingEndTimeConsoleHelper(string command)
        {
            string requestID;
            string meetingID = null;
            int extendMinutes = -1;

            requestID = "ModifyMeetingTest12345";

            try
            {
                var tokens = command.Split(' ');

                meetingID = tokens[0];
                extendMinutes = Int32.Parse(tokens[1]);

            }
            catch (Exception e)
            {
                Debug.Console(1, this, "Error parsing console command: {0}", e);
            }

            ModifyMeetingEndTime(requestID, extendMinutes);

        }

        /// <summary>
        /// Ends or Extends the current meeting by the specified number of minutes.
        /// </summary>
        /// <param name="extendMinutes">Number of minutes to extend the meeting.  A value of 0 will end the meeting.</param>
        public void ModifyMeetingEndTime(string requestID, int extendMinutes)
        {
            if(CurrentMeeting == null)
            {
                Debug.Console(1, this, "No meeting in progress.  Unable to modify end time.");
                return;
            }            
#warning Need to add logic to properly extend from the current time.  See S+ module for reference.
            if (extendMinutes > -1)
            {
                if(extendMinutes > 0)
                {
                    var extendTime = CurrentMeeting.dtEnd - DateTime.Now;
                    double extendMinutesRaw = extendTime.TotalMinutes;

                    extendMinutes = extendMinutes + (int)Math.Round(extendMinutesRaw);
                }


                string requestTest = string.Format(
                    "<RequestAction><RequestID>{0}</RequestID><RoomID>{1}</RoomID><ActionID>MeetingChange</ActionID><Parameters><Parameter ID = 'MeetingID' Value = '{2}' /><Parameter ID = 'EndTime' Value = '{3}' /></Parameters></RequestAction>"
                    , requestID, RoomGuid, CurrentMeeting.MeetingID, extendMinutes);

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
        public void CreateAsHocMeeting(string command)
        {
            string requestID = "CreateAdHocMeeting";

            DateTime now = DateTime.Now.AddMinutes(1);

            now.AddSeconds(-now.Second);

            // Assume 1 hour meeting if possible
            DateTime dtEnd = now.AddHours(1);

            // Check if room is available for 1 hour before next meeting
            if (NextMeeting != null)
            {
                var roomAvailable = NextMeeting.dtEnd.Subtract(dtEnd);

                if (roomAvailable.TotalMinutes < 60)
                {
                    /// Room not available for full hour, book until next meeting starts
                    dtEnd = NextMeeting.dtEnd;
                }
            }

            string createMeetingRequest = 
                "<CreateSchedule>" +
                    string.Format("<RequestID>{0}</RequestID>", requestID) +
                    string.Format("<RoomID>{0}</RoomID>", RoomGuid) +
                    "<Event>" +
                        string.Format("<dtStart>{0}</dtStart>", now.ToString("s")) +
                        string.Format("<dtEnd>{0}</dtEnd>", dtEnd.ToString("s")) +
                        "<Subject>AdHoc Meeting</Subject>" +
                        "<Organizer>Room User</Organizer>" +
                        "<WelcomMsg>Example Message</WelcomMsg>" +
                    "</Event>" +
                "</CreateSchedule>";

            Debug.Console(1, this, "Sending CreateMeeting Request: \n{0}", createMeetingRequest);

            FusionRoom.ExtenderRoomViewSchedulingDataReservedSigs.CreateMeeting.StringValue = createMeetingRequest;

            //Debug.Console(1, this, "Sending CreateMeeting Request: \n{0}", command);

            //FusionRoom.ExtenderRoomViewSchedulingDataReservedSigs.CreateMeeting.StringValue = command;

        }

        /// <summary>
        /// Event handler method for Device Extender sig changes
        /// </summary>
        /// <param name="currentDeviceExtender"></param>
        /// <param name="args"></param>
        void ExtenderFusionRoomDataReservedSigs_DeviceExtenderSigChange(DeviceExtender currentDeviceExtender, SigEventArgs args)
        {
            Debug.Console(1, this, "Event: {0}\n Sig: {1}\nFusionResponse:\n{2}", args.Event, args.Sig.Name, args.Sig.StringValue);


            if (args.Sig == FusionRoom.ExtenderFusionRoomDataReservedSigs.ActionQueryResponse)
            {
                try
                {
                    //ActionResponse actionResponse = new ActionResponse();

                    //TextReader reader = new StringReader(args.Sig.StringValue);

                    //actionResponse = CrestronXMLSerialization.DeSerializeObject<ActionResponse>(reader);

                    //if (actionResponse != null)
                    //{
                    //    if (actionResponse.RequestID == "InitialPushRequest")
                    //    {
                    //        if (actionResponse.Parameters != null)
                    //        {
                    //            var tempParam = actionResponse.Parameters.FirstOrDefault(p => p.ID.Equals("Registered"));

                    XmlDocument message = new XmlDocument();

                    message.LoadXml(args.Sig.StringValue);

                    var actionResponse = message["ActionResponse"];

                    if (actionResponse != null)
                    {
                        var requestID = actionResponse["RequestID"];

                        if (requestID.InnerText == "InitialPushRequest")
                        {
                            if (actionResponse["ActionID"].InnerText == "RegisterPushModel")
                            {
                                var parameters = actionResponse["Parameters"];

                                foreach (XmlElement parameter in parameters)
                                {
                                    if (parameter.HasAttributes)
                                    {
                                        var attributes = parameter.Attributes;

                                        if (attributes["ID"].Value == "Registered")
                                        {
                                            var isRegistered = Int32.Parse(attributes["Value"].Value);

                                            if (isRegistered == 1)
                                            {
                                                IsRegisteredForSchedulePushNotifications = true;

                                                if (PollTimer != null && !PollTimer.Disposed)
                                                {
                                                    PollTimer.Stop();
                                                    PollTimer.Dispose();
                                                }

                                                PushNotificationTimer = new CTimer(RequestFullRoomSchedule, null, PushNotificationTimeout, PushNotificationTimeout);

                                                PushNotificationTimer.Reset(PushNotificationTimeout, PushNotificationTimeout);
                                            }
                                            else if (isRegistered == 0)
                                            {
                                                IsRegisteredForSchedulePushNotifications = false;

                                                if (PushNotificationTimer != null && !PushNotificationTimer.Disposed)
                                                {
                                                    PushNotificationTimer.Stop();
                                                    PushNotificationTimer.Dispose();
                                                }

                                                PollTimer = new CTimer(RequestFullRoomSchedule, null, SchedulePollInterval, SchedulePollInterval);

                                                PollTimer.Reset(SchedulePollInterval, SchedulePollInterval);
                                            }
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
                catch (Exception e)
                {
                    Debug.Console(1, this, "Error parsing ActionQueryResponse: {0}", e);
                }
            }
        }

        /// <summary>
        /// Event handler method for Device Extender sig changes
        /// </summary>
        /// <param name="currentDeviceExtender"></param>
        /// <param name="args"></param>
        void FusionRoomSchedule_DeviceExtenderSigChange(DeviceExtender currentDeviceExtender, SigEventArgs args)
        {
           Debug.Console(1, this, "Scehdule Response Event: {0}\n Sig: {1}\nFusionResponse:\n{2}", args.Event, args.Sig.Name, args.Sig.StringValue);


           if (args.Sig == FusionRoom.ExtenderRoomViewSchedulingDataReservedSigs.ScheduleResponse)
           {
               try
               {
                   ScheduleResponse scheduleResponse = new ScheduleResponse();

                   XmlDocument message = new XmlDocument();

                   message.LoadXml(args.Sig.StringValue);

                   var response = message["ScheduleResponse"];

                   if (response != null)
                   {
                       // Check for push notification
                       if (response["RequestID"].InnerText == "RVRequest")
                       {
                           var action = response["Action"];

                           if (action.OuterXml.IndexOf("RequestSchedule") > -1)
                           {
                               PushNotificationTimer.Reset(PushNotificationTimeout, PushNotificationTimeout);
                           }
                       }
                       else    // Not a push notification
                       {
                           CurrentSchedule = new RoomSchedule();  // Clear Current Schedule
                           CurrentMeeting = null;   // Clear Current Meeting
                           NextMeeting = null;      // Clear Next Meeting

                           bool isNextMeeting = false;

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

                                   XmlReader reader = new XmlReader(element.OuterXml);

                                   Event tempEvent = new Event();

                                   tempEvent = CrestronXMLSerialization.DeSerializeObject<Event>(reader);

                                   scheduleResponse.Events.Add(tempEvent);

                                   // Check is this is the current event
                                   if (tempEvent.dtStart <= DateTime.Now && tempEvent.dtEnd >= DateTime.Now)
                                   {
                                       CurrentMeeting = tempEvent;  // Set Current Meeting
                                       isNextMeeting = true;        // Flag that next element is next meeting
                                   }

                                   if (isNextMeeting)
                                   {
                                       NextMeeting = tempEvent; // Set Next Meeting
                                       isNextMeeting = false;
                                   }

                                   CurrentSchedule.Meetings.Add(tempEvent);
                               }

                           }

                           PrintTodaysSchedule();

                           if (!IsRegisteredForSchedulePushNotifications)
                               PollTimer.Reset(SchedulePollInterval, SchedulePollInterval);
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
               Debug.Console(1, this, "Create Meeting Response Event: {0}\n Sig: {1}\nFusionResponse:\n{2}", args.Event, args.Sig.Name, args.Sig.StringValue);
           }

        }

        void PrintTodaysSchedule()
        {
            if (CurrentSchedule.Meetings.Count > 0)
            {
                Debug.Console(1, this, "Today's Schedule for '{0}'\n", Room.Name);

                foreach (Event e in CurrentSchedule.Meetings)
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

		void SetUpSources()
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
					TryAddRouteActionSigs("Source - TV " + i, 115 + i, kvp.Key, kvp.Value.SourceDevice);
					i++;
					if (i > 5) // We only have five spots
						break;
				}

				var discPlayers = dict.Where(d => d.Value.SourceDevice is IDiscPlayerControls);
				i = 1;
				foreach (var kvp in discPlayers)
				{
					TryAddRouteActionSigs("Source - DVD " + i, 120 + i, kvp.Key, kvp.Value.SourceDevice);
					i++;
					if (i > 5) // We only have five spots
						break;
				}

				var laptops = dict.Where(d => d.Value.SourceDevice is Laptop);
				i = 1;
				foreach (var kvp in laptops)
				{
					TryAddRouteActionSigs("Source - Laptop " + i, 100 + i, kvp.Key, kvp.Value.SourceDevice);
					i++;
					if (i > 10) // We only have ten spots???
						break;
				}

				
			}
			else
			{
				Debug.Console(1, this, "WARNING: Config source list '{0}' not found for room '{1}'",
					Room.SourceListKey, Room.Key);
			}
		}

		void TryAddRouteActionSigs(string attrName, uint attrNum, string routeKey, Device pSrc)
		{
			Debug.Console(2, this, "Creating attribute '{0}' with join {1} for source {2}",
				attrName, attrNum, pSrc.Key);
			try
			{
				var sigD = FusionRoom.CreateOffsetBoolSig(attrNum, attrName, eSigIoMask.InputOutputSig);
				// Need feedback when this source is selected
				// Event handler, added below, will compare source changes with this sig dict
				SourceToFeedbackSigs.Add(pSrc, sigD.InputSig);

				// And respond to selection in Fusion
				sigD.OutputSig.SetSigFalseAction(() => Room.RunRouteAction(routeKey));
			}
			catch (Exception)
			{
				Debug.Console(2, this, "Error creating Fusion signal {0} {1} for device '{2}'. THIS NEEDS REWORKING", attrNum, attrName, pSrc.Key);
			}
		}

		/// <summary>
		/// 
		/// </summary>
		void SetUpCommunitcationMonitors()
		{
			// Attach to all room's devices with monitors.
			//foreach (var dev in DeviceManager.Devices)
			foreach (var dev in DeviceManager.GetDevices())
			{
				if (!(dev is ICommunicationMonitor))
					continue;

				var keyNum = ExtractNumberFromKey(dev.Key);
				if (keyNum == -1)
				{
					Debug.Console(1, this, "WARNING: Cannot link device '{0}' to numbered Fusion monitoring attributes",
						dev.Key);
					continue;
				}
				string attrName = null;
				uint attrNum = Convert.ToUInt32(keyNum);

				//if (dev is SmartGraphicsTouchpanelControllerBase)
				//{
				//    if (attrNum > 10)
				//        continue;
				//    attrName = "Device Ok - Touch Panel " + attrNum;
				//    attrNum += 200;
				//}
				//// add xpanel here

				//else 
				if (dev is DisplayBase)
				{
					if (attrNum > 10)
						continue;
					attrName = "Device Ok - Display " + attrNum;
					attrNum += 240;
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
					smd.CommunicationMonitor.StatusChange += (o, a) =>
					{ sigD.InputSig.BoolValue = a.Status == MonitorStatus.IsOk; };
					Debug.Console(0, this, "Linking '{0}' communication monitor to Fusion '{1}'", dev.Key, attrName);
				}
			}
		}

		void SetUpDisplay()
		{
			var display = Room.DefaultDisplay as DisplayBase;
			if (display == null)
			{
				Debug.Console(1, this, "Cannot link null display to Fusion");
				return;
			}

			var dispPowerOnAction = new Action<bool>(b => { if (!b) display.PowerOn(); });
			var dispPowerOffAction = new Action<bool>(b => { if (!b) display.PowerOff(); });

			// Display to fusion room sigs
			FusionRoom.DisplayPowerOn.OutputSig.UserObject = dispPowerOnAction;
			FusionRoom.DisplayPowerOff.OutputSig.UserObject = dispPowerOffAction;
			display.PowerIsOnFeedback.LinkInputSig(FusionRoom.DisplayPowerOn.InputSig);
			if (display is IDisplayUsage)
				(display as IDisplayUsage).LampHours.LinkInputSig(FusionRoom.DisplayUsage.InputSig);

			// static assets --------------- testing
			// Make a display asset
            string dispAssetInstanceId;

            //Check for existing GUID
            var tempAsset = StaticAssets.FirstOrDefault(a => a.Number.Equals(3));
            if(tempAsset != null)
			    dispAssetInstanceId = tempAsset.InstanceID;
            else
                dispAssetInstanceId = "";

            var dispAsset = FusionRoom.CreateStaticAsset(3, display.Name, "Display", dispAssetInstanceId);
			dispAsset.PowerOn.OutputSig.UserObject = dispPowerOnAction;
			dispAsset.PowerOff.OutputSig.UserObject = dispPowerOffAction;
			display.PowerIsOnFeedback.LinkInputSig(dispAsset.PowerOn.InputSig);
			// NO!! display.PowerIsOn.LinkComplementInputSig(dispAsset.PowerOff.InputSig);
			// Use extension methods
			dispAsset.TrySetMakeModel(display);
			dispAsset.TryLinkAssetErrorToCommunication(display);
		}

		void SetUpError()
		{
			// Roll up ALL device errors
			ErrorMessageRollUp = new StatusMonitorCollection(this);
			foreach (var dev in DeviceManager.GetDevices())
			{
				var md = dev as ICommunicationMonitor;
				if (md != null)
				{
					ErrorMessageRollUp.AddMonitor(md.CommunicationMonitor);
					Debug.Console(2, this, "Adding '{0}' to room's overall error monitor", md.CommunicationMonitor.Parent.Key);
				}
			}
			ErrorMessageRollUp.Start();
			FusionRoom.ErrorMessage.InputSig.StringValue = ErrorMessageRollUp.Message;
			ErrorMessageRollUp.StatusChange += (o, a) =>
			{
				FusionRoom.ErrorMessage.InputSig.StringValue = ErrorMessageRollUp.Message;
			};

		}




		/// <summary>
		/// Helper to get the number from the end of a device's key string
		/// </summary>
		/// <returns>-1 if no number matched</returns>
		int ExtractNumberFromKey(string key)
		{
			var capture = System.Text.RegularExpressions.Regex.Match(key, @"\D+(\d+)");
			if (!capture.Success)
				return -1;
			else return Convert.ToInt32(capture.Groups[1].Value);
		}

		/// <summary>
		/// Event handler for when room source changes
		/// </summary>
		void Room_CurrentSourceInfoChange(EssentialsRoomBase room, SourceListItem info, ChangeType type)
		{
			// Handle null. Nothing to do when switching from or to null
			if (info == null || info.SourceDevice == null)
				return;

			var dev = info.SourceDevice;
			if (type == ChangeType.WillChange)
			{
				if (SourceToFeedbackSigs.ContainsKey(dev))
					SourceToFeedbackSigs[dev].BoolValue = false;
			}
			else
			{
				if (SourceToFeedbackSigs.ContainsKey(dev))
					SourceToFeedbackSigs[dev].BoolValue = true;
				var name = (room == null ? "" : room.Name);
				SourceNameSig.InputSig.StringValue = name;
			}
		}

		void FusionRoom_FusionStateChange(FusionBase device, FusionStateEventArgs args)
		{

			// The sig/UO method: Need separate handlers for fixed and user sigs, all flavors, 
			// even though they all contain sigs.

			var sigData = (args.UserConfiguredSigDetail as BooleanSigDataFixedName);
			if (sigData != null)
			{
				var outSig = sigData.OutputSig;
				if (outSig.UserObject is Action<bool>)
					(outSig.UserObject as Action<bool>).Invoke(outSig.BoolValue);
				else if (outSig.UserObject is Action<ushort>)
					(outSig.UserObject as Action<ushort>).Invoke(outSig.UShortValue);
				else if (outSig.UserObject is Action<string>)
					(outSig.UserObject as Action<string>).Invoke(outSig.StringValue);
				return;
			}

			var attrData = (args.UserConfiguredSigDetail as BooleanSigData);
			if (attrData != null)
			{
				var outSig = attrData.OutputSig;
				if (outSig.UserObject is Action<bool>)
					(outSig.UserObject as Action<bool>).Invoke(outSig.BoolValue);
				else if (outSig.UserObject is Action<ushort>)
					(outSig.UserObject as Action<ushort>).Invoke(outSig.UShortValue);
				else if (outSig.UserObject is Action<string>)
					(outSig.UserObject as Action<string>).Invoke(outSig.StringValue);
				return;
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
			if (number < 50) throw new ArgumentOutOfRangeException("number", "Cannot be less than 50");
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
			if (number < 50) throw new ArgumentOutOfRangeException("number", "Cannot be less than 50");
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
			if (number < 50) throw new ArgumentOutOfRangeException("number", "Cannot be less than 50");
			number -= 49;
			fr.AddSig(eSigType.String, number, name, mask);
			return fr.UserDefinedStringSigDetails[number];
		}

		/// <summary>
		/// Creates and returns a static asset
		/// </summary>
		/// <returns>the new asset</returns>
		public static FusionStaticAsset CreateStaticAsset(this FusionRoom fr, uint number, string name, string type, string instanceId)
		{            
            //if(string.IsNullOrEmpty(instanceId))
            //    instanceId = Guid.NewGuid().ToString();

            Debug.Console(0, "Creating Fusion Static Asset '{0}' with GUID: '{1}'", name, instanceId);

			fr.AddAsset(eAssetType.StaticAsset, number, name, type, instanceId);
			return fr.UserConfigurableAssetDetails[number].Asset as FusionStaticAsset;
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

    // Helper Classes for GUIDs

    /// <summary>
    /// Stores GUIDs to be written to a file in NVRAM 
    /// </summary>
    public class FusionRoomGuids
    {
        public string RoomName { get; set; }
        public uint IpId { get; set; }
        public string RoomGuid { get; set; }
        public List<StaticAsset> StaticAssets { get; set; }

        public FusionRoomGuids()
        {
            StaticAssets = new List<StaticAsset>();
        }

        public FusionRoomGuids(string roomName, uint ipId, string roomGuid, List<StaticAsset> staticAssets)
        {
            RoomName = roomName;
            IpId = ipId;
            RoomGuid = roomGuid;

            StaticAssets = new List<StaticAsset>(staticAssets);
        }
    }

    public class StaticAsset
    {
        public uint Number { get; set; }
        public string Name { get; set; }
        public string Type { get; set; }
        public string InstanceID { get; set; }

        public StaticAsset()
        {

        }

        public StaticAsset(uint slotNum, string assetName, string type, string instanceID)
        {
            Number = slotNum;
            Name = assetName;
            Type = type;
            if(string.IsNullOrEmpty(instanceID))
            {
                InstanceID = Guid.NewGuid().ToString();
            }
            else
            {
                InstanceID = instanceID;
            }
        }
    }

    //***************************************************************************************************

    public class RoomSchedule
    {
        public List<Event> Meetings { get; set; }

        public RoomSchedule()
        {
            Meetings = new List<Event>();
        }
    }

    //****************************************************************************************************
    // Helper Classes for XML API

    /// <summary>
    /// All the data needed for a full schedule request in a room
    /// </summary>
    /// //[XmlRoot(ElementName = "RequestSchedule")]
    public class RequestSchedule
    {
        //[XmlElement(ElementName = "RequestID")]
        public string RequestID { get; set; }
        //[XmlElement(ElementName = "RoomID")]
        public string RoomID { get; set; }
        //[XmlElement(ElementName = "Start")]
        public DateTime Start { get; set; }
        //[XmlElement(ElementName = "HourSpan")]
        public double HourSpan { get; set; }

        public RequestSchedule(string requestID, string roomID)
        {
            RequestID = requestID;
            RoomID = roomID;
            Start = DateTime.Now;
            HourSpan = 24;
        }
    }


    //[XmlRoot(ElementName = "RequestAction")]
    public class RequestAction
    {
        //[XmlElement(ElementName = "RequestID")]
        public string RequestID { get; set; }
        //[XmlElement(ElementName = "RoomID")]
        public string RoomID { get; set; }
        //[XmlElement(ElementName = "ActionID")]
        public string ActionID { get; set; }
        //[XmlElement(ElementName = "Parameters")]
        public List<Parameter> Parameters { get; set; }

        public RequestAction(string roomID, string actionID, List<Parameter> parameters)
        {
            RoomID = roomID;
            ActionID = actionID;
            Parameters = parameters;
        }
    }

    //[XmlRoot(ElementName = "ActionResponse")]
    public class ActionResponse
    {
        //[XmlElement(ElementName = "RequestID")]
        public string RequestID { get; set; }
        //[XmlElement(ElementName = "ActionID")]
        public string ActionID { get; set; }
        //[XmlElement(ElementName = "Parameters")]
        public List<Parameter> Parameters { get; set; }
    }

    //[XmlRoot(ElementName = "Parameter")]
    public class Parameter
    {
        //[XmlAttribute(AttributeName = "ID")]
        public string ID { get; set; }
        //[XmlAttribute(AttributeName = "Value")]
        public string Value { get; set; }
    }

    ////[XmlRoot(ElementName = "Parameters")]
    //public class Parameters
    //{
    //    //[XmlElement(ElementName = "Parameter")]
    //    public List<Parameter> Parameter { get; set; }
    //}  
    
    /// <summary>
    /// Data structure for a ScheduleResponse from Fusion
    /// </summary>
    /// //[XmlRoot(ElementName = "ScheduleResponse")]
    public class ScheduleResponse
    {
        //[XmlElement(ElementName = "RequestID")]
        public string RequestID { get; set; }
        //[XmlElement(ElementName = "RoomID")]
        public string RoomID { get; set; }
        //[XmlElement(ElementName = "RoomName")]
        public string RoomName { get; set; }
        //[XmlElement("Event")]
        public List<Event> Events { get; set; }

        public ScheduleResponse()
        {
            Events = new List<Event>();
        }
    }

    //[XmlRoot(ElementName = "Event")]
    public class Event
    {
        //[XmlElement(ElementName = "MeetingID")]
        public string MeetingID { get; set; }
        //[XmlElement(ElementName = "RVMeetingID")]
        public string RVMeetingID { get; set; }
        //[XmlElement(ElementName = "Recurring")]
        public string Recurring { get; set; }
        //[XmlElement(ElementName = "InstanceID")]
        public string InstanceID { get; set; }
        //[XmlElement(ElementName = "dtStart")]
        public DateTime dtStart { get; set; }
        //[XmlElement(ElementName = "dtEnd")]
        public DateTime dtEnd { get; set; }
        //[XmlElement(ElementName = "Organizer")]
        public string Organizer { get; set; }
        //[XmlElement(ElementName = "Attendees")]
        public Attendees Attendees { get; set; }
        //[XmlElement(ElementName = "Resources")]
        public Resources Resources { get; set; }
        //[XmlElement(ElementName = "IsEvent")]
        public string IsEvent { get; set; }
        //[XmlElement(ElementName = "IsRoomViewMeeting")]
        public string IsRoomViewMeeting { get; set; }
        //[XmlElement(ElementName = "IsPrivate")]
        public string IsPrivate { get; set; }
        //[XmlElement(ElementName = "IsExchangePrivate")]
        public string IsExchangePrivate { get; set; }
        //[XmlElement(ElementName = "MeetingTypes")]
        public MeetingTypes MeetingTypes { get; set; }
        //[XmlElement(ElementName = "ParticipantCode")]
        public string ParticipantCode { get; set; }
        //[XmlElement(ElementName = "PhoneNo")]
        public string PhoneNo { get; set; }
        //[XmlElement(ElementName = "WelcomeMsg")]
        public string WelcomeMsg { get; set; }
        //[XmlElement(ElementName = "Subject")]
        public string Subject { get; set; }
        //[XmlElement(ElementName = "LiveMeeting")]
        public LiveMeeting LiveMeeting { get; set; }
        //[XmlElement(ElementName = "ShareDocPath")]
        public string ShareDocPath { get; set; }
        //[XmlElement(ElementName = "HaveAttendees")]
        public string HaveAttendees { get; set; }
        //[XmlElement(ElementName = "HaveResources")]
        public string HaveResources { get; set; }

        /// <summary>
        /// Gets the duration of the meeting
        /// </summary>
        public string DurationInMinutes
        {
            get
            {
                string duration;

                var timeSpan = dtEnd.Subtract(dtStart);
                int hours = timeSpan.Hours;
                double minutes = timeSpan.Minutes;
                double roundedMinutes = Math.Round(minutes);
                if(hours > 0)
                {
                    duration = string.Format("{0} hours {1} minutes", hours, roundedMinutes);
                }
                else
                {
                    duration = string.Format("{0} minutes", roundedMinutes);
                }

                return duration;
            }
        }

        /// <summary>
        /// Gets the remaining time in the meeting.  Returns null if the meeting is not currently in progress.
        /// </summary>
        public string RemainingTime
        {
            get
            {
                var now = DateTime.Now;

                string remainingTime;

                if (GetInProgress())
                {
                    var timeSpan = dtEnd.Subtract(now);
                    int hours = timeSpan.Hours;
                    double minutes = timeSpan.Minutes;
                    double roundedMinutes = Math.Round(minutes);
                    if (hours > 0)
                    {
                        remainingTime = string.Format("{0} hours {1} minutes", hours, roundedMinutes);
                    }
                    else
                    {
                        remainingTime = string.Format("{0} minutes", roundedMinutes);
                    }

                    return remainingTime;
                }
                else
                    return null;
            }

        }

        /// <summary>
        /// Indicates that the meeting is in progress
        /// </summary>
        public bool isInProgress
        {
            get
            {
                return GetInProgress();
            }
        }

        /// <summary>
        /// Determines if the meeting is in progress
        /// </summary>
        /// <returns>Returns true if in progress</returns>
        bool GetInProgress()
        {
            var now = DateTime.Now;

            if (now > dtStart && now < dtEnd)
            {
                return true;
            }
            else
                return false;
        }
    }

    //[XmlRoot(ElementName = "Resources")]
    public class Resources
    {
        //[XmlElement(ElementName = "Rooms")]
        public Rooms Rooms { get; set; }
    }

    //[XmlRoot(ElementName = "Rooms")]
    public class Rooms
    {
        //[XmlElement(ElementName = "Room")]
        public List<Room> Room { get; set; }
    }

    //[XmlRoot(ElementName = "Room")]
    public class Room
    {
        //[XmlElement(ElementName = "Name")]
        public string Name { get; set; }
        //[XmlElement(ElementName = "ID")]
        public string ID { get; set; }
        //[XmlElement(ElementName = "MPType")]
        public string MPType { get; set; }
    }

    //[XmlRoot(ElementName = "Attendees")]
    public class Attendees
    {
        //[XmlElement(ElementName = "Required")]
        public Required Required { get; set; }
        //[XmlElement(ElementName = "Optional")]
        public Optional Optional { get; set; }
    }

    //[XmlRoot(ElementName = "Required")]
    public class Required
    {
        //[XmlElement(ElementName = "Attendee")]
        public List<string> Attendee { get; set; }
    }

    //[XmlRoot(ElementName = "Optional")]
    public class Optional
    {
        //[XmlElement(ElementName = "Attendee")]
        public List<string> Attendee { get; set; }
    }

    //[XmlRoot(ElementName = "MeetingType")]
    public class MeetingType
    {
        //[XmlAttribute(AttributeName = "ID")]
        public string ID { get; set; }
        //[XmlAttribute(AttributeName = "Value")]
        public string Value { get; set; }
    }

    //[XmlRoot(ElementName = "MeetingTypes")]
    public class MeetingTypes
    {
        //[XmlElement(ElementName = "MeetingType")]
        public List<MeetingType> MeetingType { get; set; }
    }

    //[XmlRoot(ElementName = "LiveMeeting")]
    public class LiveMeeting
    {
        //[XmlElement(ElementName = "URL")]
        public string URL { get; set; }
        //[XmlElement(ElementName = "ID")]
        public string ID { get; set; }
        //[XmlElement(ElementName = "Key")]
        public string Key { get; set; }
        //[XmlElement(ElementName = "Subject")]
        public string Subject { get; set; }
    }

    //[XmlRoot(ElementName = "LiveMeetingURL")]
    public class LiveMeetingURL
    {
        //[XmlElement(ElementName = "LiveMeeting")]
        public LiveMeeting LiveMeeting { get; set; }
    }
}