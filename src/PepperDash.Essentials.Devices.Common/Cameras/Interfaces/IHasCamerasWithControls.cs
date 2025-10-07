using PepperDash.Core;
using PepperDash.Essentials.Core;
using System;
using System.Collections.Generic;

namespace PepperDash.Essentials.Devices.Common.Cameras
{
    /// <summary>
    /// Interface for devices that have cameras with controls
    /// </summary>
    public interface IHasCamerasWithControls : IKeyName, IKeyed
    {
        /// <summary>
        /// List of cameras on the device. This should be a list of IHasCameraControls objects
        /// </summary>
        
        List<IHasCameraControls> Cameras { get; }

        /// <summary>
        /// The currently selected camera. This should be an IHasCameraControls object
        /// </summary>
        IHasCameraControls SelectedCamera { get; }

        /// <summary>
        /// Feedback that indicates the currently selected camera
        /// </summary>
        StringFeedback SelectedCameraFeedback { get; }

        /// <summary>
        /// Event that is raised when a camera is selected
        /// </summary>
        event EventHandler<CameraSelectedEventArgs<IHasCameraControls>> CameraSelected;

        /// <summary>
        /// Selects a camera from the list of available cameras based on the provided key.
        /// </summary>
        /// <param name="key"></param>
        void SelectCamera(string key);
    }
}
