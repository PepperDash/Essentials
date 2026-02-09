using System;

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
        /// <summary>
        /// Executes a switch on the device
        /// </summary>
        /// <param name="inputSelector">input selector</param>
		void ExecuteSwitch(object inputSelector);        
    }

    /// <summary>
    /// Defines the contract for IRoutingSinkWithSwitchingWithInputPort
    /// </summary>
    public interface IRoutingSinkWithSwitchingWithInputPort:IRoutingSinkWithSwitching, IRoutingSinkWithInputPort
    {
        /// <summary>
        /// Event raised when the input changes
        /// </summary>
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