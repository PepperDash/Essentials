extern alias Full;
using System.Collections.Generic;
using Full::Newtonsoft.Json;

namespace PepperDash.Essentials.Core.CrestronIO.Cards
{
    public class CenCi33Configuration
    {
        [JsonProperty("cards")]
        public Dictionary<uint, string> Cards { get; set; }
    }
}