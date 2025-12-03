
namespace PepperDash.Essentials.Devices.Common.Cameras
{
    /// <summary>
    /// Defines the contract for IHasCameraPtzControl
    /// </summary>
    public interface IHasCameraPtzControl : IHasCameraPanControl, IHasCameraTiltControl, IHasCameraZoomControl
    {
        /// <summary>
        /// Resets the camera position
        /// </summary>
        void PositionHome();
    }
}
