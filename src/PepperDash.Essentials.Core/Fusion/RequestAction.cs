using System.Collections.Generic;

namespace PepperDash.Essentials.Core.Fusion
{
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
            RoomID     = roomID;
            ActionID   = actionID;
            Parameters = parameters;
        }
    }
}