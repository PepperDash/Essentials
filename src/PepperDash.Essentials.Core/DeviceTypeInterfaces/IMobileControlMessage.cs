using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace PepperDash.Essentials.Core.DeviceTypeInterfaces
{
  /// <summary>
  /// Defines the contract for IMobileControlMessage
  /// </summary>
  public interface IMobileControlMessage
  {
    /// <summary>
    /// The type of mobile control message
    /// </summary>
    [JsonProperty("type")]
    string Type { get; }

    /// <summary>
    /// The client ID for the mobile control message
    /// </summary>
    [JsonProperty("clientId", NullValueHandling = NullValueHandling.Ignore)]
    string ClientId { get; }

    /// <summary>
    /// The content of the mobile control message
    /// </summary>
    [JsonProperty("content", NullValueHandling = NullValueHandling.Ignore)]
    JToken Content { get; }

  }
}