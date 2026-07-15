using System.Collections.Generic;
using System.Linq;
using PepperDash.Core;

namespace PepperDash.Essentials.Core.Web;

/// <summary>
/// Shared helpers for building routing-graph data for consumers of the routing dev tools
/// (<see cref="RequestHandlers.GetRoutingDevicesAndTieLinesHandler"/> and
/// <see cref="RoutingFeedbackWebsocket"/>).
/// </summary>
/// <remarks>
/// Devices that implement <see cref="IRoutingSinkWithLayouts"/> (e.g. a multiview decoder) expose a
/// set of per-tile child sink devices (<see cref="IRoutingSinkWithLayouts.WindowTileSinks"/>), each of
/// which is independently registered with <see cref="DeviceManager"/> and independently routable.
/// Rendered naively, each tile shows up as its own top-level node in the routing graph, which is
/// confusing - a multiview decoder with N tiles doesn't look like N separate devices to a user.
/// These helpers let routing-graph consumers instead: skip tile-sink children when enumerating
/// top-level devices, and represent each tile as a distinctly-keyed input "edge target" on the
/// parent's own node (see <see cref="QualifyTilePortKey"/>), remapping any tie line / active route
/// that targets a tile so it points at the parent device instead.
/// </remarks>
public static class RoutingGraphHelpers
{
    /// <summary>
    /// Describes where a tile-sink child device (<see cref="IRoutingSinkWithLayouts.WindowTileSinks"/>)
    /// belongs, for remapping it back to its parent's node in routing-graph output.
    /// </summary>
    public readonly struct TileChildInfo
    {
        /// <summary>The parent device this tile belongs to.</summary>
        public IRoutingSinkWithLayouts Parent { get; }

        /// <summary>The tile's 1-based window number within the parent's layout.</summary>
        public int TileNumber { get; }

        /// <summary>Initializes a new instance of the <see cref="TileChildInfo"/> struct.</summary>
        public TileChildInfo(IRoutingSinkWithLayouts parent, int tileNumber)
        {
            Parent = parent;
            TileNumber = tileNumber;
        }
    }

    /// <summary>
    /// Builds a map of every tile-sink child device key (across all <see cref="IRoutingSinkWithLayouts"/>
    /// devices currently in <see cref="DeviceManager"/>) to its parent device and tile number.
    /// </summary>
    public static Dictionary<string, TileChildInfo> BuildTileChildMap()
    {
        var map = new Dictionary<string, TileChildInfo>();

        foreach (var parent in DeviceManager.AllDevices.OfType<IRoutingSinkWithLayouts>())
        {
            foreach (var kvp in parent.WindowTileSinks)
            {
                if (kvp.Value is not IKeyed tile || string.IsNullOrEmpty(tile.Key))
                    continue;

                map[tile.Key] = new TileChildInfo(parent, kvp.Key);
            }
        }

        return map;
    }

    /// <summary>
    /// Builds a graph-unique port key for a tile's port, so multiple tiles synthesized onto the same
    /// parent node don't collide (every tile's own <c>InputPorts</c> collection typically contains a
    /// single, identically-keyed port, e.g. "tileInput").
    /// </summary>
    public static string QualifyTilePortKey(int tileNumber, string portKey) => $"tile{tileNumber}:{portKey}";

    /// <summary>
    /// Gets the device key of whatever source is currently feeding a sink's Video signal (falling back
    /// to Audio), read directly from its own <see cref="ICurrentSources"/> bookkeeping (part of
    /// <see cref="IRoutingSinkWithFeedback"/>). This is authoritative regardless of whether the route
    /// was made via a tie line (<c>ReleaseAndMakeRoute</c>) or a device-specific bulk API (e.g.
    /// <c>IHasDynamicMultiviewLayout.ApplyDynamicLayout</c>) that never touches
    /// <c>TieLineCollection</c>/<c>RouteDescriptorCollection</c> at all.
    /// </summary>
    public static string GetCurrentSourceKey(IRoutingSinkWithFeedback device)
    {
        if (device.CurrentSourceKeys.TryGetValue(eRoutingSignalType.Video, out var videoKey) && !string.IsNullOrEmpty(videoKey))
            return videoKey;

        return device.CurrentSourceKeys.TryGetValue(eRoutingSignalType.Audio, out var audioKey) ? audioKey : null;
    }
}
