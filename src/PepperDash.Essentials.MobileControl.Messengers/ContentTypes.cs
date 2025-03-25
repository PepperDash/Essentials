using Newtonsoft.Json;
using PepperDash.Essentials.Core;

namespace PepperDash.Essentials.AppServer
{
    public class SourceSelectMessageContent
    {
        
        [JsonProperty("sourceListItemKey")]
        public string SourceListItemKey { get; set; }
        [JsonProperty("sourceListKey")]
        public string SourceListKey { get; set; }
    }

    public class DirectRoute
    {

        [JsonProperty("sourceKey")]
        public string SourceKey { get; set; }
        [JsonProperty("destinationKey")]
        public string DestinationKey { get; set; }
        [JsonProperty("signalType")]
        public eRoutingSignalType SignalType { get; set; }
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="b"></param>
    public delegate void PressAndHoldAction(bool b);
}
