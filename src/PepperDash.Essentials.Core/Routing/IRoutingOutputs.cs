using PepperDash.Core;


namespace PepperDash.Essentials.Core
{
    /// <summary>
    /// Defines a class that has a collection of RoutingOutputPorts
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