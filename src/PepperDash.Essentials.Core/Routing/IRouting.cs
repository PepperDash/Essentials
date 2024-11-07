namespace PepperDash.Essentials.Core.Routing
{
    /// <summary>
    /// Defines a midpoint device as have internal routing.  Any devices in the middle of the
    /// signal chain, that do switching, must implement this for routing to work otherwise
    /// the routing algorithm will treat the IRoutingInputsOutputs device as a passthrough
    /// device.
    /// </summary>
    public interface IRouting : IRoutingInputsOutputs
	{
		void ExecuteSwitch(object inputSelector, object outputSelector, eRoutingSignalType signalType);        
    }    

    /*public interface IRouting<TInputSelector,TOutputSelector> : IRoutingInputsOutputs
    {
        void ExecuteSwitch(TInputSelector inputSelector, TOutputSelector outputSelector, eRoutingSignalType signalType);
    }*/
}