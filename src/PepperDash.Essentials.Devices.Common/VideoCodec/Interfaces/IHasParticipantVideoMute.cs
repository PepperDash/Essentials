namespace PepperDash.Essentials.Devices.Common.VideoCodec.Interfaces
{
    /// <summary>
    /// Describes the ability to mute and unmute a participant's video in a meeting
    /// </summary>
    public interface IHasParticipantVideoMute : IHasParticipants
    {
        void MuteVideoForParticipant(int userId);
        void UnmuteVideoForParticipant(int userId);
        void ToggleVideoForParticipant(int userId);
    }
}