using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using PepperDash.Essentials.Core.DeviceTypeInterfaces;

namespace PepperDash.Essentials.AppServer.Messengers
{
    /// <summary>
    /// Represents a MobileControlMessage
    /// </summary>
    public class MobileControlMessage : IMobileControlMessage
    {
        /// <summary>
        /// Gets or sets the Type
        /// </summary>
        [JsonProperty("type")]
        public string Type { get; set; }

        /// <summary>
        /// Gets or sets the ClientId
        /// </summary>
        [JsonProperty("clientId")]
        public string ClientId { get; set; }

        /// <summary>
        /// Gets or sets the Content
        /// </summary>
        [JsonProperty("content")]
        public JToken Content { get; set; }
    }
}