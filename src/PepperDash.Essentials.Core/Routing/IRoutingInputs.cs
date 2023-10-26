using PepperDash.Core;

namespace PepperDash.Essentials.Core
{
    /// <summary>
    /// Defines a class that has a collection of RoutingInputPorts
    /// </summary>
    public interface IRoutingInputs : IKeyed
    {
        RoutingPortCollection<RoutingInputPort> InputPorts { get; }
    }
}