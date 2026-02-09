using System;

using PepperDash.Core;


namespace PepperDash.Essentials.Core
{
    /// <summary>
    /// Defines the contract for IRoutingFeedback
    /// </summary>
    public interface IRoutingFeedback : IKeyName
    {
        /// <summary>
        /// Event raised when a numeric switch changes
        /// </summary>
        event EventHandler<RoutingNumericEventArgs> NumericSwitchChange;
        //void OnSwitchChange(RoutingNumericEventArgs e);
    }
}