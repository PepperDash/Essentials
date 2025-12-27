using Newtonsoft.Json;

namespace PepperDash.Essentials.Devices.Common.Shades
{
  /// <summary>
  /// Represents a ScreenLiftRelaysConfig
  /// </summary>
  public class ScreenLiftRelaysConfig
  {
    /// <summary>
    /// Gets or sets the DeviceKey
    /// </summary>
    [JsonProperty("deviceKey")]
    public string DeviceKey { get; set; }

    /// <summary>
    /// Gets or sets the PulseTimeInMs
    /// </summary>
    [JsonProperty("pulseTimeInMs")]
    public int PulseTimeInMs { get; set; }

    /// <summary>
    /// Gets or sets the RaiseTimeInMs - time in milliseconds for the raise movement to complete
    /// </summary>
    [JsonProperty("raiseTimeInMs")]
    public int RaiseTimeInMs { get; set; }

    /// <summary>
    /// Gets or sets the LowerTimeInMs - time in milliseconds for the lower movement to complete
    /// </summary>
    [JsonProperty("lowerTimeInMs")]
    public int LowerTimeInMs { get; set; }
  }
}