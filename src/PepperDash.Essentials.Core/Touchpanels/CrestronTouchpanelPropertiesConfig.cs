using Newtonsoft.Json;

namespace PepperDash.Essentials.Core
{
    public class CrestronTouchpanelPropertiesConfig
    {
        [JsonProperty("control")]
        public EssentialsControlPropertiesConfig ControlProperties { get; set; }

        [JsonProperty("ipId", NullValueHandling = NullValueHandling.Ignore)]
        public string IpId { get; set; }

        [JsonProperty("defaultRoomKey", NullValueHandling = NullValueHandling.Ignore)]
        public string DefaultRoomKey { get; set; }
        
        [JsonProperty("roomListKey", NullValueHandling = NullValueHandling.Ignore)]
        public string RoomListKey { get; set; }

        [JsonProperty("sgdFile", NullValueHandling = NullValueHandling.Ignore)]
        public string SgdFile { get; set; }

        [JsonProperty("projectName", NullValueHandling = NullValueHandling.Ignore)]
        public string ProjectName { get; set; }

        [JsonProperty("showVolumeGauge", NullValueHandling = NullValueHandling.Ignore)]
        public bool ShowVolumeGauge { get; set; }

        [JsonProperty("usesSplashPage", NullValueHandling = NullValueHandling.Ignore)]
        public bool UsesSplashPage { get; set; }

        [JsonProperty("showDate", NullValueHandling = NullValueHandling.Ignore)]
        public bool ShowDate { get; set; }

        [JsonProperty("showTime", NullValueHandling = NullValueHandling.Ignore)]
        public bool ShowTime { get; set; }

        [JsonProperty("setup", NullValueHandling = NullValueHandling.Ignore)]
        public UiSetupPropertiesConfig Setup { get; set; }

        [JsonProperty("headerStyle", NullValueHandling = NullValueHandling.Ignore)]
        public string HeaderStyle { get; set; }

        [JsonProperty("includeInFusionRoomHealth", NullValueHandling = NullValueHandling.Ignore)]
        public bool IncludeInFusionRoomHealth { get; set; }

        [JsonProperty("screenSaverTimeoutMin", NullValueHandling = NullValueHandling.Ignore)]
        public uint ScreenSaverTimeoutMin { get; set; }

        [JsonProperty("screenSaverMovePositionIntervalMs", NullValueHandling = NullValueHandling.Ignore)]
        public uint ScreenSaverMovePositionIntervalMs { get; set; }


        /// <summary>
        /// The count of sources that will trigger the "additional" arrows to show on the SRL.
        /// Defaults to 5
        /// </summary>
        [JsonProperty("sourcesOverflowCount", NullValueHandling = NullValueHandling.Ignore)]
        public int SourcesOverflowCount { get; set; }

        public CrestronTouchpanelPropertiesConfig()
        {
            SourcesOverflowCount = 5;
            HeaderStyle = CrestronTouchpanelPropertiesConfig.Habanero;

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
    /// 
    /// </summary>
    public class UiSetupPropertiesConfig
    {
        [JsonProperty("isVisible", NullValueHandling = NullValueHandling.Ignore)]
        public bool IsVisible { get; set; }
    }
}