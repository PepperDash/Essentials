using Newtonsoft.Json;

namespace PepperDash.Essentials.Core.UDMApi
{
    internal class StandardProperties
    {
        [JsonProperty("version")]
        public string version { get; set; }

        [JsonProperty("state")]
        public string state { get; set; }

        [JsonProperty("error")]
        public string error { get; set; }

        [JsonProperty("occupancy")]
        public bool occupancy { get; set; }

        [JsonProperty("helpRequest")]
        public string helpRequest { get; set; }

        [JsonProperty("activity")]
        public string activity { get; set; }
    }

}
