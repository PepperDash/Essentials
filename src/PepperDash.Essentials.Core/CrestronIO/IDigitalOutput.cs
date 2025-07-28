using PepperDash.Essentials.Core.Feedbacks;

namespace PepperDash.Essentials.Core.CrestronIO
{
    /// <summary>
    /// Represents a device that provides digital input
    /// </summary>
    public interface IDigitalOutput
    {
        BoolFeedback OutputStateFeedback { get; }
        void SetOutput(bool state);
    }
}