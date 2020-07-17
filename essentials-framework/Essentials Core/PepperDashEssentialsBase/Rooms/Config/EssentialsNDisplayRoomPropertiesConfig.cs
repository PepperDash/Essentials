using Newtonsoft.Json.Converters;
using PepperDash.Core;
using Newtonsoft.Json;

namespace PepperDash.Essentials.Core.Rooms.Config
{
    /// <summary>
    /// 
    /// </summary>
    public class EssentialsNDisplayRoomPropertiesConfig : EssentialsHuddleVtc1PropertiesConfig
    {
        [JsonProperty("defaultAudioBehavior")]
        [JsonConverter(typeof(StringEnumConverter))]
        public EAudioBehavior DefaultAudioBehavior { get; set; }
        [JsonProperty("defaultVideoBehavior")]
        [JsonConverter(typeof(StringEnumConverter))]
        public EVideoBehavior DefaultVideoBehavior { get; set; }
        [JsonProperty("destinationListKey")]
        public string DestinationListKey { get; set; }
        [JsonProperty("enableVideoBehaviorToggle")]
        public bool EnableVideoBehaviorToggle { get; set; }
    }

    public class DisplayItem : IKeyName
    {
        public string Key { get; set; }
        public string Name { get; set; }
    }

    public enum EVideoBehavior
    {
        Basic,
        Advanced
    }

    public enum EAudioBehavior
    {
        AudioFollowVideo,
        ChooseAudioFromDisplay,
        AudioFollowVideoWithDeroute
    }
}