using Newtonsoft.Json;

namespace PepperDash.Essentials.Core.Rooms.Config
{
    public class EssentialsHuddleVtc1PropertiesConfig : EssentialsHuddleRoomPropertiesConfig
    {
        [JsonProperty("videoCodecKey")]
        public string VideoCodecKey { get; set; }
        [JsonProperty("audioCodecKey")]
        public string AudioCodecKey { get; set; }
    }
}