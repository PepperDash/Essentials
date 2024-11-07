using Crestron.SimplSharpPro;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using PepperDash.Essentials.Core.Devices;

namespace PepperDash.Essentials.Core.Config
{
    public class AudioControlPointListItem
    {
        [JsonProperty("levelControls")]
        public Dictionary<string, LevelControlListItem> LevelControls { get; set; } = new Dictionary<string, LevelControlListItem>();

        [JsonProperty("presets")]
        public Dictionary<string, PresetListItem> Presets { get; set; } = new Dictionary<string, PresetListItem>();

    }
}
