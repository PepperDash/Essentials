extern alias Full;
using Full::Newtonsoft.Json;

namespace PepperDash.Essentials.Core.Utilities
{
    public class SequencedDeviceActionWrapper : DeviceActionWrapper
    {
        [JsonProperty("delayMs")]
        public int DelayMs { get; set; }
    }
}