using Newtonsoft.Json;
using PepperDash.Essentials.Core;

namespace PepperDash.Essentials.AppServer
{
    /// <summary>
    /// Represents the content of a source selection message
    /// </summary>
    public class SourceSelectMessageContent
    {
        /// <summary>
        /// Gets or sets the key of the source list item to select
        /// </summary>
        [JsonProperty("sourceListItemKey")]
        public string SourceListItemKey { get; set; }

        /// <summary>
        /// Gets or sets the key of the source list containing the item
        /// </summary>
        [JsonProperty("sourceListKey")]
        public string SourceListKey { get; set; }
    }

    /// <summary>
    /// Represents a direct routing operation between a source and destination
    /// </summary>
    public class DirectRoute
    {
        /// <summary>
        /// Gets or sets the key of the source device
        /// </summary>
        [JsonProperty("sourceKey")]
        public string SourceKey { get; set; }

        /// <summary>
        /// Gets or sets the key of the destination device
        /// </summary>
        [JsonProperty("destinationKey")]
        public string DestinationKey { get; set; }

        /// <summary>
        /// Gets or sets the type of routing signal (Audio, Video, etc.)
        /// </summary>
        [JsonProperty("signalType")]
        public eRoutingSignalType SignalType { get; set; }
    }

    /// <summary>
    /// Delegate for press and hold actions with boolean state parameter
    /// </summary>
    /// <param name="b">The state of the press and hold action</param>
    public delegate void PressAndHoldAction(bool b);
}
