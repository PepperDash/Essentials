extern alias Full;
using Full::Newtonsoft.Json;

namespace PepperDash.Essentials.Core
{
    public class RoomOnToDefaultSourceWhenOccupiedConfig
    {
        [JsonProperty("roomKey")]
        public string RoomKey { get; set; }

        [JsonProperty("enableRoomOnWhenOccupied")]
        public bool EnableRoomOnWhenOccupied { get; set; }

        [JsonProperty("occupancyStartTime")]
        public string OccupancyStartTime { get; set; }

        [JsonProperty("occupancyEndTime")]
        public string OccupancyEndTime { get; set; }

        [JsonProperty("enableSunday")]
        public bool EnableSunday { get; set; }

        [JsonProperty("enableMonday")]
        public bool EnableMonday { get; set; }

        [JsonProperty("enableTuesday")]
        public bool EnableTuesday { get; set; }

        [JsonProperty("enableWednesday")]
        public bool EnableWednesday { get; set; }

        [JsonProperty("enableThursday")]
        public bool EnableThursday { get; set; }

        [JsonProperty("enableFriday")]
        public bool EnableFriday { get; set; }

        [JsonProperty("enableSaturday")]
        public bool EnableSaturday { get; set; }
    }
}