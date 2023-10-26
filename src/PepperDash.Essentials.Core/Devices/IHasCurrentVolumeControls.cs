using System;

namespace PepperDash.Essentials.Core
{
    /// <summary>
    /// A class that implements this contains a reference to a current IBasicVolumeControls device.
    /// The class may have multiple IBasicVolumeControls.
    /// </summary>
    public interface IHasCurrentVolumeControls
    {
        IBasicVolumeControls CurrentVolumeControls { get; }
        event EventHandler<VolumeDeviceChangeEventArgs> CurrentVolumeDeviceChange;
    }
}