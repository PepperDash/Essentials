namespace PepperDash.Essentials.Core
{
  /// <summary>
  /// Defines the contract for IBasicVolumeWithFeedbackAdvanced
  /// </summary>
  public interface IBasicVolumeWithFeedbackAdvanced : IBasicVolumeWithFeedback
  {
    /// <summary>
    /// Gets the raw volume level
    /// </summary>
    int RawVolumeLevel { get; }

    /// <summary>
    /// Gets the volume level units
    /// </summary>
    eVolumeLevelUnits Units { get; }
  }
}