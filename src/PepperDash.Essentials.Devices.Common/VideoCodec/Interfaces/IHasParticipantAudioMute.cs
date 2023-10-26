namespace PepperDash.Essentials.Devices.Common.VideoCodec.Interfaces
{
    /// <summary>
    /// Describes the ability to mute and unmute a participant's audio in a meeting
    /// </summary>
    public interface IHasParticipantAudioMute : IHasParticipantVideoMute
    {
        /// <summary>
        /// Mute audio of all participants
        /// </summary>
        void MuteAudioForAllParticipants();

        void MuteAudioForParticipant(int userId);
        void UnmuteAudioForParticipant(int userId);
        void ToggleAudioForParticipant(int userId);
    }
}