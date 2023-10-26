using System.Collections.Generic;

namespace PepperDash.Essentials.Core.Fusion
{
    public class ActionResponse
    {
        //[XmlElement(ElementName = "RequestID")]
        public string RequestID { get; set; }
        //[XmlElement(ElementName = "ActionID")]
        public string ActionID { get; set; }
        //[XmlElement(ElementName = "Parameters")]
        public List<Parameter> Parameters { get; set; }
    }
}