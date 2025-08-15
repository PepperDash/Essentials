using System;
using Newtonsoft.Json;

namespace PepperDash.Essentials.Core.Config
{
  /// <summary>
  /// Represents the base properties for a streaming device.
  /// </summary>
  public class BaseStreamingDeviceProperties
  {
    /// <summary>
    /// The multicast video address for the streaming device.
    /// </summary>
    [JsonProperty("multicastVideoAddress", NullValueHandling = NullValueHandling.Ignore)]
    public string MulticastVideoAddress { get; set; }

    /// <summary>
    /// The multicast audio address for the streaming device.
    /// </summary>
    [JsonProperty("multicastAudioAddress", NullValueHandling = NullValueHandling.Ignore)]
    public string MulticastAudioAddress { get; set; }

    /// <summary>
    /// The URL for the streaming device's media stream.
    /// </summary>
    [JsonProperty("streamUrl", NullValueHandling = NullValueHandling.Ignore)]
    public string StreamUrl { get; set; }
  }
}
