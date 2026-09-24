using Newtonsoft.Json;

namespace PepperDash.Essentials.Core.Web.RequestHandlers;

/// <summary>
/// One address a debug or routing-feedback websocket can be reached on. The session handlers return
/// every reachable option so a client can pick the network it is actually on, rather than the
/// processor having to guess.
/// </summary>
public class WebsocketEndpointOption
{
    /// <summary>
    /// Stable identifier the client passes back in the <c>network</c> query string to select this
    /// endpoint: <c>lan</c>, <c>cslan</c> or <c>current</c>.
    /// </summary>
    [JsonProperty("id")]
    public string Id { get; set; }

    /// <summary>
    /// Human-readable name for this network, suitable for a picker in the debug app.
    /// </summary>
    [JsonProperty("label")]
    public string Label { get; set; }

    /// <summary>
    /// The host name or IP address the websocket is reached on for this network.
    /// </summary>
    [JsonProperty("host")]
    public string Host { get; set; }

    /// <summary>
    /// The full <c>wss://</c> URL for this network.
    /// </summary>
    [JsonProperty("url")]
    public string Url { get; set; }
}
