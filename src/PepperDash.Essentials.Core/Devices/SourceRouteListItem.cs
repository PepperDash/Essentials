extern alias Full;
using Full::Newtonsoft.Json;

namespace PepperDash.Essentials.Core
{
    public class SourceRouteListItem
    {
        [JsonProperty("sourceKey")]
        public string SourceKey { get; set; }

        [JsonProperty("destinationKey")]
        public string DestinationKey { get; set; }

        [JsonProperty("type")]
        public eRoutingSignalType Type { get; set; }
    }
}