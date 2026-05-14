namespace PepperDash.Essentials.Core;

/// <summary>
/// Defines a midpoint (passthrough) device that has both input and output routing ports
/// but does not perform active switching. For switching midpoints, see <see cref="IRoutingMidpointWithFeedback"/>.
/// </summary>
public interface IRoutingMidpoint : IRoutingInputs, IRoutingOutputs
{
}

