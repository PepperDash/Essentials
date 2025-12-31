namespace PepperDash.Essentials.Core
{
  /// <summary>
  /// Defines the contract for IBasicVolumeWithFeedbackAdvanced
  /// </summary>
  public interface IBasicVolumeWithFeedbackAdvanced : IBasicVolumeWithFeedback
  {
    int RawVolumeLevel { get; }

    eVolumeLevelUnits Units { get; }
  }
}