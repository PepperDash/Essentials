using Newtonsoft.Json;

namespace PepperDash.Essentials.AppServer
{
    /// <summary>
    /// Represents a MobileControlSimpleContent
    /// </summary>
    public class MobileControlSimpleContent<T>
    {
        [JsonProperty("value", NullValueHandling = NullValueHandling.Ignore)]
        /// <summary>
        /// Gets or sets the Value
        /// </summary>
        public T Value { get; set; }
    }
}
