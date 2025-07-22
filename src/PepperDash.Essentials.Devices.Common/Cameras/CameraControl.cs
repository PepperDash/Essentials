using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Crestron.SimplSharp;

using PepperDash.Essentials.Core;

namespace PepperDash.Essentials.Devices.Common.Cameras
{
    /// <summary>
    /// Enumeration of eCameraControlMode values
    /// </summary>
    public enum eCameraControlMode
    {       
        Manual = 0,
        Off,
        Auto
    }


    /// <summary>
    /// Defines the contract for IHasCameras
    /// </summary>
    public interface IHasCameras
    {
        event EventHandler<CameraSelectedEventArgs> CameraSelected;

        List<CameraBase> Cameras { get; }

        CameraBase SelectedCamera { get; }

        StringFeedback SelectedCameraFeedback { get; }

        void SelectCamera(string key);
    }

    /// <summary>
    /// Defines the contract for IHasCodecCameras
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

    /// <summary>
    /// Represents a CameraSelectedEventArgs
    /// </summary>
    public class CameraSelectedEventArgs : EventArgs
    {
        /// <summary>
        /// Gets or sets the SelectedCamera
        /// </summary>
        public CameraBase SelectedCamera { get; private set; }

        public CameraSelectedEventArgs(CameraBase camera)
        {
            SelectedCamera = camera;
        }
    }

    /// <summary>
    /// Defines the contract for IHasFarEndCameraControl
    /// </summary>
    public interface IHasFarEndCameraControl
    {
        CameraBase FarEndCamera { get; }

        BoolFeedback ControllingFarEndCameraFeedback { get; }

    }

    /// <summary>
    /// Defines the contract for IAmFarEndCamera
    /// </summary>
    public interface IAmFarEndCamera
    {

    }

    public interface IHasCameraControls
    {
    }

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
    /// Defines the contract for IHasCameraTiltControl
    /// </summary>
    public interface IHasCameraTiltControl : IHasCameraControls
    {
        void TiltDown();
        void TiltUp();
        void TiltStop();
    }

    /// <summary>
    /// Defines the contract for IHasCameraZoomControl
    /// </summary>
    public interface IHasCameraZoomControl : IHasCameraControls
    {
        void ZoomIn();
        void ZoomOut();
        void ZoomStop();
    }

    /// <summary>
    /// Defines the contract for IHasCameraFocusControl
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

    /// <summary>
    /// Defines the contract for IHasCameraAutoMode
    /// </summary>
    public interface IHasCameraAutoMode : IHasCameraControls
    {
        void CameraAutoModeOn();
        void CameraAutoModeOff();
        void CameraAutoModeToggle();
        BoolFeedback CameraAutoModeIsOnFeedback { get; }
    }




}