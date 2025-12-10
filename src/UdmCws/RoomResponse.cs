using System.Collections.Generic;
using Newtonsoft.Json;

namespace UdmCws
{
    /// <summary>
    /// Represents the complete room response for UDM API
    /// </summary>
    public class RoomResponse
    {
        /// <summary>
        /// API version string
        /// </summary>
        [JsonProperty("apiVersion")]
        public string ApiVersion { get; private set; }

        /// <summary>
        /// Standard room properties
        /// </summary>
        [JsonProperty("standard")]
        public StandardProperties Standard { get; set; }

        /// <summary>
        /// Status information including devices
        /// </summary>
        [JsonProperty("status")]
        public StatusProperties Status { get; set; }

        /// <summary>
        /// Custom properties dictionary
        /// </summary>
        [JsonProperty("custom")]
        public Dictionary<string, CustomProperties> Custom { get; set; }
    }
}
