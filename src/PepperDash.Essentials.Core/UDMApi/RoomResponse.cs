using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace PepperDash.Essentials.Core.UDMApi
{
    /// <summary>
    /// Represents the complete room response for UDM API
    /// </summary>
    internal class RoomResponse
    {
        public RoomResponse()
        {
            standard = new StandardProperties();
            status = new StatusProperties();
            custom = new Dictionary<string, CustomProperties>();
        }

        /// <summary>
        /// API version string
        /// </summary>
        [JsonProperty("apiVersion")]
        public string apiVersion { get; set; }

        /// <summary>
        /// Standard room properties
        /// </summary>
        [JsonProperty("standard")]
        public StandardProperties standard { get; set; }

        /// <summary>
        /// Status information including devices
        /// </summary>
        [JsonProperty("status")]
        public StatusProperties status { get; set; }

        /// <summary>
        /// Custom properties dictionary
        /// </summary>
        [JsonProperty("custom")]
        public Dictionary<string, CustomProperties> custom { get; set; }
    }

    /// <summary>
    /// Represents status properties including devices
    /// </summary>
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
