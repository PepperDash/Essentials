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
        /// When true, appends a unique cache-buster to the app URL on every send so the panel is forced to
        /// re-download the app. Only needed for panels whose browser aggressively caches the app (e.g. DGE / CH5);
        /// leave false for normal browser panels (e.g. Cisco Navigator) so they can reuse the cached bundle.
        /// </summary>
        [JsonProperty("forceAppRefresh")]
        public bool ForceAppRefresh { get; set; } = false;


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