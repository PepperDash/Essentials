namespace PepperDash.Essentials.Core
{
  /// <summary>
  /// Defines minimum functionality for an audio zone
  /// </summary>
  public interface IAudioZone : IBasicVolumeWithFeedback
  {
    /// <summary>
    /// Selects the specified input
    /// </summary>
    /// <param name="input">The input to select</param>
    void SelectInput(ushort input);
  }
}