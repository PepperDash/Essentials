using Newtonsoft.Json;
using System.Collections.Generic;

namespace UdmCws
{
    public class StatusProperties
    {
        /// <summary>
        /// Dictionary of device statuses keyed by device identifier
        /// </summary>
        [JsonProperty("devices")]
        public Dictionary<string, DeviceStatus> Devices { get; set; }
    }
}