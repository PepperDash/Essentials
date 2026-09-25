using System.Collections.Generic;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using PepperDash.Essentials.Core;

namespace PepperDash.Essentials.Devices.Common.Routing;

/// <summary>
/// Configuration properties for a <see cref="MockRoutingMidpoint"/> device.
/// </summary>
public class MockRoutingMidpointPropertiesConfig
{
    /// <summary>
    /// The input ports to build on the mock midpoint.
    /// </summary>
    [JsonProperty("inputPorts")]
    public List<MockRoutingMidpointPortConfig> InputPorts { get; set; } = new List<MockRoutingMidpointPortConfig>();

    /// <summary>
    /// The output ports to build on the mock midpoint.
    /// </summary>
    [JsonProperty("outputPorts")]
    public List<MockRoutingMidpointPortConfig> OutputPorts { get; set; } = new List<MockRoutingMidpointPortConfig>();
}

/// <summary>
/// Configuration for a single input or output port on a <see cref="MockRoutingMidpoint"/> device.
/// </summary>
public class MockRoutingMidpointPortConfig
{
    /// <summary>
    /// The name of the port. Used as both the port's key and its selector value, so it must be unique
    /// within the collection (input ports or output ports) it is configured under.
    /// </summary>
    [JsonProperty("name")]
    public string Name { get; set; }

    /// <summary>
    /// The routing signal type supported by this port (e.g. "Audio", "Video", "AudioVideo", "Usb").
    /// Defaults to AudioVideo if not specified.
    /// </summary>
    [JsonProperty("signalType")]
    [JsonConverter(typeof(StringEnumConverter))]
    public eRoutingSignalType SignalType { get; set; } = eRoutingSignalType.AudioVideo;

    /// <summary>
    /// The physical connection type of this port (e.g. "Hdmi", "Dm", "DisplayPort"). Defaults to Hdmi if
    /// not specified.
    /// </summary>
    [JsonProperty("portType")]
    [JsonConverter(typeof(StringEnumConverter))]
    public eRoutingPortConnectionType PortType { get; set; } = eRoutingPortConnectionType.Hdmi;

    /// <summary>
    /// Optional friendly display name for this port (e.g. "Rm A Codec"), surfaced via
    /// <see cref="IHasNamedRoutingSlots"/> for named-routing-slot UI. Defaults to <see cref="Name"/> if
    /// not specified.
    /// </summary>
    [JsonProperty("label", NullValueHandling = NullValueHandling.Ignore)]
    public string Label { get; set; }
}
