using Newtonsoft.Json;
using PepperDash.Essentials.Core;

namespace PepperDash.Essentials.AppServer
{
    /// <summary>
    /// Represents a SourceSelectMessageContent
    /// </summary>
    public class SourceSelectMessageContent
    {
        /// <summary>
        /// Gets or sets the SourceListItemKey
        /// </summary>
        [JsonProperty("sourceListItemKey")]
        public string SourceListItemKey { get; set; }
        /// <summary>
        /// Gets or sets the SourceListKey
        /// </summary>
        [JsonProperty("sourceListKey")]

        public string SourceListKey { get; set; }
    }

    /// <summary>
    /// Represents a DirectRoute
    /// </summary>
    public class DirectRoute
    {

        /// <summary>
        /// Gets or sets the SourceKey
        /// </summary>
        [JsonProperty("sourceKey")]
        public string SourceKey { get; set; }
        /// <summary>
        /// Gets or sets the DestinationKey
        /// </summary>
        [JsonProperty("destinationKey")]
        public string DestinationKey { get; set; }
        /// <summary>
        /// Gets or sets the SignalType
        /// </summary>
        [JsonProperty("signalType")]
        public eRoutingSignalType SignalType { get; set; }
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="b"></param>
    /// <summary>
    /// Delegate for PressAndHoldAction
    /// </summary>
    public delegate void PressAndHoldAction(bool b);
}