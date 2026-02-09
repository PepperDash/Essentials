using System;
using System.Collections.Generic;

namespace PepperDash.Essentials.Core
{
    /// <summary>
    /// Defines a midpoint device as have internal routing.  Any devices in the middle of the
    /// signal chain, that do switching, must implement this for routing to work otherwise
    /// the routing algorithm will treat the IRoutingInputsOutputs device as a passthrough
    /// device.
    /// </summary>
    public interface IRouting : IRoutingInputsOutputs
	{
        /// <summary>
        /// Executes a switch on the device
        /// </summary>
        /// <param name="inputSelector">input selector</param>
        /// <param name="outputSelector">output selector</param>
        /// <param name="signalType">type of signal</param>
		void ExecuteSwitch(object inputSelector, object outputSelector, eRoutingSignalType signalType);        
    }    

    /*public interface IRouting<TInputSelector,TOutputSelector> : IRoutingInputsOutputs
    {
        void ExecuteSwitch(TInputSelector inputSelector, TOutputSelector outputSelector, eRoutingSignalType signalType);
    }*/
}