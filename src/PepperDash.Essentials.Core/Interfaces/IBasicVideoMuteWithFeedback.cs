namespace PepperDash.Essentials.Core.Interfaces
{
    public interface IBasicVideoMuteWithFeedback : IBasicVideoMute
    {
        BoolFeedback VideoMuteIsOn { get; }

        void VideoMuteOn();
        void VideoMuteOff();
 
    }
}