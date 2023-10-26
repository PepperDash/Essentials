namespace PepperDash.Essentials.Devices.Common.Cameras
{
    /// <summary>
    /// Interface for camera tilt control
    /// </summary>
    public interface IHasCameraTiltControl : IHasCameraControls
    {
        void TiltDown();
        void TiltUp();
        void TiltStop();
    }
}