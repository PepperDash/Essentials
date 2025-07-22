using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using PepperDash.Essentials.Core.DeviceTypeInterfaces;

namespace PepperDash.Essentials.AppServer.Messengers
{
    /// <summary>
    /// Represents a mobile control message that can be sent between clients and the system
    /// </summary>
    public class MobileControlMessage : IMobileControlMessage
    {
        /// <summary>
        /// Gets or sets the message type/path for routing
        /// </summary>
        [JsonProperty("type")]
        public string Type { get; set; }

        /// <summary>
        /// Gets or sets the client ID this message is intended for (null for broadcast)
        /// </summary>
        [JsonProperty("clientId")]
        public string ClientId { get; set; }

        /// <summary>
        /// Gets or sets the JSON content of the message
        /// </summary>
        [JsonProperty("content")]
        public JToken Content { get; set; }
    }
}
