using System;

namespace PepperDash.Essentials.Core.Fusion
{
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
            RoomID    = roomID;
            Start     = DateTime.Now;
            HourSpan  = 24;
        }
    }
}