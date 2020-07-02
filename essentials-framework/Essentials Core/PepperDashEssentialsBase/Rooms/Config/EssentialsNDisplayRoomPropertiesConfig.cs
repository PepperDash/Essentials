using System.Collections.Generic;
using PepperDash.Core;
using PepperDash.Essentials.Core;

using Newtonsoft.Json;
using PepperDash.Essentials.Core.Config;

namespace PepperDash.Essentials.Core.Rooms.Config
{
    /// <summary>
    /// 
    /// </summary>
    public class EssentialsNDisplayRoomPropertiesConfig : EssentialsHuddleVtc1PropertiesConfig
    {
        [JsonProperty("defaultAudioBehavior")]
        public string DefaultAudioBehavior { get; set; }
        [JsonProperty("defaultVideoBehavior")]
        public string DefaultVideoBehavior { get; set; }
        [JsonProperty("destinationListKey")]
        public string DestinationListKey { get; set; }
    }

    public class DisplayItem : IKeyName
    {
        public string Key { get; set; }
        public string Name { get; set; }
    }
}