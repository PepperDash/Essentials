namespace PepperDash.Essentials.Core
{
  /// <summary>
  /// Defines the contract for IBasicVolumeWithFeedback
  /// </summary>
  public interface IBasicVolumeWithFeedback : IBasicVolumeControls
  {

    /// <summary>
    /// Mutes the volume
    /// </summary>
    void MuteOn();

    /// <summary>
    /// Unmutes the volume
    /// </summary>
    void MuteOff();

    /// <summary>
    /// Sets the volume to the specified level
    /// </summary>
    /// <param name="level">The volume level to set</param>
    void SetVolume(ushort level);

    /// <summary>
    /// Gets the mute feedback
    /// </summary>
    IntFeedback VolumeLevelFeedback { get; }
  }
}