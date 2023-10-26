namespace PepperDash.Essentials.Core
{
    /// <summary>
    /// Adds feedback and direct volume level set to IBasicVolumeControls
    /// </summary>
    public interface IBasicVolumeWithFeedback : IBasicVolumeControls
    {
        BoolFeedback MuteFeedback { get; }
        void MuteOn();
        void MuteOff();
        void SetVolume(ushort level);
        IntFeedback VolumeLevelFeedback { get; }
    }
}