using System.Collections.Generic;
using System;

namespace PepperDash.Essentials.Core
{
    /// <summary>
    /// Delegate for handling route change events on devices implementing <see cref="IRoutingWithFeedback"/>.
    /// </summary>
    /// <param name="midpoint">The routing device where the change occurred.</param>
    /// <param name="newRoute">A descriptor of the new route that was established.</param>
    public delegate void RouteChangedEventHandler(IRoutingWithFeedback midpoint, RouteSwitchDescriptor newRoute);
    /// <summary>
    /// Defines a routing device (<see cref="IRouting"/>) that provides feedback about its current routes.
    /// </summary>
    public interface IRoutingWithFeedback : IRouting
    {
        /// <summary>
        /// Gets a list describing the currently active routes on this device.
        /// </summary>
        List<RouteSwitchDescriptor> CurrentRoutes { get; }

        /// <summary>
        /// Event triggered when a route changes on this device.
        /// </summary>
        event RouteChangedEventHandler RouteChanged;
    }
}