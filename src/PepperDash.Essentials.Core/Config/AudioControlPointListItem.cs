using Crestron.SimplSharpPro;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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
