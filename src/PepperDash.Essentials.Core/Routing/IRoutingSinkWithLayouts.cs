using System.Collections.Generic;
using PepperDash.Core;
using PepperDash.Essentials.Core.Routing;

namespace PepperDash.Essentials.Core;


/// <summary>
/// Defines a routing sink (endpoint) device that supports layouts with multiple tile sinks.
/// Implements IRoutingSource so the windowed output of the layout can be routed to other devices.
/// </summary>
public interface IRoutingSinkWithLayouts : IRoutingSource
{
    /// <summary>
    /// Gets the collection of window tile sinks for this routing sink with layouts.
    /// Each tile sink represents a single window/tile in the layout and can be routed independently.
    /// </summary>
    Dictionary<int, IRoutingSinkWithFeedback> WindowTileSinks { get; }
}