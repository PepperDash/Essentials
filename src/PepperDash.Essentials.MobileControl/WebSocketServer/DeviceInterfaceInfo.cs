using System.Collections.Generic;
using Newtonsoft.Json;
using PepperDash.Core;


/// <summary>
/// Represents info about a device including supproted interfaces
/// </summary>
public class DeviceInterfaceInfo : IKeyName
{
  /// <summary>
  /// Gets or sets the Key
  /// </summary>
  [JsonProperty("key")]
  public string Key { get; set; }

  /// <summary>
  /// Gets or sets the Name
  /// </summary>
  [JsonProperty("name")]
  public string Name { get; set; }

  /// <summary>
  /// Gets or sets the Interfaces
  /// </summary>
  [JsonProperty("interfaces")]
  public List<string> Interfaces { get; set; }
}