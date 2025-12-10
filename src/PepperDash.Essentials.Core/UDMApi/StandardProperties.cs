using Newtonsoft.Json;

namespace PepperDash.Essentials.Core
{
    internal class StandardProperties
    {
        [JsonProperty("version")]
        public string Version { get; set; }

        [JsonProperty("state")]
        public string State { get; set; }

        [JsonProperty("error")]
        public string Error { get; set; }

        [JsonProperty("occupancy")]
        public bool Occupancy { get; set; }

        [JsonProperty("helpRequest")]
        public string HelpRequest { get; set; }

        [JsonProperty("activity")]
        public string Activity { get; set; }
    }

}
