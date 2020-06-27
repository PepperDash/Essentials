using Newtonsoft.Json;
using PepperDash.Essentials.Core.Config;

namespace PepperDash.Essentials.Core.Rooms.Config
{

    public class EssentialsHuddleVtc1PropertiesConfig : EssentialsConferenceRoomPropertiesConfig
    {
		[JsonProperty("defaultDisplayKey")]
		public string DefaultDisplayKey { get; set; }
    }
}