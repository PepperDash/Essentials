namespace PepperDash.Essentials.Devices.Common.Cameras
{
    /// <summary>
    /// Aggregates the pan, tilt and zoom interfaces
    /// </summary>
    public interface IHasCameraPtzControl : IHasCameraPanControl, IHasCameraTiltControl, IHasCameraZoomControl
    {
        /// <summary>
        /// Resets the camera position
        /// </summary>
        void PositionHome();
    }
}