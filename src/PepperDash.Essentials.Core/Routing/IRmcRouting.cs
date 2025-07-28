using PepperDash.Essentials.Core.Feedbacks;

namespace PepperDash.Essentials.Core.Routing
{
    /// <summary>
    /// Defines the contract for IRmcRouting
    /// </summary>
    public interface IRmcRouting : IRoutingNumeric
    {
        IntFeedback AudioVideoSourceNumericFeedback { get; }
    }
}