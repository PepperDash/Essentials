using Newtonsoft.Json;
namespace PepperDash.Essentials.Devices.Common.VideoCodec.Interfaces
{
  /// <summary>
  /// Represents a Participant
  /// </summary>
  public class Participant
  {
    /// <summary>
    /// Gets or sets the UserId
    /// </summary>
    [JsonProperty("userId")]
    public int UserId { get; set; }

    /// <summary>
    /// Gets or sets the IsHost
    /// </summary>
    [JsonProperty("isHost")]
    public bool IsHost { get; set; }

    /// <summary>
    /// Gets or sets the IsCohost
    /// </summary>
    [JsonProperty("isCohost")]
    public bool IsCohost { get; set; }

    /// <summary>
    /// Gets or sets the IsMyself
    /// </summary>
    [JsonProperty("isMyself")]
    public bool IsMyself { get; set; }

    /// <summary>
    /// Gets or sets the Name
    /// </summary>
    [JsonProperty("name")]
    public string Name { get; set; }

    /// <summary>
    /// Gets or sets the Email
    /// </summary>
    [JsonProperty("email")]
    public bool CanMuteVideo { get; set; }

    /// <summary>
    /// Gets or sets the CanUnmuteVideo
    /// </summary>
    [JsonProperty("canUnmuteVideo")]
    public bool CanUnmuteVideo { get; set; }

    /// <summary>
    /// Gets or sets the CanMuteAudio
    /// </summary>
    [JsonProperty("canMuteAudio")]
    public bool CanMuteAudio { get; set; }

    /// <summary>
    /// Gets or sets the AudioMuteFb
    /// </summary>
    [JsonProperty("audioMuteFb")]
    public bool AudioMuteFb { get; set; }

    /// <summary>
    /// Gets or sets the HandIsRaisedFb
    /// </summary>
    [JsonProperty("handIsRaisedFb")]
    public bool HandIsRaisedFb { get; set; }

    /// <summary>
    /// Gets or sets the IsPinnedFb
    /// </summary>
    [JsonProperty("isPinnedFb")]
    public bool IsPinnedFb { get; set; }

    /// <summary>
    /// Gets or sets the ScreenIndexIsPinnedToFb
    /// </summary>
    [JsonProperty("screenIndexIsPinnedToFb")]
    public int ScreenIndexIsPinnedToFb { get; set; }

    /// <summary>
    /// Initializes a new instance of the <see cref="Participant"/> class
    /// </summary>
    public Participant()
    {
      // Initialize to -1 (no screen)
      ScreenIndexIsPinnedToFb = -1;
    }
  }
}