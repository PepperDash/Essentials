using System.Collections.Generic;
using Newtonsoft.Json;

namespace PepperDash.Essentials.Core;

/// <summary>
/// Describes the current shape of a multiview canvas and the position, size, stacking order and
/// routed source of every visible tile within it. Fully product-agnostic (no dependency on any
/// particular decoder/hardware type) and JSON-serializable so it can be sent as-is over the routing
/// feedback WebSocket (<see cref="Web.RoutingFeedbackWebsocket"/>) and the routing devices/tie-lines
/// HTTP snapshot (<see cref="Web.RequestHandlers.GetRoutingDevicesAndTieLinesHandler"/>), for
/// rendering a visual mock-up of what is actually displayed on the fed monitor.
/// </summary>
public class MultiviewLayoutState
{
    /// <summary>
    /// Width, in pixels, of the canvas that <see cref="Tiles"/> positions/sizes are expressed
    /// against (typically the decoder's current output resolution width).
    /// </summary>
    [JsonProperty("canvasWidth")]
    public int CanvasWidth { get; set; }

    /// <summary>
    /// Height, in pixels, of the canvas that <see cref="Tiles"/> positions/sizes are expressed
    /// against (typically the decoder's current output resolution height).
    /// </summary>
    [JsonProperty("canvasHeight")]
    public int CanvasHeight { get; set; }

    /// <summary>
    /// Position, size, stacking order and routed source for every visible tile in the layout.
    /// </summary>
    [JsonProperty("tiles")]
    public List<MultiviewTileState> Tiles { get; set; } = new List<MultiviewTileState>();
}

/// <summary>
/// Describes a single tile/window within a <see cref="MultiviewLayoutState"/>.
/// </summary>
public class MultiviewTileState
{
    /// <summary>
    /// 1-based tile/window number, matching the key in
    /// <see cref="IRoutingSinkWithLayouts.WindowTileSinks"/>.
    /// </summary>
    [JsonProperty("tileNumber")]
    public int TileNumber { get; set; }

    /// <summary>
    /// Device key of this tile's <see cref="IRoutingSinkWithFeedback"/> child sink, so a client can
    /// cross-reference existing routing-feedback data (e.g. sink current-source state) for this tile
    /// without re-deriving it.
    /// </summary>
    [JsonProperty("tileSinkKey")]
    public string TileSinkKey { get; set; }

    /// <summary>Left edge of the tile, in pixels, within the canvas.</summary>
    [JsonProperty("x")]
    public int X { get; set; }

    /// <summary>Top edge of the tile, in pixels, within the canvas.</summary>
    [JsonProperty("y")]
    public int Y { get; set; }

    /// <summary>Width of the tile, in pixels.</summary>
    [JsonProperty("width")]
    public int Width { get; set; }

    /// <summary>Height of the tile, in pixels.</summary>
    [JsonProperty("height")]
    public int Height { get; set; }

    /// <summary>
    /// Stacking order for overlapping tiles (e.g. picture-in-picture/overlay layouts). Tiles with
    /// higher values are drawn on top of tiles with lower values.
    /// </summary>
    [JsonProperty("zOrder")]
    public int ZOrder { get; set; }

    /// <summary>
    /// Device key of the source currently routed to this tile, or null if the tile is empty.
    /// </summary>
    [JsonProperty("sourceDeviceKey")]
    public string SourceDeviceKey { get; set; }
}
