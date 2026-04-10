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
        /// <summary>
        /// Level controls for this audio control point
        /// </summary>
        [JsonProperty("levelControls")]
        public Dictionary<string, LevelControlListItem> LevelControls { get; set; } = new Dictionary<string, LevelControlListItem>();

        /// <summary>
        /// Presets for this audio control point
        /// </summary>
        [JsonProperty("presets")]
        public Dictionary<string, PresetListItem> Presets { get; set; } = new Dictionary<string, PresetListItem>();

    }
}
