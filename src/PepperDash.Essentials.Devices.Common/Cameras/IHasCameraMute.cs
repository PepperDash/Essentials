using PepperDash.Essentials.Core;

namespace PepperDash.Essentials.Devices.Common.Cameras
{
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
}