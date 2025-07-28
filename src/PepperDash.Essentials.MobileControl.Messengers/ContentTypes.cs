using Newtonsoft.Json;
using PepperDash.Essentials.Core.Routing;

namespace PepperDash.Essentials.AppServer
{
    /// <summary>
    /// Represents a SourceSelectMessageContent
    /// </summary>
    public class SourceSelectMessageContent
    {

        [JsonProperty("sourceListItemKey")]
        /// <summary>
        /// Gets or sets the SourceListItemKey
        /// </summary>
        public string SourceListItemKey { get; set; }
        [JsonProperty("sourceListKey")]
        /// <summary>
        /// Gets or sets the SourceListKey
        /// </summary>
        public string SourceListKey { get; set; }
    }

    /// <summary>
    /// Represents a DirectRoute
    /// </summary>
    public class DirectRoute
    {

        [JsonProperty("sourceKey")]
        /// <summary>
        /// Gets or sets the SourceKey
        /// </summary>
        public string SourceKey { get; set; }
        [JsonProperty("destinationKey")]
        /// <summary>
        /// Gets or sets the DestinationKey
        /// </summary>
        public string DestinationKey { get; set; }
        [JsonProperty("signalType")]
        /// <summary>
        /// Gets or sets the SignalType
        /// </summary>
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
