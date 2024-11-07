
using Newtonsoft.Json;

namespace PepperDash.Essentials.Core.Room.Config
{

    public class EssentialsHuddleVtc1PropertiesConfig : EssentialsConferenceRoomPropertiesConfig
    {
		[JsonProperty("defaultDisplayKey")]
		public string DefaultDisplayKey { get; set; }

    }
}