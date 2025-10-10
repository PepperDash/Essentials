namespace PepperDash.Essentials.Devices.Common.VideoCodec.Interfaces
{
  /// <summary>
  /// Defines the contract for IHasParticipantAudioMute
  /// </summary>
  public interface IHasParticipantAudioMute : IHasParticipantVideoMute
  {
    /// <summary>
    /// Mute audio of all participants
    /// </summary>
    void MuteAudioForAllParticipants();

    /// <summary>
    /// Mute audio for participant
    /// </summary>
    /// <param name="userId">The user ID of the participant to mute</param>
    void MuteAudioForParticipant(int userId);

    /// <summary>
    /// Unmute audio for participant
    /// </summary>
    /// <param name="userId">The user ID of the participant to unmute</param>
    void UnmuteAudioForParticipant(int userId);

    /// <summary>
    /// Toggles audio for participant
    /// </summary>
    /// <param name="userId">The user ID of the participant to toggle</param>
    void ToggleAudioForParticipant(int userId);
  }
}