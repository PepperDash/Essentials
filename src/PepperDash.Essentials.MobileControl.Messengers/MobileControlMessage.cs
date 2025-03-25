using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using PepperDash.Essentials.Core.DeviceTypeInterfaces;

namespace PepperDash.Essentials.AppServer.Messengers
{
    public class MobileControlMessage : IMobileControlMessage
    {
        [JsonProperty("type")]
        public string Type { get; set; }

        [JsonProperty("clientId")]
        public string ClientId { get; set; }

        [JsonProperty("content")]
        public JToken Content { get; set; }
    }
}
