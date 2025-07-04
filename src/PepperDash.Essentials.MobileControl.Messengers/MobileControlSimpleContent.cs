using Newtonsoft.Json;

namespace PepperDash.Essentials.AppServer;


/// <summary>
/// Represents a MobileControlSimpleContent
/// </summary>
public class MobileControlSimpleContent<T>
{
    /// <summary>
    /// Gets or sets the Value
    /// </summary>
    [JsonProperty("value", NullValueHandling = NullValueHandling.Ignore)]

    public T Value { get; set; }
}

