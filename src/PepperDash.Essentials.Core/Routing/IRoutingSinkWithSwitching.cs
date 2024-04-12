using System;

namespace PepperDash.Essentials.Core
{
    public delegate void InputChangedEventHandler(IRoutingSinkWithSwitching destination, RoutingInputPort currentPort);

    /// <summary>
    /// Endpoint device like a display, that selects inputs
    /// </summary>
    public interface IRoutingSinkWithSwitching : IRoutingSink
	{		
		void ExecuteSwitch(object inputSelector);

        event InputChangedEventHandler InputChanged;
    }

/*    /// <summary>
    /// Endpoint device like a display, that selects inputs
    /// </summary>
    public interface IRoutingSinkWithSwitching<TSelector> : IRoutingSink<TSelector>
    {
        void ExecuteSwitch(TSelector inputSelector);
    }*/
}