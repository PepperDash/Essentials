using System.Collections.Generic;

using Newtonsoft.Json;
using PepperDash.Essentials.Core.Devices;

namespace PepperDash.Essentials.Core.Rooms.Combining
{
    /// <summary>
    /// Config properties for an EssentialsRoomCombiner device
    /// </summary>
    public class EssentialsRoomCombinerPropertiesConfig
    {
        /// <summary>
        /// Gets or sets a value indicating whether the system operates in automatic mode.
        /// <remarks>Some systems don't have partitions sensors, and show shouldn't allow auto mode to be turned on. When this is true in the configuration, 
        /// auto mode won't be allowed to be turned on.</remarks>
        /// </summary>
        [JsonProperty("disableAutoMode")]
        public bool DisableAutoMode { get; set; }

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

        /// <summary>
        /// Gets or sets the debounce time, in seconds, for scenario changes.
        /// </summary>
        [JsonProperty("scenarioChangeDebounceTimeSeconds")]
        public int ScenarioChangeDebounceTimeSeconds { get; set; }
    }

    /// <summary>
    /// Config properties for a partition that separates rooms
    /// </summary>
    public class PartitionConfig : IKeyName
    {
        /// <summary>
        /// Gets or sets the unique key associated with the object.
        /// </summary>
        [JsonProperty("key")]
        public string Key { get; set; }

        /// <summary>
        /// Gets or sets the name associated with the object.
        /// </summary>
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
        /// <summary>
        /// Gets or sets the key associated with the object.
        /// </summary>
        [JsonProperty("key")]
        public string Key { get; set; }

        /// <summary>
        /// Gets or sets the name associated with the object.
        /// </summary>
        [JsonProperty("name")]
        public string Name { get; set; }

        /// <summary>
        /// Gets or sets the collection of partition states.
        /// </summary>
        [JsonProperty("partitionStates")]
        public List<PartitionState> PartitionStates { get; set; }

        /// <summary>
        /// Determines which UI devices get mapped to which room in this scenario.  The Key should be the key of the UI device and the Value should be the key of the room to map to
        /// </summary>
        [JsonProperty("uiMap")]
        public Dictionary<string, string> UiMap { get; set; }

        /// <summary>
        /// Gets or sets the list of actions to be performed during device activation.
        /// </summary>
        [JsonProperty("activationActions")]
        public List<DeviceActionWrapper> ActivationActions { get; set; }

        /// <summary>
        /// Gets or sets the list of actions to be performed when a device is deactivated.
        /// </summary>
        [JsonProperty("deactivationActions")]
        public List<DeviceActionWrapper> DeactivationActions { get; set; }    
    }

    /// <summary>
    /// Config properties to represent the state of a partition sensor in a RoomCombinationScenario
    /// </summary>
    public class PartitionState
    {
        /// <summary>
        /// Gets or sets the partition key used to group and organize data within a storage system.
        /// </summary>
        [JsonProperty("partitionKey")]
        public string PartitionKey { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether a partition is currently present.
        /// </summary>
        [JsonProperty("partitionSensedState")]
        public bool PartitionPresent { get; set; }
    }
}