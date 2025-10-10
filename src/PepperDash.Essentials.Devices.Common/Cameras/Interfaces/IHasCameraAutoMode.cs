using PepperDash.Essentials.Core;

namespace PepperDash.Essentials.Devices.Common.Cameras
{

    /// <summary>
    /// Interface for devices that have camera auto mode control
    /// </summary>
    public interface IHasCameraAutoMode : IHasCameraControls
    {
        /// <summary>
        /// Enables or disables the camera's auto mode, which may include automatic adjustments for focus, exposure, and other settings.
        /// </summary>
        void CameraAutoModeOn();

        /// <summary>
        /// Disables the camera's auto mode, allowing for manual control of camera settings.
        /// </summary>
        void CameraAutoModeOff();

        /// <summary>
        /// Toggles the camera's auto mode state. If the camera is in auto mode, it will switch to manual mode, and vice versa.
        /// </summary>
        void CameraAutoModeToggle();

        /// <summary>
        /// Feedback that indicates whether the camera's auto mode is currently enabled.
        /// </summary>
        BoolFeedback CameraAutoModeIsOnFeedback { get; }
    }
}
