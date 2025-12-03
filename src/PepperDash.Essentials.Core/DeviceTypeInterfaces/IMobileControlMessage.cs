using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace PepperDash.Essentials.Core.DeviceTypeInterfaces
{
  public interface IMobileControlMessage
  {
    [JsonProperty("type")]
    string Type { get; }

    [JsonProperty("clientId", NullValueHandling = NullValueHandling.Ignore)]
    string ClientId { get; }

    [JsonProperty("content", NullValueHandling = NullValueHandling.Ignore)]
    JToken Content { get; }

  }
}