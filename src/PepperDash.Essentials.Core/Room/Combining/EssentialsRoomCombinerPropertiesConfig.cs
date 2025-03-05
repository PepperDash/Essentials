

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Crestron.SimplSharp;

using PepperDash.Core;

using Newtonsoft.Json;

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

    /// <summary>
    /// Config properties for a partition that separates rooms
    /// </summary>
    public class PartitionConfig : IKeyName
    {
        [JsonProperty("key")]
        public string Key { get; set; }

        [JsonProperty("name")]
        public string Name { get; set; }

        /// <summary>
        /// Key of the device that implements IPartitionStateProvider to provide the state of the partition
        /// </summary>
        [JsonProperty("deviceKey")]
        public string DeviceKey { get; set; }

        /// <summary>
        /// Keys of the rooms that this partion would be located between
        /// </summary>
        [JsonProperty("adjacentRoomKeys")]
        public List<string> AdjacentRoomKeys { get; set; }
    }

    /// <summary>
    /// Config propeties for a room combination scenario
    /// </summary>
    public class RoomCombinationScenarioConfig : IKeyName
    {
        [JsonProperty("key")]
        public string Key { get; set; }

        [JsonProperty("name")]
        public string Name { get; set; }

        [JsonProperty("partitionStates")]
        public List<PartitionState> PartitionStates { get; set; }

        /// <summary>
        /// Determines which UI devices get mapped to which room in this scenario.  The Key should be the key of the UI device and the Value should be the key of the room to map to
        /// </summary>
        [JsonProperty("uiMap")]
        public Dictionary<string, string> UiMap { get; set; }

        [JsonProperty("activationActions")]
        public List<DeviceActionWrapper> ActivationActions { get; set; }

        [JsonProperty("deactivationActions")]
        public List<DeviceActionWrapper> DeactivationActions { get; set; }    
    }

    /// <summary>
    /// Config properties to represent the state of a partition sensor in a RoomCombinationScenario
    /// </summary>
    public class PartitionState
    {
        [JsonProperty("partitionKey")]
        public string PartitionKey { get; set; }

        [JsonProperty("partitionSensedState")]
        public bool PartitionPresent { get; set; }
    }
}