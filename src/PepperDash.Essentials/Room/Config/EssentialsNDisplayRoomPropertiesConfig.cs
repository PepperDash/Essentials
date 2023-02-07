extern alias Full;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Crestron.SimplSharp;

using PepperDash.Core;
using PepperDash.Essentials.Core;

using Full.Newtonsoft.Json;

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