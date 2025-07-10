using Newtonsoft.Json;

namespace PepperDash.Essentials.AppServer
{
    /// <summary>
    /// Generic container for simple mobile control message content with a single value
    /// </summary>
    /// <typeparam name="T">The type of the value contained in the message</typeparam>
    public class MobileControlSimpleContent<T>
    {
        /// <summary>
        /// Gets or sets the value of the message content
        /// </summary>
        [JsonProperty("value", NullValueHandling = NullValueHandling.Ignore)]
        public T Value { get; set; }
    }
}
