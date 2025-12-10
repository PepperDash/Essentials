using System.Collections.Generic;
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
        public StandardProperties standard { get; private set; }

        /// <summary>
        /// Status information including devices
        /// </summary>
        [JsonProperty("status")]
        public StatusProperties status { get; private set; }

        /// <summary>
        /// Custom properties dictionary
        /// </summary>
        [JsonProperty("custom")]
        public Dictionary<string, CustomProperties> custom { get; private set; }
    }

    /// <summary>
    /// Represents status properties including devices
    /// </summary>
}
