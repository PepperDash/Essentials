
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

    /// <summary>
    /// Represents a DisplayItem
    /// </summary>
    public class DisplayItem : IKeyName
    {
        /// <summary>
        /// Gets or sets the Key
        /// </summary>
        public string Key { get; set; }
        /// <summary>
        /// Gets or sets the Name
        /// </summary>
        public string Name { get; set; }
    }

}