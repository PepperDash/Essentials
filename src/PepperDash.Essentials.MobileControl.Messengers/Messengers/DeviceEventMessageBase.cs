using Newtonsoft.Json;

namespace PepperDash.Essentials.AppServer.Messengers
{
  /// <summary>
  /// Base class for event messages that include the type of message and an event type
  /// </summary>
  public abstract class DeviceEventMessageBase : DeviceMessageBase
  {
    /// <summary>
    /// The event type
    /// </summary>
    [JsonProperty("eventType")]
    public string EventType { get; set; }
  }

}