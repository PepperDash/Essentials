using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Crestron.SimplSharp;
using Crestron.SimplSharpPro.Fusion;

using PepperDash.Core;
using Serilog.Events;

namespace PepperDash.Essentials.Core.Fusion
{
    // Helper Classes for GUIDs

    /// <summary>
    /// Stores GUIDs to be written to a file in NVRAM 
    /// </summary>
    public class FusionRoomGuids
    {
        /// <summary>
        /// Gets or sets the RoomName
        /// </summary>
        public string RoomName { get; set; }

        /// <summary>
        /// Gets or sets the IpId
        /// </summary>
        public uint IpId { get; set; }

        /// <summary>
        /// Gets or sets the RoomGuid
        /// </summary>
        public string RoomGuid { get; set; }

        /// <summary>
        /// Gets or sets the OccupancyAsset
        /// </summary>
        public FusionOccupancySensorAsset OccupancyAsset { get; set; }

        /// <summary>
        /// Gets or sets the StaticAssets
        /// </summary>
        public Dictionary<int, FusionAsset> StaticAssets { get; set; }

        /// <summary>
        /// FusionRoomGuids constructor
        /// </summary>
        public FusionRoomGuids()
        {
            StaticAssets = new Dictionary<int, FusionAsset>();
            OccupancyAsset = new FusionOccupancySensorAsset();
        }

        /// <summary>
        /// FusionRoomGuids constructor
        /// </summary>
        /// <param name="roomName">name of the fusion room</param>
        /// <param name="ipId">ipID of the fusion room</param>
        /// <param name="roomGuid">room GUID</param>
        /// <param name="staticAssets">dictionary of assets</param>
        public FusionRoomGuids(string roomName, uint ipId, string roomGuid, Dictionary<int, FusionAsset> staticAssets)
        {
            RoomName = roomName;
            IpId = ipId;
            RoomGuid = roomGuid;

            StaticAssets = staticAssets;
            OccupancyAsset = new FusionOccupancySensorAsset();
        }

        /// <summary>
        /// FusionRoomGuids constructor
        /// </summary>
        /// <param name="roomName">name of the fusion room</param>
        /// <param name="ipId">ipID of the fusion room</param>
        /// <param name="roomGuid">room GUID</param>
        /// <param name="staticAssets">dictionary of assets</param>
        /// <param name="occAsset">occupancy asset</param>
        public FusionRoomGuids(string roomName, uint ipId, string roomGuid, Dictionary<int, FusionAsset> staticAssets, FusionOccupancySensorAsset occAsset)
        {
            RoomName = roomName;
            IpId = ipId;
            RoomGuid = roomGuid;

            StaticAssets = staticAssets;
            OccupancyAsset = occAsset;
        }

        /// <summary>
        /// Generates a new room GUID prefixed by the program slot number and NIC MAC address
        /// </summary>
        /// <param name="progSlot"></param>
        /// <param name="mac"></param>
        /// <summary>
        /// GenerateNewRoomGuid method
        /// </summary>
        public string GenerateNewRoomGuid(uint progSlot, string mac)
        {
            Guid roomGuid = Guid.NewGuid();

            return string.Format("{0}-{1}-{2}", progSlot, mac, roomGuid.ToString());
        }


        /// <summary>
        /// Adds an asset to the StaticAssets collection and returns the new asset
        /// </summary>
        /// <param name="room"></param>
        /// <param name="uid"></param>
        /// <param name="assetName"></param>
        /// <param name="type"></param>
        /// <param name="instanceId"></param>
        /// <returns></returns>
        public FusionAsset AddStaticAsset(FusionRoom room, int uid, string assetName, string type, string instanceId)
        {
            var slotNum = GetNextAvailableAssetNumber(room);

            Debug.LogMessage(LogEventLevel.Verbose, "Adding Fusion Asset: {0} of Type: {1} at Slot Number: {2} with GUID: {3}", assetName, type, slotNum, instanceId);

            var tempAsset = new FusionAsset(slotNum, assetName, type, instanceId);

            StaticAssets.Add(uid, tempAsset);

            return tempAsset;
        }

