extern alias Full;
using System.Collections.Generic;
using Full::Newtonsoft.Json;
using PepperDash.Core;

namespace PepperDash.Essentials.Core
{
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
}