
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

    /// <summary>
    /// UC-ENGINE-FLEX-TEAMS only. When set, the Teams Rooms "auto screen share" setting is written to this value when the
    /// engine comes online (and again whenever it comes back online). When omitted the setting is left alone.
    /// The engine takes it as a write-only string signal (the SDK documents no value format and the engine reports nothing
    /// back); "true" or "false" is written unless <see cref="AutoScreenShareFormat"/> says otherwise. Not confirmed on hardware.
    /// </summary>
    [JsonProperty("autoScreenShare", NullValueHandling = NullValueHandling.Ignore)]
    public bool? AutoScreenShare { get; set; }

    /// <summary>
    /// UC-ENGINE-FLEX-TEAMS only. How <see cref="AutoScreenShare"/> is written to the engine: "bool" (the default) writes
    /// "true" or "false", "numeric" writes "1" or "0". The Teams Rooms XML config takes both forms, and which one the engine
    /// accepts is not confirmed, so this is the fallback to try if "bool" has no effect.
    /// </summary>
    [JsonProperty("autoScreenShareFormat", NullValueHandling = NullValueHandling.Ignore)]
    public string AutoScreenShareFormat { get; set; }
}

