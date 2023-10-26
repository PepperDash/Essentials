extern alias Full;
using Full::Newtonsoft.Json;

namespace PepperDash.Essentials.Core.CrestronIO.Cards
{
    public class CenCi31Configuration
    {
        [JsonProperty("card")]
        public string Card { get; set; }
    }
}