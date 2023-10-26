extern alias Full;
using Full::Newtonsoft.Json;

namespace PepperDash.Essentials.Room.Config
{
    public class EssentialsAvRoomPropertiesConfig : EssentialsRoomPropertiesConfig
    {
        [JsonProperty("defaultAudioKey")]
        public string DefaultAudioKey { get; set; }
        [JsonProperty("sourceListKey")]
        public string SourceListKey { get; set; }
        [JsonProperty("destinationListKey")]
        public string DestinationListKey { get; set; }
        [JsonProperty("defaultSourceItem")]
        public string DefaultSourceItem { get; set; }
        /// <summary>
        /// Indicates if the room supports advanced sharing
        /// </summary>
        [JsonProperty("supportsAdvancedSharing")]
        public bool SupportsAdvancedSharing { get; set; }
        /// <summary>
        /// Indicates if non-tech users can change the share mode
        /// </summary>
        [JsonProperty("userCanChangeShareMode")]
        public bool UserCanChangeShareMode { get; set; }
    }
}