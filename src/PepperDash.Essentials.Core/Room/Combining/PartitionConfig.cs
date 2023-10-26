extern alias Full;
using System.Collections.Generic;
using Full::Newtonsoft.Json;
using PepperDash.Core;

namespace PepperDash.Essentials.Core
{
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
}