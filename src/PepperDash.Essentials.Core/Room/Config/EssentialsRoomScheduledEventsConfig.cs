

using System.Collections.Generic;
using Crestron.SimplSharp.Scheduler;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using PepperDash.Essentials.Core;

namespace PepperDash.Essentials.Room.Config
{
    public class EssentialsRoomScheduledEventsConfig
    {
        [JsonProperty("scheduledEvents")]
        public List<ScheduledEventConfig> ScheduledEvents;
    }

    public class ScheduledEventConfig
    {
        [JsonProperty("key")]
        public string Key;

        [JsonProperty("name")]
        public string Name;

        [JsonProperty("days")]
        public ScheduledEventCommon.eWeekDays Days;

        [JsonProperty("time")]
        public string Time;

        [JsonProperty("actions")]
        public List<DeviceActionWrapper> Actions;

        [JsonProperty("persistent")]
        public bool Persistent;

        [JsonProperty("acknowledgeable")]
        public bool Acknowledgeable;

        [JsonProperty("enable")]
        public bool Enable;
    }
}