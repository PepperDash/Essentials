using Newtonsoft.Json;

namespace PepperDash.Essentials.Core.UDMApi
{
    internal class DeviceStatus
    {
        [JsonProperty("label")]
        public string Label { get; set; }

        [JsonProperty("status")]
        public string Status { get; set; }

        [JsonProperty("description")]
        public string Description { get; set; }

        [JsonProperty("videoSource")]
        public string VideoSource { get; set; }

        [JsonProperty("audioSource")]
        public string AudioSource { get; set; }

        [JsonProperty("usage")]
        public int Usage { get; set; }

        [JsonProperty("error")]
        public string Error { get; set; }
    }

}
