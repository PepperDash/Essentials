using System.Collections.Generic;
using System;

namespace PepperDash.Essentials.Core;

/// <summary>
/// Delegate for RouteChangedEventHandler.
/// </summary>
/// <param name="midpoint">The routing device where the change occurred.</param>
/// <param name="newRoute">A descriptor of the new route that was established.</param>
public delegate void RouteChangedEventHandler(IRoutingMidpointWithFeedback midpoint, RouteSwitchDescriptor newRoute);

/// <summary>
/// Defines a midpoint device that performs active switching and provides feedback about its current routes.
/// Combines the capabilities of the former IRouting, IRoutingWithFeedback, and IRoutingWithClear interfaces.
/// </summary>
public interface IRoutingMidpointWithFeedback : IRoutingMidpoint
{
    /// <summary>
    /// Executes a switch on the device.
    /// </summary>
    /// <param name="inputSelector">Input selector.</param>
    /// <param name="outputSelector">Output selector.</param>
    /// <param name="signalType">Type of signal.</param>
    void ExecuteSwitch(object inputSelector, object outputSelector, eRoutingSignalType signalType);

    /// <summary>
    /// Clears a route to an output.
    /// </summary>
    /// <param name="outputSelector">Output to clear.</param>
    /// <param name="signalType">Signal type to clear.</param>
    void ClearRoute(object outputSelector, eRoutingSignalType signalType);

    /// <summary>
    /// Gets a list describing the currently active routes on this device.
    /// </summary>
    List<RouteSwitchDescriptor> CurrentRoutes { get; }

    /// <summary>
    /// Event triggered when a route changes on this device.
    /// </summary>
    event RouteChangedEventHandler RouteChanged;
}
