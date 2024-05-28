using Newtonsoft.Json;

namespace PepperDash.Essentials.Core
{
    public class CrestronTouchpanelPropertiesConfig
    {
        [JsonProperty("ipId")]
        public string IpId { get; set; }

        [JsonProperty("defaultRoomKey")]
        public string DefaultRoomKey { get; set; }
        
        [JsonProperty("roomListKey")]
        public string RoomListKey { get; set; }

        [JsonProperty("sgdFile")]
        public string SgdFile { get; set; }

        [JsonProperty("projectName")]
        public string ProjectName { get; set; }

        [JsonProperty("showVolumeGauge")]
        public bool ShowVolumeGauge { get; set; }

        [JsonProperty("usesSplashPage")]
        public bool UsesSplashPage { get; set; }

        [JsonProperty("showDate")]
        public bool ShowDate { get; set; }

        [JsonProperty("showTime")]
        public bool ShowTime { get; set; }

        [JsonProperty("setup")]
        public UiSetupPropertiesConfig Setup { get; set; }

        [JsonProperty("headerStyle")]
        public string HeaderStyle { get; set; }

        [JsonProperty("includeInFusionRoomHealth")]
        public bool IncludeInFusionRoomHealth { get; set; }

        [JsonProperty("screenSaverTimeoutMin")]
        public uint ScreenSaverTimeoutMin { get; set; }

        [JsonProperty("screenSaverMovePositionIntervalMs")]
        public uint ScreenSaverMovePositionIntervalMs { get; set; }


        /// <summary>
        /// The count of sources that will trigger the "additional" arrows to show on the SRL.
        /// Defaults to 5
        /// </summary>
        [JsonProperty("sourcesOverflowCount")]
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
        [JsonProperty("isVisible")]
        public bool IsVisible { get; set; }
    }
}