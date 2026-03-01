using System;

namespace PepperDash.Essentials.Core
{
  /// <summary>
  /// Defines the contract for IHasCurrentVolumeControls
  /// </summary>
  public interface IHasCurrentVolumeControls
  {
    /// <summary>
    /// CurrentVolumeControls property
    /// </summary>
    IBasicVolumeControls CurrentVolumeControls { get; }

    /// <summary>
    /// CurrentVolumeDeviceChange event
    /// </summary>
    event EventHandler<VolumeDeviceChangeEventArgs> CurrentVolumeDeviceChange;

    /// <summary>
    /// SetDefaultLevels method
    /// </summary>
    void SetDefaultLevels();

    /// <summary>
    /// ZeroVolumeWhenSwtichingVolumeDevices property
    /// </summary>
    bool ZeroVolumeWhenSwtichingVolumeDevices { get; }
  }
}