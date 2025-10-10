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
        /// <summary>
        /// Gets or sets the Favorites
        /// </summary>
        [JsonProperty("favorites")]
        public List<CodecActiveCallItem> Favorites { get; set; }

        /// <summary>
        /// Gets or sets the Presets
        /// </summary>
        [JsonProperty("presets")]
        public List<CodecRoomPreset> Presets { get; set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="MockVcPropertiesConfig"/> class.
        /// </summary>
        public MockVcPropertiesConfig()
        {
            Favorites = new List<CodecActiveCallItem>();
            Presets = new List<CodecRoomPreset>();
        }
    }
}