
namespace PepperDash.Essentials.Devices.Common.Cameras
{
    /// <summary>
    /// Defines the contract for IHasCameraFocusControl
    /// </summary>
    public interface IHasCameraFocusControl : IHasCameraControls
    {
        /// <summary>
        /// Focuses the camera near
        /// </summary>
        void FocusNear();

        /// <summary>
        /// Focuses the camera far
        /// </summary>
        void FocusFar();

        /// <summary>
        /// Stops the camera focus movement
        /// </summary>
        void FocusStop();

        /// <summary>
        /// Triggers the camera's auto focus functionality, if available.
        /// </summary>
        void TriggerAutoFocus();
    }
}
