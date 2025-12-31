namespace PepperDash.Essentials.Core
{
  /// <summary>
  /// Defines minimum functionality for an audio zone
  /// </summary>
  public interface IAudioZone : IBasicVolumeWithFeedback
  {
    void SelectInput(ushort input);
  }
}