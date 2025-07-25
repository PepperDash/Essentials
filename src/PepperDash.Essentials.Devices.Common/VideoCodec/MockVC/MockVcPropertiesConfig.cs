using System.Collections.Generic;
using Newtonsoft.Json;
using PepperDash.Essentials.Devices.Common.Codec;

namespace PepperDash.Essentials.Devices.Common.VideoCodec
{
    /// <summary>
    /// Represents a MockVcPropertiesConfig
    /// </summary>
    public class MockVcPropertiesConfig
    {
        [JsonProperty("favorites")]
        /// <summary>
        /// Gets or sets the Favorites
        /// </summary>
        public List<CodecActiveCallItem> Favorites { get; set; }

        [JsonProperty("presets")]
        /// <summary>
        /// Gets or sets the Presets
        /// </summary>
        public List<CodecRoomPreset> Presets { get; set; }

        public MockVcPropertiesConfig()
        {
            Favorites = new List<CodecActiveCallItem>();
            Presets = new List<CodecRoomPreset>();
        }
    }
}