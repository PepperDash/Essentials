namespace PepperDash.Essentials.Core
{
  /// <summary>
  /// Defines the contract for IBasicVolumeWithFeedback
  /// </summary>
  public interface IBasicVolumeWithFeedback : IBasicVolumeControls
  {
    BoolFeedback MuteFeedback { get; }
    void MuteOn();
    void MuteOff();
    void SetVolume(ushort level);
    IntFeedback VolumeLevelFeedback { get; }
  }
}