using Newtonsoft.Json;
using System.Collections.Generic;

namespace PepperDash.Essentials.Core.DeviceTypeInterfaces;

/// <summary>
/// Defines a device (e.g. a multiview-capable video decoder) that can build a multiview tile
/// layout at runtime from a set of sources with priority values, rather than only supporting
/// pre-configured/named layouts. Implementing this interface (instead of requiring consumers to
/// reference the hardware-specific plugin type directly) lets other plugins (e.g. a room plugin)
/// drive dynamic, priority-based layouts on the device without taking a compile-time dependency on
/// the specific hardware plugin that implements it.
/// </summary>
public interface IHasDynamicMultiviewLayout
{
    /// <summary>
    /// Computes and applies a multiview layout from the given participant sources (ordered by
    /// priority) and an optional active presentation source.
    /// </summary>
    /// <param name="participantSources">Sources to place in participant tiles, each with a priority (lower value = higher priority).</param>
    /// <param name="presentationSourceKey">Device key for the active presentation source, or null/empty if no presentation is active.</param>
    /// <returns>True if the layout was successfully applied.</returns>
    bool ApplyDynamicLayout(IReadOnlyList<MultiviewParticipantSource> participantSources, string presentationSourceKey);
}

/// <summary>
/// Represents a single source eligible for placement in a dynamic multiview layout, along with
/// its priority. Lower priority values are placed first / given more prominent tiles (i.e. lower
/// number = higher priority).
/// </summary>
public class MultiviewParticipantSource
{
    /// <summary>
    /// The device key of the source to place in a tile.
    /// </summary>
    [JsonProperty("sourceKey")]
    public string SourceKey { get; set; }

    /// <summary>
    /// The priority of this source. Lower values are placed first / given more prominent tiles.
    /// </summary>
    [JsonProperty("priority")]
    public int Priority { get; set; }

    /// <summary>
    /// Parameterless constructor for deserialization.
    /// </summary>
    public MultiviewParticipantSource()
    {
    }

    /// <summary>
    /// Initializes a new instance of the MultiviewParticipantSource class.
    /// </summary>
    public MultiviewParticipantSource(string sourceKey, int priority)
    {
        SourceKey = sourceKey;
        Priority = priority;
    }
}
