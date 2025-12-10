using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PepperDash.Essentials.Core.UDMApi
{
    internal class StatusProperties
    {
        /// <summary>
        /// Dictionary of device statuses keyed by device identifier
        /// </summary>
        [JsonProperty("devices")]
        public Dictionary<string, DeviceStatus> devices { get; set; }

        public StatusProperties()
        {
            devices = new Dictionary<string, DeviceStatus>();
        }
    }
}
