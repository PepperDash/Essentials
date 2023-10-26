namespace PepperDash.Essentials.Core
{
    /// <summary>
    /// Adds feedback for current power state
    /// </summary>
    public interface IHasPowerControlWithFeedback : IHasPowerControl
    {
        BoolFeedback PowerIsOnFeedback { get; }
    }
}