        /// <summary>
        /// Returns the next available slot number in the Fusion UserConfigurableAssetDetails collection
        /// </summary>
        /// <param name="room"></param>
        /// <returns></returns>
        /// <summary>
        /// GetNextAvailableAssetNumber method
        /// </summary>
        public static uint GetNextAvailableAssetNumber(FusionRoom room)
        {
            uint slotNum = 0;

            foreach (var item in room.UserConfigurableAssetDetails)
            {
                if(item.Number > slotNum)
                    slotNum = item.Number;
            }

            if (slotNum < 5)
            {
                slotNum = 5;
            }
            else
                slotNum = slotNum + 1;

            Debug.LogMessage(LogEventLevel.Verbose, "#Next available fusion asset number is: {0}", slotNum);

            return slotNum;
        }

    }

    /// <summary>
    /// Represents a FusionOccupancySensorAsset
    /// </summary>
    public class FusionOccupancySensorAsset
    {
        // SlotNumber fixed at 4

        /// <summary>
        /// Gets or sets the SlotNumber
        /// </summary>
        public uint SlotNumber { get { return 4; } }
        /// <summary>
        /// Gets or sets the Name
        /// </summary>
        public string Name { get { return "Occupancy Sensor"; } }
        /// <summary>
        /// Gets or sets the Type
        /// </summary>
        public eAssetType Type { get; set; }
        /// <summary>
        /// Gets or sets the InstanceId
        /// </summary>
        public string InstanceId { get; set; }

        /// <summary>
        /// Default constructor
        /// </summary>
        public FusionOccupancySensorAsset()
        {
        }

        /// <summary>
        /// FusionOccupancySensorAsset constructor
        /// </summary>
        /// <param name="type">asset type</param>
        public FusionOccupancySensorAsset(eAssetType type)
        {
            Type = type;

            InstanceId = Guid.NewGuid().ToString();
        }
    }

    /// <summary>
    /// Represents a FusionAsset
    /// </summary>
    public class FusionAsset
    {
        /// <summary>
        /// Gets or sets the SlotNumber
        /// </summary>
        public uint SlotNumber { get; set; }
        /// <summary>
        /// Gets or sets the Name
        /// </summary>
        public string Name { get; set; }
        /// <summary>
        /// Gets or sets the Type
        /// </summary>
        public string Type { get;  set; }
        /// <summary>
        /// Gets or sets the InstanceId
        /// </summary>
        public string InstanceId { get;set; }

        /// <summary>
        /// Default constructor
        /// </summary>
        public FusionAsset()
        {

        }

        /// <summary>
        /// FusionAsset constructor
        /// </summary>
        /// <param name="slotNum">slot number of asset</param>
        /// <param name="assetName">name of the asset</param>
        /// <param name="type">type of the asset</param>
        /// <param name="instanceId">instance ID of the asset</param>
        public FusionAsset(uint slotNum, string assetName, string type, string instanceId)
        {
            SlotNumber = slotNum;
            Name = assetName;
            Type = type;
            if (string.IsNullOrEmpty(instanceId))
            {
                InstanceId = Guid.NewGuid().ToString();
            }
            else
            {
                InstanceId = instanceId;
            }
        }
    }

    //***************************************************************************************************

    /// <summary>
    /// Represents a RoomSchedule
    /// </summary>
    public class RoomSchedule
    {
        /// <summary>
        /// Gets or sets the Meetings
        /// </summary>
        public List<Event> Meetings { get; set; }

        /// <summary>
        /// RoomSchedule constructor
        /// </summary>
        public RoomSchedule()
        {
            Meetings = new List<Event>();
        }
    }

    //****************************************************************************************************
    // Helper Classes for XML API

    /// <summary>
    /// Represents a LocalTimeRequest
    /// </summary>
    public class LocalTimeRequest
    {
        /// <summary>
        /// Gets or sets the RequestID
        /// </summary>
        public string RequestID { get; set; }
    }

    /// <summary>
    /// All the data needed for a full schedule request in a room
    /// </summary>
    /// //[XmlRoot(ElementName = "RequestSchedule")]
    public class RequestSchedule
    {
        //[XmlElement(ElementName = "RequestID")]
        /// <summary>
        /// Gets or sets the RequestID
        /// </summary>
        public string RequestID { get; set; }

