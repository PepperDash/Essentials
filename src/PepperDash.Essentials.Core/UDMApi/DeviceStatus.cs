using Newtonsoft.Json;

namespace PepperDash.Essentials.Core.UDMApi
{
    internal class DeviceStatus
    {
        [JsonProperty("label")]
        public string label { get; set; }

        [JsonProperty("status")]
        public string status { get; set; }

        [JsonProperty("description")]
        public string description { get; set; }

        [JsonProperty("videoSource")]
        public string videoSource { get; set; }

        [JsonProperty("audioSource")]
        public string audioSource { get; set; }

        [JsonProperty("usage")]
        public int usage { get; set; }

        [JsonProperty("error")]
        public string error { get; set; }
    }

}
