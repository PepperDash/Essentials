using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using PepperDash.Essentials.Core.DeviceTypeInterfaces;

namespace PepperDash.Essentials.AppServer.Messengers
{

#if SERIES4
    public class MobileControlMessage : IMobileControlMessage
#else
    public class MobileControlMessage
#endif
    {
        [JsonProperty("type")]
        public string Type { get; set; }

        [JsonProperty("clientId")]
        public string ClientId { get; set; }

        [JsonProperty("content")]
        public JToken Content { get; set; }
    }
}
