namespace PepperDash.Essentials.Core
{
  /// <summary>
  /// Defines the contract for IHasVolumeDevice
  /// </summary>
  public interface IHasVolumeDevice
  {
    /// <summary>
    /// VolumeDevice property
    /// </summary>
    IBasicVolumeControls VolumeDevice { get; }
  }
}