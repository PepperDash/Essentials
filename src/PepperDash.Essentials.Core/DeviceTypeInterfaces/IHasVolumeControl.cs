namespace PepperDash.Essentials.Core
{
  /// <summary>
  /// Defines the contract for IHasVolumeControl
  /// </summary>
  public interface IHasVolumeControl
  {
    void VolumeUp(bool pressRelease);
    void VolumeDown(bool pressRelease);
  }
}