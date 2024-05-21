using Crestron.SimplSharpPro;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PepperDash.Essentials.Core.Config
{
    public class AudioControlPointListItem
    {
        [JsonProperty("levelControls")]
        public Dictionary<string, LevelControlListItem> LevelControls { get; set; }

        [JsonProperty("presets")]
        public Dictionary<string, PresetListItem> Presets { get; set; }
    }
}
