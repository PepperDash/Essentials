namespace PepperDash.Essentials.Core.CrestronIO
{
    /// <summary>
    /// Represents a device that provides digital input
    /// </summary>
    public interface IDigitalInput
    {
        BoolFeedback InputStateFeedback { get; }
    }
}