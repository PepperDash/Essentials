extern alias Full;
using Full::Newtonsoft.Json;

namespace PepperDash.Essentials.Devices.Common.Codec
{
    public class SharingProperties
    {
        [JsonProperty("autoShareContentWhileInCall")]
        public bool AutoShareContentWhileInCall { get; set; }
    }
}