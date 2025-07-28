using Newtonsoft.Json;
using PepperDash.Essentials.Core.Touchpanels;

namespace PepperDash.Essentials.Touchpanel
{
    /// <summary>
    /// Represents a MobileControlTouchpanelProperties
    /// </summary>
    public class MobileControlTouchpanelProperties : CrestronTouchpanelPropertiesConfig
    {
        [JsonProperty("useDirectServer")]
        /// <summary>
        /// Gets or sets the UseDirectServer
        /// </summary>
        public bool UseDirectServer { get; set; } = false;

        [JsonProperty("zoomRoomController")]
        /// <summary>
        /// Gets or sets the ZoomRoomController
        /// </summary>
        public bool ZoomRoomController { get; set; } = false;

        [JsonProperty("buttonToolbarTimeoutInS")]
        /// <summary>
        /// Gets or sets the ButtonToolbarTimoutInS
        /// </summary>
        public ushort ButtonToolbarTimoutInS { get; set; } = 0;

        [JsonProperty("theme")]
        /// <summary>
        /// Gets or sets the Theme
        /// </summary>
        public string Theme { get; set; } = "light";
    }
}