namespace PepperDash.Essentials.Core
{
    /// <summary>
    /// Defines the contract for IRmcRouting
    /// </summary>
    public interface IRmcRouting : IRoutingNumeric
    {
        IntFeedback AudioVideoSourceNumericFeedback { get; }
    }
}