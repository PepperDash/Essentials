
using Newtonsoft.Json;
using PepperDash.Essentials.Core;

namespace PepperDash.Essentials.Touchpanel;

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
    /// Gets or sets the DevelopmentServerIp. Optional. If UseDirectServer is true, 
    /// this IP address will be used for direct communication between the mobile control and a development server running on the same local network.
    /// This allows development using a Vite server for hot module replacement (HMR) and live reloading during development. 
    /// Specifying this property will override the default behavior of mobile control.
    /// </summary>
    [JsonProperty("developmentServerAddress", NullValueHandling = NullValueHandling.Ignore)]
    public string DevelopmentServerAddress { get; set; }


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

