using System.Collections.Generic;
using System;

namespace PepperDash.Essentials.Core
{
    public delegate void RouteChangedEventHandler(IRoutingWithFeedback midpoint, RouteSwitchDescriptor newRoute);
    /// <summary>
    /// Defines an IRouting with a feedback event
    /// </summary>
    public interface IRoutingWithFeedback : IRouting
    {
        List<RouteSwitchDescriptor> CurrentRoutes { get; }

        event RouteChangedEventHandler RouteChanged;
    }
}