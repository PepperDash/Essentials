using System;

namespace PepperDash.Essentials.Core
{
  /// <summary>
  /// Defines the contract for IHasCurrentVolumeControls
  /// </summary>
  public interface IHasCurrentVolumeControls
  {
    IBasicVolumeControls CurrentVolumeControls { get; }
    event EventHandler<VolumeDeviceChangeEventArgs> CurrentVolumeDeviceChange;

    void SetDefaultLevels();

    bool ZeroVolumeWhenSwtichingVolumeDevices { get; }
  }
}