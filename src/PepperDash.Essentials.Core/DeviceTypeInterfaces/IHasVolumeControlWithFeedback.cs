namespace PepperDash.Essentials.Core
{
  /// <summary>
  /// Defines volume control methods and properties with feedback
  /// </summary>
  public interface IHasVolumeControlWithFeedback : IHasVolumeControl
  {
    /// <summary>
    /// SetVolume method
    /// </summary>
    /// <param name="level">The volume level to set</param>
    void SetVolume(ushort level);

    /// <summary>
    /// VolumeLevelFeedback property
    /// </summary>
    IntFeedback VolumeLevelFeedback { get; }
  }
}