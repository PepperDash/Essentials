using System.Collections.Generic;
using Newtonsoft.Json;

namespace PepperDash.Essentials.Room.Config
{
    public class EssentialsTechRoomConfig
    {
        [JsonProperty("displays")]
        public List<string> Displays;
        
        [JsonProperty("tuners")]
        public List<string> Tuners;

        [JsonProperty("userPin")]
        public string UserPin;

        [JsonProperty("techPin")]
        public string TechPin;

        [JsonProperty("presetsFileName")]
        public string PresetsFileName;

        [JsonProperty("scheduledEvents")]
        public List<ScheduledEventConfig> ScheduledEvents;

        [JsonProperty("isPrimary")] public bool IsPrimary;

        [JsonProperty("isTvPresetsProvider")] public bool IsTvPresetsProvider;

        public EssentialsTechRoomConfig()
        {
            Displays = new List<string>();
            Tuners = new List<string>();
            ScheduledEvents = new List<ScheduledEventConfig>();
        }
    }
}