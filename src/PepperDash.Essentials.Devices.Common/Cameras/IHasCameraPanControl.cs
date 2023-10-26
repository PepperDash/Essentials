namespace PepperDash.Essentials.Devices.Common.Cameras
{
    /// <summary>
    /// Interface for camera pan control
    /// </summary>
    public interface IHasCameraPanControl : IHasCameraControls
    {
        void PanLeft();
        void PanRight();
        void PanStop();
    }
}