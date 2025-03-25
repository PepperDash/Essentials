using Newtonsoft.Json;
using PepperDash.Essentials.Core;

namespace PepperDash.Essentials.Devices.Common.TouchPanel
{
    public class MobileControlTouchpanelProperties : CrestronTouchpanelPropertiesConfig
    {
        [JsonProperty("useDirectServer")]
        public bool UseDirectServer { get; set; } = false;

        [JsonProperty("zoomRoomController")]
        public bool ZoomRoomController { get; set; } = false;

        [JsonProperty("buttonToolbarTimeoutInS")]
        public ushort ButtonToolbarTimoutInS { get; set; } = 0;

        [JsonProperty("theme")]
        public string Theme { get; set; } = "light";
    }
}