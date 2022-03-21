namespace PepperDash.Essentials.Core.Interfaces
{
    /// <summary>
    /// Defines basic volume control methods
    /// </summary>
    public interface IHasVolumeControl
    {
        void VolumeUp(bool pressRelease);
        void VolumeDown(bool pressRelease);
    }
}