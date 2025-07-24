using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Crestron.SimplSharp;
using PepperDash.Core;
using PepperDash.Essentials.Core;

namespace PepperDash.Essentials.Devices.Common.Cameras
{
    /// <summary>
    /// Enum for camera control modes
    /// </summary>
    public enum eCameraControlMode
    {
        /// <summary>
        /// Manual control mode, where the camera is controlled directly by the user or system
        /// </summary>
        Manual = 0,
        /// <summary>
        /// Off control mode, where the camera is turned off or disabled
        /// </summary>
        Off,
        /// <summary>
        ///  Auto control mode, where the camera automatically adjusts settings based on the environment or conditions
        /// </summary>
        Auto
    }


    /// <summary>
    /// Interface for devices that have cameras
    /// </summary>
    public interface IHasCameras : IKeyName
    {
        /// <summary>
        /// Event that is raised when a camera is selected
        /// </summary>
        event EventHandler<CameraSelectedEventArgs> CameraSelected;

        /// <summary>
        /// List of cameras on the device.  This should be a list of CameraBase objects
        /// </summary>
        List<CameraBase> Cameras { get; }

        /// <summary>
        /// The currently selected camera.  This should be a CameraBase object
        /// </summary>
        CameraBase SelectedCamera { get; }

        /// <summary>
        /// Feedback that indicates the currently selected camera
        /// </summary>
        StringFeedback SelectedCameraFeedback { get; }

        /// <summary>
        /// Selects a camera from the list of available cameras based on the provided key.
        /// </summary>
        /// <param name="key">The unique identifier or name of the camera to select.</param>
        void SelectCamera(string key);
    }

    /// <summary>
    /// Aggregates far end cameras with near end cameras
    /// </summary>
    public interface IHasCodecCameras : IHasCameras, IHasFarEndCameraControl
    {

    }

    /// <summary>
    /// To be implmented on codecs that can disable their camera(s) to blank the near end video
    /// </summary>
    public interface IHasCameraOff
    {
        /// <summary>
        /// Feedback that indicates whether the camera is off
        /// </summary>
        BoolFeedback CameraIsOffFeedback { get; }

        /// <summary>
        /// Turns the camera off, blanking the near end video
        /// </summary>
        void CameraOff();
    }

    /// <summary>
    /// Describes the ability to mute and unmute camera video
    /// </summary>
    public interface IHasCameraMute
    {
        /// <summary>
        /// Feedback that indicates whether the camera is muted
        /// </summary>
        BoolFeedback CameraIsMutedFeedback { get; }

        /// <summary>
        /// Mutes the camera video, preventing it from being sent to the far end
        /// </summary>
        void CameraMuteOn();

        /// <summary>
        /// Unmutes the camera video, allowing it to be sent to the far end
        /// </summary>
        void CameraMuteOff();

        /// <summary>
        /// Toggles the camera mute state. If the camera is muted, it will be unmuted, and vice versa.
        /// </summary>
        void CameraMuteToggle();
    }

    /// <summary>
    /// Interface for devices that can mute and unmute their camera video, with an event for unmute requests
    /// </summary>
    public interface IHasCameraMuteWithUnmuteReqeust : IHasCameraMute
    {
        /// <summary>
        /// Event that is raised when a video unmute is requested, typically by the far end
        /// </summary>
        event EventHandler VideoUnmuteRequested;
    }

    /// <summary>
    /// Event arguments for the CameraSelected event
    /// </summary>
    public class CameraSelectedEventArgs : EventArgs
    {
        /// <summary>
        /// The selected camera
        /// </summary>
        public CameraBase SelectedCamera { get; private set; }

        /// <summary>
        /// Constructor for CameraSelectedEventArgs
        /// </summary>
        /// <param name="camera"></param>
        public CameraSelectedEventArgs(CameraBase camera)
        {
            SelectedCamera = camera;
        }
    }

    /// <summary>
    /// Interface for devices that have a far end camera control
    /// </summary>
    public interface IHasFarEndCameraControl
    {
        /// <summary>
        /// Gets the far end camera, which is typically a CameraBase object that represents the camera at the far end of a call
        /// </summary>
        CameraBase FarEndCamera { get; }

        /// <summary>
        /// Feedback that indicates whether the far end camera is being controlled
        /// </summary>
        BoolFeedback ControllingFarEndCameraFeedback { get; }

    }

    /// <summary>
    /// Used to decorate a camera as a far end
    /// </summary>
    public interface IAmFarEndCamera
    {

    }

    /// <summary>
    /// Interface for devices that have camera controls
    /// </summary>
    public interface IHasCameraControls
    {
    }

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

    /// <summary>
    /// Interface for camera pan control
    /// </summary>
    public interface IHasCameraPanControl : IHasCameraControls
    {
        /// <summary>
        /// Pans the camera left
        /// </summary>
        void PanLeft();

        /// <summary>
        /// Pans the camera right
        /// </summary>
        void PanRight();

        /// <summary>
        /// Stops the camera pan movement
        /// </summary>
        void PanStop();
    }

    /// <summary>
    /// Interface for camera tilt control
    /// </summary>
    public interface IHasCameraTiltControl : IHasCameraControls
    {
        /// <summary>
        /// Tilts the camera down
        /// </summary>
        void TiltDown();

        /// <summary>
        /// Tilts the camera up
        /// </summary>
        void TiltUp();

        /// <summary>
        /// Stops the camera tilt movement
        /// </summary>
        void TiltStop();
    }

    /// <summary>
    /// Interface for camera zoom control
    /// </summary>
    public interface IHasCameraZoomControl : IHasCameraControls
    {
        /// <summary>
        /// Zooms the camera in
        /// </summary>
        void ZoomIn();

        /// <summary>
        /// Zooms the camera out
        /// </summary>
        void ZoomOut();

        /// <summary>
        /// Stops the camera zoom movement
        /// </summary>
        void ZoomStop();
    }

    /// <summary>
    /// Interface for camera focus control
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

    /// <summary>
    /// Interface for devices that have auto focus mode control
    /// </summary>
    public interface IHasAutoFocusMode
    {
        /// <summary>
        /// Sets the focus mode to auto or manual, or toggles between them.
        /// </summary>
        void SetFocusModeAuto();

        /// <summary>
        /// Sets the focus mode to manual, allowing for manual focus adjustments.
        /// </summary>
        void SetFocusModeManual();

        /// <summary>
        /// Toggles the focus mode between auto and manual.
        /// </summary>
        void ToggleFocusMode();
    }

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