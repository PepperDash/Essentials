using System;

namespace PepperDash.Essentials.Core
{
    /// <summary>
    /// Endpoint device like a display, that selects inputs
    /// </summary>
    public interface IRoutingSinkWithSwitching : IRoutingSink
	{		
		void ExecuteSwitch(object inputSelector);
	}

/*    /// <summary>
    /// Endpoint device like a display, that selects inputs
    /// </summary>
    public interface IRoutingSinkWithSwitching<TSelector> : IRoutingSink<TSelector>
    {
        void ExecuteSwitch(TSelector inputSelector);
    }*/
}