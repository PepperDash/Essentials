namespace PepperDash.Essentials.Devices.Common.Cameras
{
    /// <summary>
    /// Interface for camera zoom control
    /// </summary>
    public interface IHasCameraZoomControl : IHasCameraControls
    {
        void ZoomIn();
        void ZoomOut();
        void ZoomStop();
    }
}