        //[XmlElement(ElementName = "RoomID")]
        /// <summary>
        /// Gets or sets the RoomID
        /// </summary>
        public string RoomID { get; set; }

        //[XmlElement(ElementName = "Start")]
        /// <summary>
        /// Gets or sets the Start
        /// </summary>
        public DateTime Start { get; set; }

        //[XmlElement(ElementName = "HourSpan")]
        /// <summary>
        /// Gets or sets the HourSpan
        /// </summary>
        public double HourSpan { get; set; }

        /// <summary>
        /// RequestSchedule constructor
        /// </summary>
        /// <param name="requestID">id of the request</param>
        /// <param name="roomID">id of the room</param>
        public RequestSchedule(string requestID, string roomID)
        {
            RequestID = requestID;
            RoomID = roomID;
            Start = DateTime.Now;
            HourSpan = 24;
        }
    }


    //[XmlRoot(ElementName = "RequestAction")]
    /// <summary>
    /// Represents a RequestAction
    /// </summary>
    public class RequestAction
    {
        //[XmlElement(ElementName = "RequestID")]
        /// <summary>
        /// Gets or sets the RequestID
        /// </summary>
        public string RequestID { get; set; }
        //[XmlElement(ElementName = "RoomID")]
        /// <summary>
        /// Gets or sets the RoomID
        /// </summary>
        public string RoomID { get; set; }
        //[XmlElement(ElementName = "ActionID")]
        /// <summary>
        /// Gets or sets the ActionID
        /// </summary>
        public string ActionID { get; set; }
        //[XmlElement(ElementName = "Parameters")]
        /// <summary>
        /// Gets or sets the Parameters
        /// </summary>
        public List<Parameter> Parameters { get; set; }

        /// <summary>
        /// RequestAction constructor
        /// </summary>
        /// <param name="roomID">id of the room</param>
        /// <param name="actionID">id of the action</param>
        /// <param name="parameters">list of parameters</param>
        public RequestAction(string roomID, string actionID, List<Parameter> parameters)
        {
            RoomID = roomID;
            ActionID = actionID;
            Parameters = parameters;
        }
    }

    //[XmlRoot(ElementName = "ActionResponse")]
    /// <summary>
    /// Represents a ActionResponse
    /// </summary>
    public class ActionResponse
    {
        //[XmlElement(ElementName = "RequestID")]
        /// <summary>
        /// Gets or sets the RequestID
        /// </summary>
        public string RequestID { get; set; }

        //[XmlElement(ElementName = "ActionID")]
        /// <summary>
        /// Gets or sets the ActionID
        /// </summary>
        public string ActionID { get; set; }

        //[XmlElement(ElementName = "Parameters")]
        /// <summary>
        /// Gets or sets the Parameters
        /// </summary>
        public List<Parameter> Parameters { get; set; }
    }

    //[XmlRoot(ElementName = "Parameter")]
    /// <summary>
    /// Represents a Parameter
    /// </summary>
    public class Parameter
    {
        //[XmlAttribute(AttributeName = "ID")]
        /// <summary>
        /// Gets or sets the ID
        /// </summary>
        public string ID { get; set; }

        //[XmlAttribute(AttributeName = "Value")]
        /// <summary>
        /// Gets or sets the Value
        /// </summary>
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
        /// <summary>
        /// Gets or sets the RequestID
        /// </summary>
        public string RequestID { get; set; }

        //[XmlElement(ElementName = "RoomID")]
        /// <summary>
        /// Gets or sets the RoomID
        /// </summary>
        public string RoomID { get; set; }

        //[XmlElement(ElementName = "RoomName")]
        /// <summary>
        /// Gets or sets the RoomName
        /// </summary>
        public string RoomName { get; set; }

        //[XmlElement("Event")]
        /// <summary>
        /// Gets or sets the Events
        /// </summary>
        public List<Event> Events { get; set; }

        /// <summary>
        /// ScheduleResponse constructor
        /// </summary>
        public ScheduleResponse()
        {
            Events = new List<Event>();
        }
    }

