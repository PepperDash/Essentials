using Newtonsoft.Json;

namespace PepperDash.Essentials.Core.UDMApi
{

    internal class CustomProperties
    {
        [JsonProperty("label")]
        public string label { get; set; }

        [JsonProperty("value")]
        public string value { get; set; }
    }

}
