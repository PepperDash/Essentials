extern alias Full;
using Full::Newtonsoft.Json;

namespace PepperDash.Essentials.Devices.Common.VideoCodec.ZoomRoom
{
    /// <summary>
    /// Represents a response from a ZoomRoom system
    /// </summary>
    public class Response
    {
        public Status Status { get; set; }
        public bool Sync { get; set; }
        [JsonProperty("topKey")]
        public string TopKey { get; set; }
        [JsonProperty("type")]
        public string Type { get; set; }

        public Response()
        {
            Status = new Status();
        }
    }
}