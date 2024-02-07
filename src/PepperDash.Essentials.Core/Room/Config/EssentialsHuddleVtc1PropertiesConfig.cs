
using Newtonsoft.Json;

namespace PepperDash.Essentials.Room.Config
{

    public class EssentialsHuddleVtc1PropertiesConfig : EssentialsConferenceRoomPropertiesConfig
    {
		[JsonProperty("defaultDisplayKey")]
		public string DefaultDisplayKey { get; set; }

    }
}