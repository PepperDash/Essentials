
using System.Collections.Generic;
using Newtonsoft.Json;
using PepperDash.Core;
using PepperDash.Essentials.Core;

namespace PepperDash.Essentials.Room.Config
{
    /// <summary>
    /// 
    /// </summary>
    public class EssentialsNDisplayRoomPropertiesConfig : EssentialsConferenceRoomPropertiesConfig
    {
        [JsonProperty("defaultAudioBehavior")]
        public string DefaultAudioBehavior { get; set; }
        [JsonProperty("defaultVideoBehavior")]
        public string DefaultVideoBehavior { get; set; }
        [JsonProperty("displays")]
        public Dictionary<eSourceListItemDestinationTypes, DisplayItem> Displays { get; set; }

        public EssentialsNDisplayRoomPropertiesConfig()
        {
            Displays = new Dictionary<eSourceListItemDestinationTypes, DisplayItem>();
        }

    }

    public class DisplayItem : IKeyName
    {
        public string Key { get; set; }
        public string Name { get; set; }
    }

}