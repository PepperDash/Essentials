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

using PepperDash.Essentials.Core;

using PepperDash.Core;
using PepperDash.Essentials;
using PepperDash.Essentials.Devices.Common;


namespace PepperDash.Essentials.Fusion
{
	public class EssentialsHuddleSpaceFusionSystemController : Device
	{
		FusionRoom FusionRoom;
		EssentialsHuddleSpaceRoom Room;
		Dictionary<Device, BoolInputSig> SourceToFeedbackSigs = 
			new Dictionary<Device, BoolInputSig>();

		StatusMonitorCollection ErrorMessageRollUp;

		StringSigData SourceNameSig;

        string GUID;

        Event NextMeeting;

        public EssentialsHuddleSpaceFusionSystemController(EssentialsHuddleSpaceRoom room, uint ipId)
			: base(room.Key + "-fusion")
		{
			Room = room;

            GUID = "awesomeGuid-" + Room.Key;

			CreateSymbolAndBasicSigs(ipId);
			SetUpSources();
			SetUpCommunitcationMonitors();
			SetUpDisplay();
			SetUpError();

			// test assets --- THESE ARE BOTH WIRED TO AssetUsage somewhere internally.
			var ta1 = FusionRoom.CreateStaticAsset(1, "Test asset 1", "Awesome Asset", "Awesome123");
			ta1.AssetError.InputSig.StringValue = "This should be error";


			var ta2 = FusionRoom.CreateStaticAsset(2, "Test asset 2", "Awesome Asset", "Awesome1232");
			ta2.AssetUsage.InputSig.StringValue = "This should be usage";

			// Make it so!
			FusionRVI.GenerateFileForAllFusionDevices();
		}

		void CreateSymbolAndBasicSigs(uint ipId)
		{
            FusionRoom = new FusionRoom(ipId, Global.ControlSystem, Room.Name, GUID);
            FusionRoom.ExtenderRoomViewSchedulingDataReservedSigs.Use();

			FusionRoom.Register();

			FusionRoom.FusionStateChange += new FusionStateEventHandler(FusionRoom_FusionStateChange);

            FusionRoom.ExtenderRoomViewSchedulingDataReservedSigs.DeviceExtenderSigChange += new DeviceExtenderJoinChangeEventHandler(FusionRoomSchedule_DeviceExtenderSigChange);

            CrestronConsole.AddNewConsoleCommand(RequestFullRoomSchedule, "FusReqRoomSchedule", "Requests schedule of the room for the next 24 hours", ConsoleAccessLevelEnum.AccessOperator);

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

        /// <summary>
        /// Generates a room schedule request for this room for the next 24 hours.
        /// </summary>
        /// <param name="requestID">string identifying this request. Used with a corresponding ScheduleResponse value</param>
        public void RequestFullRoomSchedule(string requestID)
        {
            // Need to see if we can omit the XML declaration 

            //XmlWriterSettings settings = new XmlWriterSettings();
            //settings.OmitXmlDeclaration = true;

            //StringBuilder builder = new StringBuilder();

            //XmlWriter xmlWriter = new XmlWriter(builder, settings);

            //RequestSchedule request = new RequestSchedule(requestID, GUID);

            //CrestronXMLSerialization.SerializeObject(xmlWriter, request);

            DateTime now = DateTime.UtcNow;

            Debug.Console(1, this, "Current time: {0}", now.ToString());

            string requestTest =
                string.Format("<RequestSchedule><RequestID>{0}</RequestID><RoomID>{1}</RoomID><Start>2017-05-02T00:00:00</Start><HourSpan>24</HourSpan></RequestSchedule>", requestID, GUID);

            Debug.Console(1, this, "Sending Fusion ScheduleQuery: \n{0}", requestTest);

            FusionRoom.ExtenderRoomViewSchedulingDataReservedSigs.ScheduleQuery.StringValue = requestTest;
        }

        /// <summary>
        /// Ends or Extends a meeting by the specified number of minutes.
        /// </summary>
        /// <param name="extendMinutes">Number of minutes to extend the meeting.  A value of 0 will end the meeting.</param>
        public void ModifyMeetingEndTime(string requestID, Event meeting, int extendMinutes)
        {
            //StringWriter stringWriter = new StringWriter();

            //List<Parameter> parameters = new List<Parameter>();

            //parameters.Add( new Parameter { ID = "MeetingID", Value = meeting.MeetingID });

            //parameters.Add( new Parameter { ID = "EndTime", Value = extendMinutes.ToString()});

            //RequestAction request = new RequestAction(GUID, "MeetingChange", parameters);

            //CrestronXMLSerialization.SerializeObject(stringWriter, request);

            string requestTest = string.Format(
                "<RequestAction><RequestID>{0}</RequestID><RoomID>{1}</RoomID><ActionID>MeetingChange</ActionID><Parameters><Parameter ID = \"MeetingID\" Value = \"\" /><Parameter ID = \"EndTime\" Value = \"{2}\"/></Parameters></RequestAction>"
                , requestID, meeting.MeetingID, extendMinutes);

            Debug.Console(1, this, "Sending MeetingChange Request: \n{0}", requestTest);

            FusionRoom.ExtenderRoomViewSchedulingDataReservedSigs.ScheduleQuery.StringValue = requestTest;
        }

        void FusionRoomSchedule_DeviceExtenderSigChange(DeviceExtender currentDeviceExtender, SigEventArgs args)
        {
            Debug.Console(1, this, "Event: {0}\n Sig: {1}\nFusionResponse:\n{2}", args.Event, args.Sig.Name, args.Sig.StringValue);

            try
            {
                XmlReader reader = new XmlReader(args.Sig.StringValue);

                ScheduleResponse scheduleResponse = new ScheduleResponse();

                scheduleResponse = CrestronXMLSerialization.DeSerializeObject<ScheduleResponse>(reader);

                Debug.Console(1, this, "ScheduleResponse DeSerialization Successfull for Room: '{0}'", scheduleResponse.RoomName);

                if (scheduleResponse.Event.Count > 0)
                {
                    Debug.Console(1, this, "Meetings Count: {0}\n", scheduleResponse.Event.Count);

                    foreach (Event e in scheduleResponse.Event)
                    {
                        Debug.Console(1, this, "Subject: {0}", e.Subject);
                        Debug.Console(1, this, "MeetingID: {0}", e.MeetingID);
                        Debug.Console(1, this, "Start Time: {0}", e.DtStart);
                        Debug.Console(1, this, "End Time: {0}\n", e.DtEnd);
                    }
                }
                
            }
            catch (Exception e)
            {
                Debug.Console(1, this, "Error parsing ScheduleResponse: {0}", e);
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
			var dispAsset = FusionRoom.CreateStaticAsset(3, display.Name, "Display", "awesomeDisplayId" + Room.Key);
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
        //[XmlElement(ElementName = "Event")]
        public List<Event> Event { get; set; }
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
        public string DtStart { get; set; }
        //[XmlElement(ElementName = "dtEnd")]
        public string DtEnd { get; set; }
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