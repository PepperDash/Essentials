
using Newtonsoft.Json;

namespace PepperDash.Essentials.Room.Config
{

  /// <summary>
  /// Represents a EssentialsHuddleVtc1PropertiesConfig
  /// </summary>
  public class EssentialsHuddleVtc1PropertiesConfig : EssentialsConferenceRoomPropertiesConfig
  {
    /// <summary>
    /// Gets or sets the DefaultDisplayKey
    /// </summary>
    [JsonProperty("defaultDisplayKey")]
    public string DefaultDisplayKey { get; set; }

  }
}