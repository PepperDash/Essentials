using System;

using PepperDash.Core;


namespace PepperDash.Essentials.Core;

/// <summary>
/// Defines an event structure for reporting output route data
/// </summary>
public interface IRoutingFeedback : IKeyName
{
    event EventHandler<RoutingNumericEventArgs> NumericSwitchChange;
    //void OnSwitchChange(RoutingNumericEventArgs e);
}