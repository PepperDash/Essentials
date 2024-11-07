using System.Collections.Generic;

namespace PepperDash.Essentials.Core.Routing
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