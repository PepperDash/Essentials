using System;


namespace PepperDash.Essentials.Core.Routing
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