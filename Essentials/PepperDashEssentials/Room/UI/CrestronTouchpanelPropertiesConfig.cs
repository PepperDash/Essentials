namespace PepperDash.Essentials
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
        public UiHeaderStyle HeaderStyle { get; set; }

        /// <summary>
        /// The count of sources that will trigger the "additional" arrows to show on the SRL.
        /// Defaults to 5
        /// </summary>
        public int SourcesOverflowCount { get; set; }

        public CrestronTouchpanelPropertiesConfig()
        {
            SourcesOverflowCount = 5;
            HeaderStyle = UiHeaderStyle.Habanero;
        }
	}

    /// <summary>
    /// 
    /// </summary>
    public class UiSetupPropertiesConfig
    {
        public bool IsVisible { get; set; }
    }

    /// <summary>
    /// 
    /// </summary>
    public enum UiHeaderStyle
    {
        Habanero,
        Verbose
    }

}