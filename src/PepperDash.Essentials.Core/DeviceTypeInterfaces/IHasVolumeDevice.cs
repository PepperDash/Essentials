namespace PepperDash.Essentials.Core
{
  /// <summary>
  /// Defines the contract for IHasVolumeDevice
  /// </summary>
  public interface IHasVolumeDevice
  {
    IBasicVolumeControls VolumeDevice { get; }
  }
}