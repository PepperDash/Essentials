using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Crestron.SimplSharp;

using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using Newtonsoft.Json.Linq;

namespace PepperDash.Essentials.Room.Config
{
    /// <summary>
    /// 
    /// </summary>
    public class EssentialsHuddleRoomPropertiesConfig : EssentialsRoomPropertiesConfig
    {
        /// <summary>
        /// The key of the default display device
        /// </summary>
        [JsonProperty("defaultDisplayKey")]
        public string DefaultDisplayKey { get; set; }

        /// <summary>
        /// The key of the default audio device
        /// </summary>
        [JsonProperty("defaultAudioKey")]
        public string DefaultAudioKey { get; set; }

        /// <summary>
        /// The key of the source list for the room
        /// </summary>
        [JsonProperty("sourceListKey")]
        public string SourceListKey { get; set; }

        /// <summary>
        /// The key of the default source item from the source list
        /// </summary>
        [JsonProperty("defaultSourceItem")]
        public string DefaultSourceItem { get; set; }
    }
}