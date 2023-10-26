extern alias Full;
using Full::Newtonsoft.Json;

namespace PepperDash.Essentials.Devices.Common.VideoCodec.ZoomRoom
{
    public class Status
    {
        [JsonProperty("message")]
        public string Message { get; set; }
        [JsonProperty("state")]
        public string State { get; set; }
    }
}