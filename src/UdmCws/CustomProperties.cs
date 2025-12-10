using Newtonsoft.Json;

namespace UdmCws
{

    public class CustomProperties
    {
        [JsonProperty("label")]
        public string Label { get; set; }

        [JsonProperty("value")]
        public string Value { get; set; }
    }

}
