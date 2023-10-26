extern alias Full;
using Full::Newtonsoft.Json;

namespace PepperDash.Essentials.Room.Config
{
    public class EssentialsRoomMicrophonePrivacyConfig
    {
        [JsonProperty("deviceKey")]
        public string DeviceKey { get; set; }

        [JsonProperty("behaviour")]
        public string Behaviour { get; set; }
    }
}