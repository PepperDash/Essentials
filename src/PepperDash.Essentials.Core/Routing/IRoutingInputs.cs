using PepperDash.Core;
namespace PepperDash.Essentials.Core;

/// <summary>
/// Defines the contract for IRoutingInputs
/// </summary>
public interface IRoutingInputs : IKeyed
{
    /// <summary>
    /// Collection of Input Ports
    /// </summary>
    RoutingPortCollection<RoutingInputPort> InputPorts { get; }
}

