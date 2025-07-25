

using System.Collections.Generic;
using Crestron.SimplSharp.Scheduler;
using Newtonsoft.Json;
using PepperDash.Essentials.Core;

namespace PepperDash.Essentials.Room.Config
{
    /// <summary>
    /// Represents a EssentialsRoomScheduledEventsConfig
    /// </summary>
    public class EssentialsRoomScheduledEventsConfig
    {
        [JsonProperty("scheduledEvents")]
        /// <summary>
        /// Gets or sets the ScheduledEvents
        /// </summary>
        public List<ScheduledEventConfig> ScheduledEvents;
    }

    /// <summary>
    /// Represents a ScheduledEventConfig
    /// </summary>
    public class ScheduledEventConfig
    {
        [JsonProperty("key")]
        /// <summary>
        /// Gets or sets the Key
        /// </summary>
        public string Key;

        [JsonProperty("name")]
        /// <summary>
        /// Gets or sets the Name
        /// </summary>
        public string Name;

        [JsonProperty("days")]
        public ScheduledEventCommon.eWeekDays Days;

        [JsonProperty("time")]
        /// <summary>
        /// Gets or sets the Time
        /// </summary>
        public string Time;

        [JsonProperty("actions")]
        /// <summary>
        /// Gets or sets the Actions
        /// </summary>
        public List<DeviceActionWrapper> Actions;

        [JsonProperty("persistent")]
        /// <summary>
        /// Gets or sets the Persistent
        /// </summary>
        public bool Persistent;

        [JsonProperty("acknowledgeable")]
        /// <summary>
        /// Gets or sets the Acknowledgeable
        /// </summary>
        public bool Acknowledgeable;

        [JsonProperty("enable")]
        /// <summary>
        /// Gets or sets the Enable
        /// </summary>
        public bool Enable;
    }
}