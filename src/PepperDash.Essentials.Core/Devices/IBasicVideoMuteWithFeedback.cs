namespace PepperDash.Essentials.Core
{
    public interface IBasicVideoMuteWithFeedback : IBasicVideoMute
    {
        BoolFeedback VideoMuteIsOn { get; }

        void VideoMuteOn();
        void VideoMuteOff();
 
    }
}