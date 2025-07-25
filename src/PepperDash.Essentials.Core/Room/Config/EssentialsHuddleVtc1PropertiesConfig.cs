
using Newtonsoft.Json;

namespace PepperDash.Essentials.Room.Config
{

    /// <summary>
    /// Represents a EssentialsHuddleVtc1PropertiesConfig
    /// </summary>
    public class EssentialsHuddleVtc1PropertiesConfig : EssentialsConferenceRoomPropertiesConfig
    {
		[JsonProperty("defaultDisplayKey")]
  /// <summary>
  /// Gets or sets the DefaultDisplayKey
  /// </summary>
		public string DefaultDisplayKey { get; set; }

    }
}