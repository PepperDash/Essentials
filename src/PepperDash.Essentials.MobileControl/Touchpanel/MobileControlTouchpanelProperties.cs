using Newtonsoft.Json;
using PepperDash.Essentials.Core;

namespace PepperDash.Essentials.Touchpanel
{
    /// <summary>
    /// Represents a MobileControlTouchpanelProperties
    /// </summary>
    public class MobileControlTouchpanelProperties : CrestronTouchpanelPropertiesConfig
    {

        /// <summary>
        /// Gets or sets the UseDirectServer
        /// </summary>
        [JsonProperty("useDirectServer")]
        public bool UseDirectServer { get; set; } = false;


        /// <summary>
        /// Gets or sets the ZoomRoomController
        /// </summary>
        [JsonProperty("zoomRoomController")]
        public bool ZoomRoomController { get; set; } = false;


        /// <summary>
        /// Gets or sets the ButtonToolbarTimoutInS
        /// </summary>
        [JsonProperty("buttonToolbarTimeoutInS")]
        public ushort ButtonToolbarTimoutInS { get; set; } = 0;


        /// <summary>
        /// Gets or sets the Theme
        /// </summary>
        [JsonProperty("theme")]
        public string Theme { get; set; } = "light";
    }
}