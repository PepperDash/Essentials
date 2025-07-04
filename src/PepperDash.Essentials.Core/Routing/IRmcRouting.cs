namespace PepperDash.Essentials.Core;

/// <summary>
/// Defines a receiver that has internal routing (DM-RMC-4K-Z-SCALER-C)
/// </summary>
public interface IRmcRouting : IRoutingNumeric
{
    IntFeedback AudioVideoSourceNumericFeedback { get; }
}