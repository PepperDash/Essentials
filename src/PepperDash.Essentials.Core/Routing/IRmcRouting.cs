namespace PepperDash.Essentials.Core
{
    /// <summary>
    /// Defines the contract for IRmcRouting
    /// </summary>
    public interface IRmcRouting : IRoutingNumeric
    {
        /// <summary>
        /// Feedback for the current Audio/Video source as a number
        /// </summary>
        IntFeedback AudioVideoSourceNumericFeedback { get; }
    }
}