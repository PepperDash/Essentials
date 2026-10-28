using System.Collections.Generic;
using Newtonsoft.Json;

namespace PepperDash.Essentials.Core.Web;

/// <summary>
/// The current routes of every midpoint and sink, plus the multiview layouts. Built by
/// <see cref="RoutingGraphHelpers.BuildRoutingSnapshot"/>.
/// </summary>
public class RoutingSnapshot
{
    /// <summary>Current routes of each midpoint, keyed by device key.</summary>
    [JsonProperty("midpointRoutes")]
    public Dictionary<string, List<MidpointRouteInfo>> MidpointRoutes { get; set; }

    /// <summary>Current source of each sink input, keyed by device key.</summary>
    [JsonProperty("sinkRoutes")]
    public Dictionary<string, List<SinkRouteInfo>> SinkRoutes { get; set; }

    /// <summary>Current layout of each multiview device, keyed by device key.</summary>
    [JsonProperty("layouts")]
    public Dictionary<string, MultiviewLayoutState> Layouts { get; set; }
}

/// <summary>
/// One input-to-output route on a midpoint.
/// </summary>
public class MidpointRouteInfo
{
    [JsonProperty("inputPortKey")]
    public string InputPortKey { get; set; }

    [JsonProperty("outputPortKey")]
    public string OutputPortKey { get; set; }

    [JsonProperty("signalType")]
    public string SignalType { get; set; }
}

/// <summary>
/// The source routed to one sink input.
/// </summary>
public class SinkRouteInfo
{
    [JsonProperty("inputPortKey")]
    public string InputPortKey { get; set; }

    [JsonProperty("sourceDeviceKey")]
    public string SourceDeviceKey { get; set; }

    [JsonProperty("signalType")]
    public string SignalType { get; set; }
}
