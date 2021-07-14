using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Crestron.SimplSharp;

using PepperDash.Core;

using Newtonsoft.Json;

namespace PepperDash.Essentials.Core.Room
{
    /// <summary>
    /// Config properties for an EssentialsRoomCombiner device
    /// </summary>
    public class EssentialsRoomCombinerPropertiesConfig
    {
        [JsonProperty("partitions")]
        public List<PartitionConfig> Partitions {get; set;}

        [JsonProperty("scenarios")]
        public List<RoomCombinationScenario> Scenarios { get; set; }

        [JsonProperty("rooms")]
        public List<IKeyed> Rooms {get; set;}
    }

    /// <summary>
    /// Config properties for a partition that separates rooms
    /// </summary>
    public class PartitionConfig : IKeyName
    {
        /// <summary>
        /// Key of the device that implements IPartitionStateProvider to provide the state of the partition
        /// </summary>
        [JsonProperty("deviceKey")]
        public string DeviceKey { get; set; }

        /// <summary>
        /// Keys of the rooms that this partion would be located between
        /// </summary>
        [JsonProperty("roomKeys")]
        public List<string> RoomKeys { get; set; }
    }

    /// <summary>
    /// Config propeties for a room combination scenario
    /// </summary>
    public class RoomCombinationScenario : IKeyName
    {
        [JsonProperty("partitionStates")]
        public List<PartitionState> PartitionStates { get; set; }

        [JsonProperty("enabledRoomKeys")]
        public List<string> EnabledRoomKeys { get; set; }

        [JsonProperty("actions")]
        public List<DeviceActionWrapper> Actions { get; set; }    
    }

    /// <summary>
    /// Config properties to represent the state of a partition sensor in a RoomCombinationScenario
    /// </summary>
    public class PartitionState
    {
        [JsonProperty("partitionKey")]
        public string PartitionKey { get; set; }

        [JsonProperty("partitionSensedState")]
        public bool PartitionSensedState { get; set; }
    }
}