    //[XmlRoot(ElementName = "Event")]
    /// <summary>
    /// Represents a Event
    /// </summary>
    public class Event
    {
        //[XmlElement(ElementName = "MeetingID")]
        /// <summary>
        /// Gets or sets the MeetingID
        /// </summary>
        public string MeetingID { get; set; }
        //[XmlElement(ElementName = "RVMeetingID")]
        /// <summary>
        /// Gets or sets the RVMeetingID
        /// </summary>
        public string RVMeetingID { get; set; }
        //[XmlElement(ElementName = "Recurring")]
        /// <summary>
        /// Gets or sets the Recurring
        /// </summary>
        public string Recurring { get; set; }
        //[XmlElement(ElementName = "InstanceID")]
        /// <summary>
        /// Gets or sets the InstanceID
        /// </summary>
        public string InstanceID { get; set; }
        //[XmlElement(ElementName = "dtStart")]
        /// <summary>
        /// Gets or sets the dtStart
        /// </summary>
        public DateTime dtStart { get; set; }
        //[XmlElement(ElementName = "dtEnd")]
        /// <summary>
        /// Gets or sets the dtEnd
        /// </summary>
        public DateTime dtEnd { get; set; }
        //[XmlElement(ElementName = "Organizer")]
        /// <summary>
        /// Gets or sets the Organizer
        /// </summary>
        public string Organizer { get; set; }
        //[XmlElement(ElementName = "Attendees")]
        /// <summary>
        /// Gets or sets the Attendees
        /// </summary>
        public Attendees Attendees { get; set; }
        //[XmlElement(ElementName = "Resources")]
        /// <summary>
        /// Gets or sets the Resources
        /// </summary>
        public Resources Resources { get; set; }
        //[XmlElement(ElementName = "IsEvent")]
        /// <summary>
        /// Gets or sets the IsEvent
        /// </summary>
        public string IsEvent { get; set; }
        //[XmlElement(ElementName = "IsRoomViewMeeting")]
        /// <summary>
        /// Gets or sets the IsRoomViewMeeting
        /// </summary>
        public string IsRoomViewMeeting { get; set; }
        //[XmlElement(ElementName = "IsPrivate")]
        /// <summary>
        /// Gets or sets the IsPrivate
        /// </summary>
        public string IsPrivate { get; set; }
        //[XmlElement(ElementName = "IsExchangePrivate")]
        /// <summary>
        /// Gets or sets the IsExchangePrivate
        /// </summary>
        public string IsExchangePrivate { get; set; }
        //[XmlElement(ElementName = "MeetingTypes")]
        /// <summary>
        /// Gets or sets the MeetingTypes
        /// </summary>
        public MeetingTypes MeetingTypes { get; set; }
        //[XmlElement(ElementName = "ParticipantCode")]
        /// <summary>
        /// Gets or sets the ParticipantCode
        /// </summary>
        public string ParticipantCode { get; set; }
        //[XmlElement(ElementName = "PhoneNo")]
        /// <summary>
        /// Gets or sets the PhoneNo
        /// </summary>
        public string PhoneNo { get; set; }
        //[XmlElement(ElementName = "WelcomeMsg")]
        /// <summary>
        /// Gets or sets the WelcomeMsg
        /// </summary>
        public string WelcomeMsg { get; set; }
        //[XmlElement(ElementName = "Subject")]
        /// <summary>
        /// Gets or sets the Subject
        /// </summary>
        public string Subject { get; set; }
        //[XmlElement(ElementName = "LiveMeeting")]
        /// <summary>
        /// Gets or sets the LiveMeeting
        /// </summary>
        public LiveMeeting LiveMeeting { get; set; }
        //[XmlElement(ElementName = "ShareDocPath")]
        /// <summary>
        /// Gets or sets the ShareDocPath
        /// </summary>
        public string ShareDocPath { get; set; }
        //[XmlElement(ElementName = "HaveAttendees")]
        /// <summary>
        /// Gets or sets the HaveAttendees
        /// </summary>
        public string HaveAttendees { get; set; }
        //[XmlElement(ElementName = "HaveResources")]
        /// <summary>
        /// Gets or sets the HaveResources
        /// </summary>
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
                if (hours > 0)
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
    /// <summary>
    /// Represents a Resources
    /// </summary>
    public class Resources
    {
        //[XmlElement(ElementName = "Rooms")]
        /// <summary>
        /// Gets or sets the Rooms
        /// </summary>
        public Rooms Rooms { get; set; }
    }

