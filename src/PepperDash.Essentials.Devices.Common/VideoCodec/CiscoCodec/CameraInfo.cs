namespace PepperDash.Essentials.Devices.Common.Codec
{
    /// <summary>
    /// Describes configuration information for the near end cameras
    /// </summary>
    public class CameraInfo
    {
        public int CameraNumber { get; set; }
        public string Name { get; set; }
        public int SourceId { get; set; }
    }
}