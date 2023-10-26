using System.Collections.Generic;

namespace PepperDash.Essentials.Core.Fusion
{
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
}