extern alias Full;

using System.Collections.Generic;
using Full.Newtonsoft.Json;

namespace PepperDash.Essentials.Core
{
    public class OccupancyAggregatorConfig
    {
        [JsonProperty("deviceKeys")] public List<string> DeviceKeys { get; set; }

        public OccupancyAggregatorConfig()
        {
            DeviceKeys = new List<string>();
        }
    }
}