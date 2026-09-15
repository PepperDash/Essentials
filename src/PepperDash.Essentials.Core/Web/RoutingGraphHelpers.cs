using System;
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
    /// Parses a tile-qualified port key of the form produced by <see cref="QualifyTilePortKey"/>,
    /// e.g. "tile2:tileInput". Returns false for any key that is not tile-qualified.
    /// </summary>
    /// <remarks>
    /// Deliberately hand-rolled rather than using a regex: a real port key may legitimately contain
    /// a colon, so the "tile" prefix and the all-digit tile number must both be verified before the
    /// remainder is treated as a child port key.
    /// </remarks>
    public static bool TryParseTilePortKey(string portKey, out int tileNumber, out string childPortKey)
    {
        tileNumber = 0;
        childPortKey = null;

        const string prefix = "tile";
        if (string.IsNullOrEmpty(portKey) || !portKey.StartsWith(prefix, StringComparison.OrdinalIgnoreCase))
            return false;

        var colonIndex = portKey.IndexOf(':');
        if (colonIndex <= prefix.Length || colonIndex == portKey.Length - 1)
            return false;

        var numberPart = portKey.Substring(prefix.Length, colonIndex - prefix.Length);
        if (!int.TryParse(numberPart, out tileNumber))
            return false;

        childPortKey = portKey.Substring(colonIndex + 1);
        return true;
    }

    /// <summary>
    /// Resolves a graph-facing (device key, input port key) pair to the real routable
    /// <see cref="IRoutingInputs"/> device and its real port key, de-qualifying multiview tile ports
    /// back to their child tile sink.
    /// </summary>
    /// <remarks>
    /// A tile-addressed route MUST go through this. <see cref="IRoutingSinkWithLayouts"/> derives from
    /// <see cref="IRoutingSource"/>, not <see cref="IRoutingInputs"/> - the parent device has no input
    /// ports of its own, so routing directly to it is impossible. Its tiles are synthesized onto its
    /// node purely for display (see <see cref="QualifyTilePortKey"/>).
    /// </remarks>
    /// <param name="deviceKey">Graph-facing device key, as reported by the routing graph API.</param>
    /// <param name="inputPortKey">Graph-facing input port key; may be "tile{N}:{portKey}". Optional.</param>
    /// <param name="destination">The resolved routable destination device.</param>
    /// <param name="resolvedInputPortKey">The real port key on <paramref name="destination"/>.</param>
    /// <param name="errorCode">Stable error code when resolution fails.</param>
    /// <param name="errorMessage">Human-readable failure detail.</param>
    /// <returns>True when the target resolved.</returns>
    public static bool TryResolveRoutingInputTarget(
        string deviceKey,
        string inputPortKey,
        out IRoutingInputs destination,
        out string resolvedInputPortKey,
        out string errorCode,
        out string errorMessage)
    {
        destination = null;
        resolvedInputPortKey = inputPortKey;
        errorCode = null;
        errorMessage = null;

        var device = DeviceManager.GetDeviceForKey(deviceKey);
        if (device == null)
        {
            errorCode = "deviceNotFound";
            errorMessage = $"No device with key '{deviceKey}'.";
            return false;
        }

        // Checked before the plain IRoutingInputs case: a "tile{N}:" key is unambiguous intent, even
        // for a device that somehow implements both.
        if (TryParseTilePortKey(inputPortKey, out var tileNumber, out var childPortKey)
            && device is IRoutingSinkWithLayouts layoutParent)
        {
            if (!layoutParent.WindowTileSinks.TryGetValue(tileNumber, out var tileSink)
                || tileSink is not IRoutingInputs tileInputs)
            {
                errorCode = "tileNotFound";
                errorMessage = $"Device '{deviceKey}' has no tile {tileNumber}.";
                return false;
            }

            destination = tileInputs;
            resolvedInputPortKey = childPortKey;
            return true;
        }

        if (device is IRoutingInputs inputs)
        {
            destination = inputs;
            return true;
        }

        errorCode = "deviceNotRoutable";
        errorMessage = $"Device '{deviceKey}' does not implement IRoutingInputs.";
        return false;
    }

    /// <summary>
    /// Maps a real (device key, port key) pair back to the graph-facing pair, applying tile
    /// qualification, so command responses line up with what the routing graph renders.
    /// </summary>
    public static (string DeviceKey, string PortKey) ToGraphKeys(
        Dictionary<string, TileChildInfo> tileChildren,
        string deviceKey,
        string portKey)
    {
        if (tileChildren != null && deviceKey != null && tileChildren.TryGetValue(deviceKey, out var tile))
            return (tile.Parent.Key, QualifyTilePortKey(tile.TileNumber, portKey));

        return (deviceKey, portKey);
    }

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

    /// <summary>
    /// Builds a snapshot of the current <see cref="MultiviewLayoutState"/> for every device in
    /// <see cref="DeviceManager"/> that implements <see cref="IRoutingSinkWithLayoutState"/>, keyed
    /// by device key. Devices with no currently active layout (<c>CurrentLayout == null</c>) are
    /// omitted. Shared by <see cref="RequestHandlers.GetRoutingDevicesAndTieLinesHandler"/> (initial
    /// HTTP snapshot) and <see cref="RoutingFeedbackWebsocket"/> (WebSocket snapshot on connect).
    /// </summary>
    public static Dictionary<string, MultiviewLayoutState> BuildMultiviewLayoutSnapshot()
    {
        var result = new Dictionary<string, MultiviewLayoutState>();

        foreach (var device in DeviceManager.AllDevices.OfType<IRoutingSinkWithLayoutState>())
        {
            var layout = device.CurrentLayout;
            if (layout == null)
                continue;

            result[device.Key] = layout;
        }

        return result;
    }
}