    //[XmlRoot(ElementName = "Rooms")]
    /// <summary>
    /// Represents a Rooms
    /// </summary>
    public class Rooms
    {
        //[XmlElement(ElementName = "Room")]
        /// <summary>
        /// Gets or sets the Room
        /// </summary>
        public List<Room> Room { get; set; }
    }

    //[XmlRoot(ElementName = "Room")]
    /// <summary>
    /// Represents a Room
    /// </summary>
    public class Room
    {
        //[XmlElement(ElementName = "Name")]
        /// <summary>
        /// Gets or sets the Name
        /// </summary>
        public string Name { get; set; }
        //[XmlElement(ElementName = "ID")]
        /// <summary>
        /// Gets or sets the ID
        /// </summary>
        public string ID { get; set; }
        //[XmlElement(ElementName = "MPType")]
        /// <summary>
        /// Gets or sets the MPType
        /// </summary>
        public string MPType { get; set; }
    }

    //[XmlRoot(ElementName = "Attendees")]
    /// <summary>
    /// Represents a Attendees
    /// </summary>
    public class Attendees
    {
        //[XmlElement(ElementName = "Required")]
        /// <summary>
        /// Gets or sets the Required
        /// </summary>
        public Required Required { get; set; }
        //[XmlElement(ElementName = "Optional")]
        /// <summary>
        /// Gets or sets the Optional
        /// </summary>
        public Optional Optional { get; set; }
    }

    //[XmlRoot(ElementName = "Required")]
    /// <summary>
    /// Represents a Required
    /// </summary>
    public class Required
    {
        //[XmlElement(ElementName = "Attendee")]
        /// <summary>
        /// Gets or sets the Attendee
        /// </summary>
        public List<string> Attendee { get; set; }
    }

    //[XmlRoot(ElementName = "Optional")]
    /// <summary>
    /// Represents a Optional
    /// </summary>
    public class Optional
    {
        //[XmlElement(ElementName = "Attendee")]
        /// <summary>
        /// Gets or sets the Attendee
        /// </summary>
        public List<string> Attendee { get; set; }
    }

    //[XmlRoot(ElementName = "MeetingType")]
    /// <summary>
    /// Represents a MeetingType
    /// </summary>
    public class MeetingType
    {
        //[XmlAttribute(AttributeName = "ID")]
        /// <summary>
        /// Gets or sets the ID
        /// </summary>
        public string ID { get; set; }
        //[XmlAttribute(AttributeName = "Value")]
        /// <summary>
        /// Gets or sets the Value
        /// </summary>
        public string Value { get; set; }
    }

    //[XmlRoot(ElementName = "MeetingTypes")]
    /// <summary>
    /// Represents a MeetingTypes
    /// </summary>
    public class MeetingTypes
    {
        //[XmlElement(ElementName = "MeetingType")]
        /// <summary>
        /// Gets or sets the MeetingType
        /// </summary>
        public List<MeetingType> MeetingType { get; set; }
    }

    //[XmlRoot(ElementName = "LiveMeeting")]
    /// <summary>
    /// Represents a LiveMeeting
    /// </summary>
    public class LiveMeeting
    {
        //[XmlElement(ElementName = "URL")]
        /// <summary>
        /// Gets or sets the URL
        /// </summary>
        public string URL { get; set; }
        //[XmlElement(ElementName = "ID")]
        /// <summary>
        /// Gets or sets the ID
        /// </summary>
        public string ID { get; set; }
        //[XmlElement(ElementName = "Key")]
        /// <summary>
        /// Gets or sets the Key
        /// </summary>
        public string Key { get; set; }
        //[XmlElement(ElementName = "Subject")]
        /// <summary>
        /// Gets or sets the Subject
        /// </summary>
        public string Subject { get; set; }
    }

    //[XmlRoot(ElementName = "LiveMeetingURL")]
    /// <summary>
    /// Represents a LiveMeetingURL
    /// </summary>
    public class LiveMeetingURL
    {
        //[XmlElement(ElementName = "LiveMeeting")]
        /// <summary>
        /// Gets or sets the LiveMeeting
        /// </summary>
        public LiveMeeting LiveMeeting { get; set; }
    }
}