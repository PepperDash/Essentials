namespace PepperDash.Essentials.Core
{
    /// <summary>
    /// A class that implements this, contains a reference to an IBasicVolumeControls device.
    /// For example, speakers attached to an audio zone.  The speakers can provide reference
    /// to their linked volume control.
    /// </summary>
    public interface IHasVolumeDevice
    {
        IBasicVolumeControls VolumeDevice { get; }
    }
}