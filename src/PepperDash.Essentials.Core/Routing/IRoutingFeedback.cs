using System;

using PepperDash.Core;


namespace PepperDash.Essentials.Core
{
    /// <summary>
    /// Defines the contract for IRoutingFeedback
    /// </summary>
    public interface IRoutingFeedback : IKeyName
    {
        event EventHandler<RoutingNumericEventArgs> NumericSwitchChange;
        //void OnSwitchChange(RoutingNumericEventArgs e);
    }
}