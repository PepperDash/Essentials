using Newtonsoft.Json;

namespace PepperDash.Essentials.AppServer.Messengers
{
  /// <summary>
  /// Base class for device messages that include the type of message
  /// </summary>
  public abstract class DeviceMessageBase
  {
    /// <summary>
    /// The device key
    /// </summary>
    [JsonProperty("key")]
    /// <summary>
    /// Gets or sets the Key
    /// </summary>
    public string Key { get; set; }

    /// <summary>
    /// The device name
    /// </summary>
    [JsonProperty("name")]
    public string Name { get; set; }

    /// <summary>
    /// The type of the message class
    /// </summary>
    [JsonProperty("messageType")]
    public string MessageType => GetType().Name;

    /// <summary>
    /// Gets or sets the MessageBasePath
    /// </summary>
    [JsonProperty("messageBasePath")]

    public string MessageBasePath { get; set; }
  }

}