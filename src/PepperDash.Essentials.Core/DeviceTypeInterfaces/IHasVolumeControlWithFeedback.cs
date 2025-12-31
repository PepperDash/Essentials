namespace PepperDash.Essentials.Core
{
  /// <summary>
  /// Defines volume control methods and properties with feedback
  /// </summary>
  public interface IHasVolumeControlWithFeedback : IHasVolumeControl
  {
    void SetVolume(ushort level);
    IntFeedback VolumeLevelFeedback { get; }
  }
}