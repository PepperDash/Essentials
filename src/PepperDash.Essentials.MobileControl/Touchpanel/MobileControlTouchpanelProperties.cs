
using Newtonsoft.Json;
using PepperDash.Essentials.Core;

namespace PepperDash.Essentials.Touchpanel;

/// <summary>
/// The config properties of a Mobile Control touchpanel device.
/// </summary>
public class MobileControlTouchpanelProperties : CrestronTouchpanelPropertiesConfig
{

    /// <summary>
    /// Connect the panel's UI to the Mobile Control server running on the processor instead of
    /// through the Mobile Control cloud server.
    /// </summary>
    [JsonProperty("useDirectServer")]
    public bool UseDirectServer { get; set; } = false;

    /// <summary>
    /// The address of a development server, such as a Vite server, to load the panel's UI from instead of
    /// the processor. Optional. If UseDirectServer is true, 
    /// this IP address will be used for direct communication between the mobile control and a development server running on the same local network.
    /// This allows development using a Vite server for hot module replacement (HMR) and live reloading during development. 
    /// Specifying this property will override the default behavior of mobile control.
    /// </summary>
    [JsonProperty("developmentServerAddress", NullValueHandling = NullValueHandling.Ignore)]
    public string DevelopmentServerAddress { get; set; }


    /// <summary>
    /// Mark this panel as the system-wide Zoom Rooms controller. Enable this on at most one panel.
    /// When exactly one panel is marked, Mobile Control uses its key in every room's configuration.
    /// </summary>
    [JsonProperty("zoomRoomController")]
    public bool ZoomRoomController { get; set; } = false;


    /// <summary>
    /// Seconds the UI waits before hiding its button toolbar. Essentials doesn't read this.
    /// </summary>
    [JsonProperty("buttonToolbarTimeoutInS")]
    public ushort ButtonToolbarTimoutInS { get; set; } = 0;


    /// <summary>
    /// The UI's theme. Defaults to "light". When the UI changes it, Essentials saves the new theme to this config.
    /// </summary>
    [JsonProperty("theme")]
    public string Theme { get; set; } = "light";
}

