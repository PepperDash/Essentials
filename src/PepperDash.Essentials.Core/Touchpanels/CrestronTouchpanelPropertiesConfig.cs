using Newtonsoft.Json;

namespace PepperDash.Essentials.Core
{
    /// <summary>
    /// Represents a CrestronTouchpanelPropertiesConfig
    /// </summary>
    public class CrestronTouchpanelPropertiesConfig
    {
        /// <summary>
        /// Gets or sets the ControlProperties
        /// </summary>
        [JsonProperty("control")]
        public EssentialsControlPropertiesConfig ControlProperties { get; set; }

        /// <summary>
        /// Gets or sets the IpId
        /// </summary>
        [JsonProperty("ipId", NullValueHandling = NullValueHandling.Ignore)]
        public string IpId { get; set; }

        /// <summary>
        /// Gets or sets the DefaultRoomKey
        /// </summary>
        [JsonProperty("defaultRoomKey", NullValueHandling = NullValueHandling.Ignore)]
        public string DefaultRoomKey { get; set; }
        
        /// <summary>
        /// Gets or sets the RoomListKey
        /// </summary>
        [JsonProperty("roomListKey", NullValueHandling = NullValueHandling.Ignore)]
        public string RoomListKey { get; set; }

        /// <summary>
        /// Gets or sets the SgdFile
        /// </summary>
        [JsonProperty("sgdFile", NullValueHandling = NullValueHandling.Ignore)]
        public string SgdFile { get; set; }

        /// <summary>
        /// Gets or sets the ProjectName
        /// </summary>
        [JsonProperty("projectName", NullValueHandling = NullValueHandling.Ignore)]
        public string ProjectName { get; set; }

        /// <summary>
        /// Gets or sets the ShowVolumeGauge
        /// </summary>
        [JsonProperty("showVolumeGauge", NullValueHandling = NullValueHandling.Ignore)]
        public bool? ShowVolumeGauge { get; set; }

        /// <summary>
        /// Gets or sets the UsesSplashPage
        /// </summary>
        [JsonProperty("usesSplashPage", NullValueHandling = NullValueHandling.Ignore)]
        public bool? UsesSplashPage { get; set; }

        /// <summary>
        /// Gets or sets the ShowDate
        /// </summary>
        [JsonProperty("showDate", NullValueHandling = NullValueHandling.Ignore)]
        public bool? ShowDate { get; set; }

        /// <summary>
        /// Gets or sets the ShowTime
        /// </summary>
        [JsonProperty("showTime", NullValueHandling = NullValueHandling.Ignore)]
        public bool? ShowTime { get; set; }

        /// <summary>
        /// Gets or sets the Setup
        /// </summary>
        [JsonProperty("setup", NullValueHandling = NullValueHandling.Ignore)]
        public UiSetupPropertiesConfig Setup { get; set; }

        /// <summary>
        /// Gets or sets the HeaderStyle
        /// </summary>
        [JsonProperty("headerStyle", NullValueHandling = NullValueHandling.Ignore)]
        public string HeaderStyle { get; set; }

        /// <summary>
        /// Gets or sets the IncludeInFusionRoomHealth
        /// </summary>
        [JsonProperty("includeInFusionRoomHealth", NullValueHandling = NullValueHandling.Ignore)]
        public bool? IncludeInFusionRoomHealth { get; set; }

        /// <summary>
        /// Gets or sets the ScreenSaverTimeoutMin
        /// </summary>
        [JsonProperty("screenSaverTimeoutMin", NullValueHandling = NullValueHandling.Ignore)]
        public uint? ScreenSaverTimeoutMin { get; set; }

        /// <summary>
        /// Gets or sets the ScreenSaverMovePositionIntervalMs
        /// </summary>
        [JsonProperty("screenSaverMovePositionIntervalMs", NullValueHandling = NullValueHandling.Ignore)]
        public uint? ScreenSaverMovePositionIntervalMs { get; set; }


        /// <summary>
        /// The count of sources that will trigger the "additional" arrows to show on the SRL.
        /// Defaults to 5
        /// </summary>
        [JsonProperty("sourcesOverflowCount", NullValueHandling = NullValueHandling.Ignore)]
        public int? SourcesOverflowCount { get; set; }

        /// <summary>
        /// Constructor
        /// </summary>
        public CrestronTouchpanelPropertiesConfig() : this(false) { }        

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="setDefaultValues">set values to default if true</param>
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
    /// Represents a UiSetupPropertiesConfig
    /// </summary>
    public class UiSetupPropertiesConfig
    {
        /// <summary>
        /// Gets or sets the IsVisible
        /// </summary>
        [JsonProperty("isVisible", NullValueHandling = NullValueHandling.Ignore)]
        public bool IsVisible { get; set; }
    }
}