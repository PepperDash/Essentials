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
    /// Gets or sets the MoveTimeInMs - time in milliseconds for the movement to complete
    /// </summary>
    [JsonProperty("moveTimeInMs")]
    public int MoveTimeInMs { get; set; }
  }
}