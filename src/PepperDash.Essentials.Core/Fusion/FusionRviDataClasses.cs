using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Crestron.SimplSharp;
using Crestron.SimplSharpPro.Fusion;

using PepperDash.Core;
using Serilog.Events;

namespace PepperDash.Essentials.Core.Fusion;

// Helper Classes for GUIDs

/// <summary>
/// Stores GUIDs to be written to a file in NVRAM 
/// </summary>
public class FusionRoomGuids
{
    public string RoomName { get; set; }
    public uint IpId { get; set; }
    public string RoomGuid { get; set; }
    public FusionOccupancySensorAsset OccupancyAsset { get; set; }
    public Dictionary<int, FusionAsset> StaticAssets { get; set; }

    public FusionRoomGuids()
    {
        StaticAssets = new Dictionary<int, FusionAsset>();
        OccupancyAsset = new FusionOccupancySensorAsset();
    }

    public FusionRoomGuids(string roomName, uint ipId, string roomGuid, Dictionary<int, FusionAsset> staticAssets)
    {
        RoomName = roomName;
        IpId = ipId;
        RoomGuid = roomGuid;

        StaticAssets = staticAssets;
        OccupancyAsset = new FusionOccupancySensorAsset();
    }

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

public class FusionOccupancySensorAsset
{
    // SlotNumber fixed at 4

    public uint SlotNumber { get { return 4; } }
    public string Name { get { return "Occupancy Sensor"; } }
    public eAssetType Type { get; set; }
    public string InstanceId { get; set; }

    public FusionOccupancySensorAsset()
    {
    }

    public FusionOccupancySensorAsset(eAssetType type)
    {
        Type = type;

        InstanceId = Guid.NewGuid().ToString();
    }
}

public class FusionAsset
{
    public uint SlotNumber { get; set; }
    public string Name { get; set; }
    public string Type { get;  set; }
    public string InstanceId { get;set; }

    public FusionAsset()
    {

    }

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
/// Data needed to request the local time from the Fusion server
/// </summary>
public class LocalTimeRequest
{
    public string RequestID { get; set; }
}

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