using Newtonsoft.Json;
using System.Collections.Generic;

namespace PepperDash.Essentials.Core.UDMApi
{
    internal class StatusProperties
    {
        /// <summary>
        /// Dictionary of device statuses keyed by device identifier
        /// </summary>
        [JsonProperty("devices")]
        public Dictionary<string, DeviceStatus> Devices { get; set; }
    }
}
