namespace PepperDash.Essentials.Core.Interfaces
{
    /// <summary>
    /// Defines volume control methods and properties with feedback
    /// </summary>
    public interface IHasVolumeControlWithFeedback : IHasVolumeControl
    {
        void SetVolume(ushort level);
        IntFeedback VolumeLevelFeedback { get; }
    }
}