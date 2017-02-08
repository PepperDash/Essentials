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
	}
}