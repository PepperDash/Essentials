using PepperDash.Core;

namespace PepperDash.Essentials.Core.Routing
{
    /// <summary>
    /// Defines a class that has a collection of RoutingInputPorts
    /// </summary>
    public interface IRoutingInputs : IKeyed
	{
		RoutingPortCollection<RoutingInputPort> InputPorts { get; }
	}

/*    public interface IRoutingInputs<TSelector> : IKeyed
    {
        RoutingPortCollection<RoutingInputPort<TSelector>, TSelector> InputPorts { get; }
    }*/
}