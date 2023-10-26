extern alias Full;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Crestron.SimplSharp;
using Full.Newtonsoft.Json;

namespace PepperDash.Essentials.Core
{
    /// <summary>
    /// Config properties for an EssentialsRoomCombiner device
    /// </summary>
    public class EssentialsRoomCombinerPropertiesConfig
    {
        /// <summary>
        /// The list of partitions that device the rooms
        /// </summary>
        [JsonProperty("partitions")]
        public List<PartitionConfig> Partitions {get; set;}

        /// <summary>
        /// The list of combinations scenarios for the rooms
        /// </summary>
        [JsonProperty("scenarios")]
        public List<RoomCombinationScenarioConfig> Scenarios { get; set; }

        /// <summary>
        /// The list of rooms keys that can be combined
        /// </summary>
        [JsonProperty("roomKeys")]
        public List<string> RoomKeys {get; set;}

        /// <summary>
        /// Set to true to default to manual mode
        /// </summary>
        [JsonProperty("defaultToManualMode")]
        public bool defaultToManualMode { get; set; }

        /// <summary>
        /// The key of the scenario to default to at system startup if in manual mode
        /// </summary>
        [JsonProperty("defaultScenarioKey")]
        public string defaultScenarioKey { get; set; }

        [JsonProperty("scenarioChangeDebounceTimeSeconds")]
        public int ScenarioChangeDebounceTimeSeconds { get; set; }
    }
}