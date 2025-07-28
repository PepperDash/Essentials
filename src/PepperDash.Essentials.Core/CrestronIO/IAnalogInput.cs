using PepperDash.Essentials.Core.Feedbacks;

namespace PepperDash.Essentials.Core.CrestronIO
{
    /// <summary>
    /// Defines the contract for IAnalogInput
    /// </summary>
    public interface IAnalogInput
    {
        IntFeedback InputValueFeedback { get; }
    }
}