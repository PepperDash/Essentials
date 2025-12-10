using Newtonsoft.Json;

namespace PepperDash.Essentials.Core.UDMApi
{

    internal class CustomProperties
    {
        [JsonProperty("label")]
        public string Label { get; set; }

        [JsonProperty("value")]
        public string Value { get; set; }
    }

}
