namespace PepperDash.Essentials.Core.Interfaces
{
    /// <summary>
    /// Defines mute control methods and properties with feedback
    /// </summary>
    public interface IHasMuteControlWithFeedback : IHasMuteControl
    {
        BoolFeedback MuteFeedback { get; }
        void MuteOn();
        void MuteOff();
    }
}