extern alias Full;
using Full::Newtonsoft.Json;

namespace PepperDash.Essentials.Devices.Common.Codec
{
    public class BrandingLogoProperties
    {
        [JsonProperty("enable")]
        public bool Enable { get; set; }

        [JsonProperty("brandingUrl")]
        public string BrandingUrl { get; set; }
    }
}