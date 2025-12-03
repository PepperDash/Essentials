using Newtonsoft.Json;
using PepperDash.Core;
using PepperDash.Essentials.Core;
using System;
using System.Collections.Generic;

namespace PepperDash.Essentials.Devices.Common.Cameras
{
    /// <summary>
    /// Interface for devices that have cameras
    /// </summary>
    [Obsolete("Use IHasCamerasWithControls instead. This interface will be removed in a future version")]
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
}
