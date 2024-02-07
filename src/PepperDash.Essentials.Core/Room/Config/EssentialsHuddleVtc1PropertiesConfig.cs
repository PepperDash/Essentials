
using Newtonsoft.Json;

namespace PDT.Plugins.Essentials.Rooms.Config
{

    public class EssentialsHuddleVtc1PropertiesConfig : EssentialsConferenceRoomPropertiesConfig
    {
		[JsonProperty("defaultDisplayKey")]
		public string DefaultDisplayKey { get; set; }

    }
}