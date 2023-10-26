namespace PepperDash.Essentials.Devices.Common.VideoCodec.ZoomRoom
{
    /// <summary>
    /// Used to track the current configuration of a ZoomRoom
    /// </summary>
    public class ZoomRoomConfiguration
    {
        public zConfiguration.Call Call { get; set; }
        public zConfiguration.Audio Audio { get; set; }
        public zConfiguration.Video Video { get; set; }
        public zConfiguration.Client Client { get; set; }
        public zConfiguration.Camera Camera { get; set; }

        public ZoomRoomConfiguration()
        {
            Call   = new zConfiguration.Call();
            Audio  = new zConfiguration.Audio();
            Video  = new zConfiguration.Video();
            Client = new zConfiguration.Client();
            Camera = new zConfiguration.Camera();
        }
    }
}