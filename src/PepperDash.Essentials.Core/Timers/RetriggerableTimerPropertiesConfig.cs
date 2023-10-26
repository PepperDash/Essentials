extern alias Full;
using System.Collections.Generic;
using Full::Newtonsoft.Json;

namespace PepperDash.Essentials.Core.Timers
{
    /// <summary>
    /// Configuration Properties for RetriggerableTimer
    /// </summary>
    public class RetriggerableTimerPropertiesConfig
    {
        [JsonProperty("startTimerOnActivation")]
        public bool StartTimerOnActivation { get; set; }

        [JsonProperty("timerIntervalMs")]
        public long TimerIntervalMs { get; set; }

        [JsonProperty("events")]
        public Dictionary<eRetriggerableTimerEvents, DeviceActionWrapper> Events { get; set; }

        public RetriggerableTimerPropertiesConfig()
        {
            Events = new Dictionary<eRetriggerableTimerEvents, DeviceActionWrapper>();
        }
    }
}