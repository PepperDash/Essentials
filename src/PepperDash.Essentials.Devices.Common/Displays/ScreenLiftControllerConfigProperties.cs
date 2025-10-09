using System.Collections.Generic;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using PepperDash.Essentials.Core.DeviceTypeInterfaces;

namespace PepperDash.Essentials.Devices.Common.Shades
{
  /// <summary>
  /// Represents a ScreenLiftControllerConfigProperties
  /// </summary>
  public class ScreenLiftControllerConfigProperties
  {

    /// <summary>
    /// Gets or sets the DisplayDeviceKey
    /// </summary>
    [JsonProperty("displayDeviceKey")]
    public string DisplayDeviceKey { get; set; }

    /// <summary>
    /// Gets or sets the Type
    /// </summary>
    [JsonProperty("type")]
    [JsonConverter(typeof(StringEnumConverter))]
    public eScreenLiftControlType Type { get; set; }

    /// <summary>
    /// Gets or sets the Mode
    /// </summary>
    [JsonProperty("mode")]
    [JsonConverter(typeof(StringEnumConverter))]
    public eScreenLiftControlMode Mode { get; set; }

    /// <summary>
    /// Gets or sets the Relays
    /// </summary>
    [JsonProperty("relays")]
    public Dictionary<string, ScreenLiftRelaysConfig> Relays { get; set; }

  }
}