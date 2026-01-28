using PepperDash.Core;


namespace PepperDash.Essentials.Core
{

    /// <summary>
    /// Defines the contract for IRoutingOutputs
    /// </summary>
    public interface IRoutingOutputs : IKeyed
	{
        /// <summary>
        /// Collection of Output Ports
        /// </summary>
		RoutingPortCollection<RoutingOutputPort> OutputPorts { get; }
	}

/*    public interface IRoutingOutputs<TSelector> : IKeyed
    {
        RoutingPortCollection<RoutingOutputPort<TSelector>, TSelector> OutputPorts { get; }
    }*/
}