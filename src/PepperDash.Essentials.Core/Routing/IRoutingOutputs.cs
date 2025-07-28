namespace PepperDash.Essentials.Core.Routing
{

    /// <summary>
    /// Defines the contract for IRoutingOutputs
    /// </summary>
    public interface IRoutingOutputs : IKeyed
	{
		RoutingPortCollection<RoutingOutputPort> OutputPorts { get; }
	}

/*    public interface IRoutingOutputs<TSelector> : IKeyed
    {
        RoutingPortCollection<RoutingOutputPort<TSelector>, TSelector> OutputPorts { get; }
    }*/
}