namespace PepperDash.Essentials.Core.Interfaces
{
    /// <summary>
    /// Adds feedback and direct volume level set to IBasicVolumeControls
    /// </summary>
    public interface IBasicVolumeWithFeedback : IBasicVolumeControls, IHasVolumeControlWithFeedback, IHasMuteControlWithFeedback
    {
    }
}