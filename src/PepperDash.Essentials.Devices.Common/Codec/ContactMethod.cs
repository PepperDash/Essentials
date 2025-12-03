using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace PepperDash.Essentials.Devices.Common.Codec
{
  /// <summary>
  /// Represents a ContactMethod
  /// </summary>
  public class ContactMethod
  {
    /// <summary>
    /// Gets or sets the ContactMethodId
    /// </summary>
    [JsonProperty("contactMethodId")]
    public string ContactMethodId { get; set; }

    /// <summary>
    /// Gets or sets the Number
    /// </summary>
    [JsonProperty("number")]
    public string Number { get; set; }

    /// <summary>
    /// Gets or sets the Device
    /// </summary>
    [JsonProperty("device")]
    [JsonConverter(typeof(StringEnumConverter))]
    public eContactMethodDevice Device { get; set; }

    /// <summary>
    /// Gets or sets the CallType
    /// </summary>
    [JsonProperty("callType")]
    [JsonConverter(typeof(StringEnumConverter))]
    public eContactMethodCallType CallType { get; set; }
  }
}