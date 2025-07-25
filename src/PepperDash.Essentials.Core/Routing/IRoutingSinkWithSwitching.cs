namespace PepperDash.Essentials.Core
{
    /// <summary>
    /// Delegate for InputChangedEventHandler
    /// </summary>
    public delegate void InputChangedEventHandler(IRoutingSinkWithSwitching destination, RoutingInputPort currentPort);

    /// <summary>
    /// Defines the contract for IRoutingSinkWithSwitching
    /// </summary>
    public interface IRoutingSinkWithSwitching : IRoutingSink
	{		
		void ExecuteSwitch(object inputSelector);        
    }

    /// <summary>
    /// Defines the contract for IRoutingSinkWithSwitchingWithInputPort
    /// </summary>
    public interface IRoutingSinkWithSwitchingWithInputPort:IRoutingSinkWithSwitching, IRoutingSinkWithInputPort
    {
        event InputChangedEventHandler InputChanged;
    }

/*    /// <summary>
    /// Endpoint device like a display, that selects inputs
    /// </summary>
    /// <summary>
    /// Defines the contract for IRoutingSinkWithSwitching
    /// </summary>
    public interface IRoutingSinkWithSwitching<TSelector> : IRoutingSink<TSelector>
    {
        void ExecuteSwitch(TSelector inputSelector);
    }*/
}