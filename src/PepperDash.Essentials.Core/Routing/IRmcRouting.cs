using PepperDash.Essentials.Core.Feedbacks;

namespace PepperDash.Essentials.Core.Routing
{
    /// <summary>
    /// Defines a receiver that has internal routing (DM-RMC-4K-Z-SCALER-C)
    /// </summary>
    public interface IRmcRouting : IRoutingNumeric
    {
        IntFeedback AudioVideoSourceNumericFeedback { get; }
    }
}