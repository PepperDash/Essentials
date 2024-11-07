using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Crestron.SimplSharp;

using PepperDash.Essentials.Core;
using PepperDash.Essentials.Core.Feedbacks;

namespace PepperDash.Essentials.Devices.Common.Cameras
{
    public enum eCameraControlMode
    {       
        Manual = 0,
        Off,
        Auto
    }


    public interface IHasCameras
    {
        event EventHandler<CameraSelectedEventArgs> CameraSelected;

        List<CameraBase> Cameras { get; }

        CameraBase SelectedCamera { get; }

        StringFeedback SelectedCameraFeedback { get; }

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
        BoolFeedback CameraIsOffFeedback { get; }
        void CameraOff();
    }

    /// <summary>
    /// Describes the ability to mute and unmute camera video
    /// </summary>
    public interface IHasCameraMute
    {
        BoolFeedback CameraIsMutedFeedback { get; }
        void CameraMuteOn();
        void CameraMuteOff();
        void CameraMuteToggle();
    }

    public interface IHasCameraMuteWithUnmuteReqeust : IHasCameraMute
    {
        event EventHandler VideoUnmuteRequested;
    }

    public class CameraSelectedEventArgs : EventArgs
    {
        public CameraBase SelectedCamera { get; private set; }

        public CameraSelectedEventArgs(CameraBase camera)
        {
            SelectedCamera = camera;
        }
    }

    public interface IHasFarEndCameraControl
    {
        CameraBase FarEndCamera { get; }

        BoolFeedback ControllingFarEndCameraFeedback { get; }

    }

    /// <summary>
    /// Used to decorate a camera as a far end
    /// </summary>
    public interface IAmFarEndCamera
    {

    }

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
        void PanLeft();
        void PanRight();
        void PanStop();
    }

    /// <summary>
    /// Interface for camera tilt control
    /// </summary>
    public interface IHasCameraTiltControl : IHasCameraControls
    {
        void TiltDown();
        void TiltUp();
        void TiltStop();
    }

    /// <summary>
    /// Interface for camera zoom control
    /// </summary>
    public interface IHasCameraZoomControl : IHasCameraControls
    {
        void ZoomIn();
        void ZoomOut();
        void ZoomStop();
    }

    /// <summary>
    /// Interface for camera focus control
    /// </summary>
    public interface IHasCameraFocusControl : IHasCameraControls
    {
        void FocusNear();
        void FocusFar();
        void FocusStop();

        void TriggerAutoFocus();
    }

    public interface IHasAutoFocusMode
    {
        void SetFocusModeAuto();
        void SetFocusModeManual();
        void ToggleFocusMode();
    }

    public interface IHasCameraAutoMode : IHasCameraControls
    {
        void CameraAutoModeOn();
        void CameraAutoModeOff();
        void CameraAutoModeToggle();
        BoolFeedback CameraAutoModeIsOnFeedback { get; }
    }




}