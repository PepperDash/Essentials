using PepperDash.Essentials.Core.Feedbacks;

namespace PepperDash.Essentials.Core.Routing
{
    /// <summary>
    /// Defines the contract for ITxRouting
    /// </summary>
    public interface ITxRouting : IRoutingNumeric
    {
        /// <summary>
        /// Feedback indicating the currently routed video source by its numeric identifier.
        /// </summary>
        IntFeedback VideoSourceNumericFeedback { get; }
        /// <summary>
        /// Feedback indicating the currently routed audio source by its numeric identifier.
        /// </summary>
        IntFeedback AudioSourceNumericFeedback { get; }
    }
}