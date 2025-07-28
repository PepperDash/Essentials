using PepperDash.Essentials.Core.Feedbacks;

namespace PepperDash.Essentials.Core.Devices
{
    /// <summary>
    /// Defines the contract for IBasicVideoMute
    /// </summary>
    public interface IBasicVideoMute
    {
        void VideoMuteToggle();
    }

    /// <summary>
    /// Defines the contract for IBasicVideoMuteWithFeedback
    /// </summary>
    public interface IBasicVideoMuteWithFeedback : IBasicVideoMute
    {
        BoolFeedback VideoMuteIsOn { get; }

        void VideoMuteOn();
        void VideoMuteOff();
 
    }
}