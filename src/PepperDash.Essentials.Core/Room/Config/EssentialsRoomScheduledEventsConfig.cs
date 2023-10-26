extern alias Full;

using System.Collections.Generic;
using Full.Newtonsoft.Json;
using Full.Newtonsoft.Json.Converters;

namespace PepperDash.Essentials.Room.Config
{
    public class EssentialsRoomScheduledEventsConfig
    {
        [JsonProperty("scheduledEvents")]
        public List<ScheduledEventConfig> ScheduledEvents;
    }
}