extern alias Full;
using System.Collections.Generic;
using Full::Newtonsoft.Json;

namespace PepperDash.Essentials.Core.Presets
{
    public class PresetsList
    {
        [JsonProperty(Required=Required.Always,PropertyName = "name")]
        public string Name { get; set; }

        [JsonProperty(Required = Required.Always, PropertyName = "channels")]
        public List<PresetChannel> Channels { get; set; }
    }
}