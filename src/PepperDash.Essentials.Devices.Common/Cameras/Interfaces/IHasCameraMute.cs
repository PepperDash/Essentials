using PepperDash.Core;
using PepperDash.Essentials.Core;

namespace PepperDash.Essentials.Devices.Common.Cameras
{
    /// <summary>
    /// Describes the ability to mute and unmute camera video
    /// </summary>
    public interface IHasCameraMute : IKeyName
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
}
