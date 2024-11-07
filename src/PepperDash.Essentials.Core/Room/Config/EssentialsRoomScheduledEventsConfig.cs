﻿using System.Collections.Generic;
using Crestron.SimplSharp.Scheduler;
using Newtonsoft.Json;
using PepperDash.Essentials.Core.Devices;

namespace PepperDash.Essentials.Core.Room.Config
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