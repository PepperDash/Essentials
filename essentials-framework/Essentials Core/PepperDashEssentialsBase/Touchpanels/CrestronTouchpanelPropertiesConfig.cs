namespace PepperDash.Essentials.Core
{
    public class CrestronTouchpanelPropertiesConfig
    {
        public string IpId { get; set; }
        public string DefaultRoomKey { get; set; }
        public string RoomListKey { get; set; }
        public string SgdFile { get; set; }
        public string ProjectName { get; set; }
        public bool ShowVolumeGauge { get; set; }
        public bool UsesSplashPage { get; set; }
        public bool ShowDate { get; set; }
        public bool ShowTime { get; set; }
        public UiSetupPropertiesConfig Setup { get; set; }
        public string HeaderStyle { get; set; }
        public bool IncludeInFusionRoomHealth { get; set; }


        /// <summary>
        /// The count of sources that will trigger the "additional" arrows to show on the SRL.
        /// Defaults to 5
        /// </summary>
        public int SourcesOverflowCount { get; set; }

        public CrestronTouchpanelPropertiesConfig()
        {
            SourcesOverflowCount = 5;
            HeaderStyle = CrestronTouchpanelPropertiesConfig.Habanero;
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
        public bool IsVisible { get; set; }
    }
}