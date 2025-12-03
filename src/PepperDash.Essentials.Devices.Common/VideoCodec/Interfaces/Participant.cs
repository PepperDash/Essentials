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
    public int UserId { get; set; }

    /// <summary>
    /// Gets or sets the IsHost
    /// </summary>
    public bool IsHost { get; set; }

    /// <summary>
    /// Gets or sets the IsMyself
    /// </summary>
    public bool IsMyself { get; set; }

    /// <summary>
    /// Gets or sets the Name
    /// </summary>
    public string Name { get; set; }

    /// <summary>
    /// Gets or sets the Email
    /// </summary>
    public bool CanMuteVideo { get; set; }

    /// <summary>
    /// Gets or sets the CanUnmuteVideo
    /// </summary>
    public bool CanUnmuteVideo { get; set; }

    /// <summary>
    /// Gets or sets the CanMuteAudio
    /// </summary>
    public bool VideoMuteFb { get; set; }

    /// <summary>
    /// Gets or sets the AudioMuteFb
    /// </summary>
    public bool AudioMuteFb { get; set; }

    /// <summary>
    /// Gets or sets the HandIsRaisedFb
    /// </summary>
    public bool HandIsRaisedFb { get; set; }

    /// <summary>
    /// Gets or sets the IsPinnedFb
    /// </summary>
    public bool IsPinnedFb { get; set; }

    /// <summary>
    /// Gets or sets the ScreenIndexIsPinnedToFb
    /// </summary>
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