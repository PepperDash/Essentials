using Newtonsoft.Json;
using PepperDash.Essentials.Core.Devices;
using System.Collections.Generic;

namespace PepperDash.Essentials.Core.Config
{
    /// <summary>
    /// Represents a AudioControlPointListItem
    /// </summary>
    public class AudioControlPointListItem
    {
        [JsonProperty("levelControls")]
        public Dictionary<string, LevelControlListItem> LevelControls { get; set; } = new Dictionary<string, LevelControlListItem>();

        [JsonProperty("presets")]
        public Dictionary<string, PresetListItem> Presets { get; set; } = new Dictionary<string, PresetListItem>();

    }
}
