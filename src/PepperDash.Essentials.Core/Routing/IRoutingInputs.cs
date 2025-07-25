using PepperDash.Core;


namespace PepperDash.Essentials.Core
{
    /// <summary>
    /// Defines the contract for IRoutingInputs
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