extern alias Full;
using Full::Newtonsoft.Json;

namespace PepperDash.Essentials.Room.Config
{
    public class EssentialsRoomUiBehaviorConfig
    {
        [JsonProperty("disableActivityButtonsWhileWarmingCooling")]
        public bool DisableActivityButtonsWhileWarmingCooling { get; set; }
    }
}