namespace PepperDash.Essentials.Devices.Common.Cameras
{
    /// <summary>
    /// Aggregates far end cameras with near end cameras
    /// </summary>
    public interface IHasCodecCameras : IHasCameras, IHasFarEndCameraControl
    {

    }
}