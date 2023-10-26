extern alias Full;
using Full::Newtonsoft.Json;

namespace PepperDash.Essentials.Core
{
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