using PepperDash.Core;
using PepperDash.Essentials.Core;


namespace PepperDash.Essentials.Devices.Common.Cameras
{
    /// <summary>
    /// Interface for devices that have a far end camera control
    /// </summary>
    public interface IHasFarEndCameraControl : IKeyName
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
}
