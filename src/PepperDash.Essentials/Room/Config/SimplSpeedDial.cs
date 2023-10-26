extern alias Full;
using Full::Newtonsoft.Json;

namespace PepperDash.Essentials.Room.Config
{
    public class SimplSpeedDial
    {
        [JsonProperty("name")]
        public string Name { get; set; }
        [JsonProperty("number")]
        public string Number { get; set; }
    }
}