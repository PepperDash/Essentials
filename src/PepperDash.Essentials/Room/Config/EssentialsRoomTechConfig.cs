extern alias Full;
using Full::Newtonsoft.Json;

namespace PepperDash.Essentials.Room.Config
{
    public class EssentialsRoomTechConfig
    {
        [JsonProperty("password")]
        public string Password { get; set; }
    }
}