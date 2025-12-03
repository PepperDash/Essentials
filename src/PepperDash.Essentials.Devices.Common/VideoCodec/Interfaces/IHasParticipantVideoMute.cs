namespace PepperDash.Essentials.Devices.Common.VideoCodec.Interfaces
{
  /// <summary>
  /// Describes the ability to mute and unmute a participant's video in a meeting
  /// </summary>
  public interface IHasParticipantVideoMute : IHasParticipants
  {
    /// <summary>
    /// Mute video of all participants
    /// </summary>
    /// <param name="userId">The user ID of the participant to mute</param>
    void MuteVideoForParticipant(int userId);

    /// <summary>
    /// Unmute video of all participants
    /// </summary>
    /// <param name="userId">The user ID of the participant to unmute</param>
    void UnmuteVideoForParticipant(int userId);

    /// <summary>
    /// Toggles video of a participant
    /// </summary>
    /// <param name="userId">The user ID of the participant to toggle</param>
    void ToggleVideoForParticipant(int userId);
  }
}