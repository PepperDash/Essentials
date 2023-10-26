using System.Collections.Generic;

namespace PepperDash.Essentials.Core.Fusion
{
    public class RoomInformation
    {
        public RoomInformation()
        {
            FusionCustomProperties = new List<FusionCustomProperty>();
        }

        public string ID { get; set; }
        public string Name { get; set; }
        public string Location { get; set; }
        public string Description { get; set; }
        public string TimeZone { get; set; }
        public string WebcamURL { get; set; }
        public string BacklogMsg { get; set; }
        public string SubErrorMsg { get; set; }
        public string EmailInfo { get; set; }
        public List<FusionCustomProperty> FusionCustomProperties { get; set; }
    }
}