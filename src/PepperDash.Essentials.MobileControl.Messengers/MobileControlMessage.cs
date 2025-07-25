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
        [JsonProperty("type")]
        /// <summary>
        /// Gets or sets the Type
        /// </summary>
        public string Type { get; set; }

        [JsonProperty("clientId")]
        /// <summary>
        /// Gets or sets the ClientId
        /// </summary>
        public string ClientId { get; set; }

        [JsonProperty("content")]
        /// <summary>
        /// Gets or sets the Content
        /// </summary>
        public JToken Content { get; set; }
    }
}
