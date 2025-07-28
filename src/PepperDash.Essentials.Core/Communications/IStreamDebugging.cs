using Newtonsoft.Json;

namespace PepperDash.Essentials.Core.Communications
{
    /// <summary>
    /// Represents a device with stream debugging capablities
    /// </summary>
    public interface IStreamDebugging
    {
        /// <summary>
        /// Object to enable stream debugging
        /// </summary>
        [JsonProperty("streamDebugging")]
        CommunicationStreamDebugging StreamDebugging { get; }
    }
}