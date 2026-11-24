using Newtonsoft.Json;

namespace PepperDash.Essentials.Core;

/// <summary>
/// The config properties of a Crestron touchpanel device.
/// </summary>
public class CrestronTouchpanelPropertiesConfig
{
    /// <summary>
    /// How Essentials connects to the panel. Its ipId is the IP ID the panel registers with.
    /// </summary>
    [JsonProperty("control")]
    public EssentialsControlPropertiesConfig ControlProperties { get; set; }

    /// <summary>
    /// Essentials 3.x doesn't read this top-level IP ID.
    /// Set control.ipId to the panel's IP ID as a hex string ("03").
    /// </summary>
    [JsonProperty("ipId", NullValueHandling = NullValueHandling.Ignore)]
    public string IpId { get; set; }

    /// <summary>
    /// The key of the room the panel controls when it starts. When a room combiner is in use,
    /// the active scenario's UI map can move the panel to a different room.
    /// </summary>
    [JsonProperty("defaultRoomKey", NullValueHandling = NullValueHandling.Ignore)]
    public string DefaultRoomKey { get; set; }
    
    /// <summary>
    /// The key of a list of rooms the panel can switch between.
    /// Essentials 3.x doesn't read this; the touchpanel UI built into earlier versions of Essentials did.
    /// </summary>
    [JsonProperty("roomListKey", NullValueHandling = NullValueHandling.Ignore)]
    public string RoomListKey { get; set; }

    /// <summary>
    /// The file name of the panel's smart graphics definition (.sgd) file. Essentials looks for it in
    /// the user sgd folder, then in the program's SGD folder, and stops loading the panel if it finds neither.
    /// </summary>
    [JsonProperty("sgdFile", NullValueHandling = NullValueHandling.Ignore)]
    public string SgdFile { get; set; }

    /// <summary>
    /// The project name the Crestron app loads. Only the Crestron app panel type (mccrestronapp) uses it.
    /// </summary>
    [JsonProperty("projectName", NullValueHandling = NullValueHandling.Ignore)]
    public string ProjectName { get; set; }

    /// <summary>
    /// Show a volume gauge on the panel.
    /// Essentials 3.x doesn't read this; the touchpanel UI built into earlier versions of Essentials did.
    /// </summary>
    [JsonProperty("showVolumeGauge", NullValueHandling = NullValueHandling.Ignore)]
    public bool? ShowVolumeGauge { get; set; }

    /// <summary>
    /// Show a splash page before the main page.
    /// Essentials 3.x doesn't read this; the touchpanel UI built into earlier versions of Essentials did.
    /// </summary>
    [JsonProperty("usesSplashPage", NullValueHandling = NullValueHandling.Ignore)]
    public bool? UsesSplashPage { get; set; }

    /// <summary>
    /// Show the date in the panel's header.
    /// Essentials 3.x doesn't read this; the touchpanel UI built into earlier versions of Essentials did.
    /// </summary>
    [JsonProperty("showDate", NullValueHandling = NullValueHandling.Ignore)]
    public bool? ShowDate { get; set; }

    /// <summary>
    /// Show the time in the panel's header.
    /// Essentials 3.x doesn't read this; the touchpanel UI built into earlier versions of Essentials did.
    /// </summary>
    [JsonProperty("showTime", NullValueHandling = NullValueHandling.Ignore)]
    public bool? ShowTime { get; set; }

    /// <summary>
    /// Settings for the panel's setup page.
    /// </summary>
    [JsonProperty("setup", NullValueHandling = NullValueHandling.Ignore)]
    public UiSetupPropertiesConfig Setup { get; set; }

    /// <summary>
    /// The style of the panel's header: "habanero" or "verbose".
    /// Essentials 3.x doesn't read this; the touchpanel UI built into earlier versions of Essentials did.
    /// </summary>
    [JsonProperty("headerStyle", NullValueHandling = NullValueHandling.Ignore)]
    public string HeaderStyle { get; set; }

    /// <summary>
    /// Include the panel's online status in the room's Fusion room health.
    /// Essentials 3.x doesn't read this; the touchpanel UI built into earlier versions of Essentials did.
    /// </summary>
    [JsonProperty("includeInFusionRoomHealth", NullValueHandling = NullValueHandling.Ignore)]
    public bool? IncludeInFusionRoomHealth { get; set; }

    /// <summary>
    /// Minutes without a touch before the panel shows its screen saver. Defaults to 5.
    /// Essentials 3.x doesn't read this; the touchpanel UI built into earlier versions of Essentials did.
    /// </summary>
    [JsonProperty("screenSaverTimeoutMin", NullValueHandling = NullValueHandling.Ignore)]
    public uint? ScreenSaverTimeoutMin { get; set; }

    /// <summary>
    /// Milliseconds between moves of the screen saver's content, which keeps it from
    /// burning in. Defaults to 15000.
    /// Essentials 3.x doesn't read this; the touchpanel UI built into earlier versions of Essentials did.
    /// </summary>
    [JsonProperty("screenSaverMovePositionIntervalMs", NullValueHandling = NullValueHandling.Ignore)]
    public uint? ScreenSaverMovePositionIntervalMs { get; set; }


    /// <summary>
    /// The count of sources that will trigger the "additional" arrows to show on the SRL.
    /// Defaults to 5
    /// </summary>
    [JsonProperty("sourcesOverflowCount", NullValueHandling = NullValueHandling.Ignore)]
    public int? SourcesOverflowCount { get; set; }

    public CrestronTouchpanelPropertiesConfig() : this(false) { }        

    public CrestronTouchpanelPropertiesConfig(bool setDefaultValues = false)
    {
        if(!setDefaultValues) { return; }
        SourcesOverflowCount = 5;
        HeaderStyle = Habanero;

        // Default values
        ScreenSaverTimeoutMin = 5;
        ScreenSaverMovePositionIntervalMs = 15000;
    }        

    /// <summary>
    /// "habanero"
    /// </summary>
    public const string Habanero = "habanero";
    /// <summary>
    /// "verbose"
    /// </summary>
    public const string Verbose = "verbose";
}

/// <summary>
/// Settings for a touchpanel's setup page.
/// </summary>
public class UiSetupPropertiesConfig
{
    /// <summary>
    /// Show the setup page.
    /// Essentials 3.x doesn't read this; the touchpanel UI built into earlier versions of Essentials did.
    /// </summary>
    [JsonProperty("isVisible", NullValueHandling = NullValueHandling.Ignore)]
    public bool IsVisible { get; set; }
}