extern alias Full;
using Full::Newtonsoft.Json;

namespace PepperDash.Essentials.Room.Config
{
    public class EssentialsConferenceRoomPropertiesConfig : EssentialsAvRoomPropertiesConfig
    {
        [JsonProperty("videoCodecKey")]
        public string VideoCodecKey { get; set; }
        [JsonProperty("audioCodecKey")]
        public string AudioCodecKey { get; set; }
    }
}