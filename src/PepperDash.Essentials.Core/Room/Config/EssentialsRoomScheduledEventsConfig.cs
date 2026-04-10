

using System.Collections.Generic;
using Crestron.SimplSharp.Scheduler;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using PepperDash.Essentials.Core;

namespace PepperDash.Essentials.Room.Config
{
    /// <summary>
    /// Represents a EssentialsRoomScheduledEventsConfig
    /// </summary>
    public class EssentialsRoomScheduledEventsConfig
    {
        /// <summary>
        /// Gets or sets the ScheduledEvents
        /// </summary>
        [JsonProperty("scheduledEvents")]
        public List<ScheduledEventConfig> ScheduledEvents;
    }

    /// <summary>
    /// Represents a ScheduledEventConfig
    /// </summary>
    public class ScheduledEventConfig
    {
        /// <summary>
        /// Gets or sets the Key
        /// </summary>
        [JsonProperty("key")]
        public string Key;

        /// <summary>
        /// Gets or sets the Name
        /// </summary>
        [JsonProperty("name")]
        public string Name;

        /// <summary>
        /// Gets or sets the Days
        /// </summary>
        [JsonProperty("days")]
        public ScheduledEventCommon.eWeekDays Days;

        /// <summary>
        /// Gets or sets the Time
        /// </summary>
        [JsonProperty("time")]
        public string Time;

        /// <summary>
        /// Gets or sets the Actions
        /// </summary>
        [JsonProperty("actions")]
        public List<DeviceActionWrapper> Actions;

        /// <summary>
        /// Gets or sets the Persistent
        /// </summary>
        [JsonProperty("persistent")]
        public bool Persistent;

        /// <summary>
        /// Gets or sets the Acknowledgeable
        /// </summary>
        [JsonProperty("acknowledgeable")]
        public bool Acknowledgeable;

        /// <summary>
        /// Gets or sets the Enable
        /// </summary>
        [JsonProperty("enable")]
        public bool Enable;
    }
}