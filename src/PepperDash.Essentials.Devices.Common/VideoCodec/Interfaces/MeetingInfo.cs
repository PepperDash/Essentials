using Newtonsoft.Json;

namespace PepperDash.Essentials.Devices.Common.VideoCodec.Interfaces
{
  /// <summary>
  /// Represents a MeetingInfo
  /// </summary>
  public class MeetingInfo
  {

    /// <summary>
    /// Gets or sets the Id
    /// </summary>
    [JsonProperty("id", NullValueHandling = NullValueHandling.Ignore)]
    public string Id { get; private set; }

    /// <summary>
    /// Gets or sets the Name
    /// </summary>
    [JsonProperty("name", NullValueHandling = NullValueHandling.Ignore)]
    public string Name { get; private set; }

    /// <summary>
    /// Gets or sets the Host
    /// </summary>
    [JsonProperty("host", NullValueHandling = NullValueHandling.Ignore)]
    public string Host { get; private set; }

    /// <summary>
    /// Gets or sets the Password
    /// </summary>
    [JsonProperty("password", NullValueHandling = NullValueHandling.Ignore)]
    public string Password { get; private set; }

    /// <summary>
    /// Gets or sets the ShareStatus
    /// </summary>
    [JsonProperty("shareStatus", NullValueHandling = NullValueHandling.Ignore)]
    public string ShareStatus { get; private set; }

    /// <summary>
    /// Gets or sets the IsHost
    /// </summary>
    [JsonProperty("isHost", NullValueHandling = NullValueHandling.Ignore)]
    public bool IsHost { get; private set; }

    /// <summary>
    /// Gets or sets the IsSharingMeeting
    /// </summary>
    [JsonProperty("isSharingMeeting", NullValueHandling = NullValueHandling.Ignore)]
    public bool IsSharingMeeting { get; private set; }

    /// <summary>
    /// Gets or sets the WaitingForHost
    /// </summary>
    [JsonProperty("waitingForHost", NullValueHandling = NullValueHandling.Ignore)]
    public bool WaitingForHost { get; private set; }

    /// <summary>
    /// Gets or sets the IsLocked
    /// </summary>
    [JsonProperty("isLocked", NullValueHandling = NullValueHandling.Ignore)]
    public bool IsLocked { get; private set; }

    /// <summary>
    /// Gets or sets the IsRecording
    /// </summary>
    [JsonProperty("isRecording", NullValueHandling = NullValueHandling.Ignore)]
    public bool IsRecording { get; private set; }

    /// <summary>
    /// Gets or sets the CanRecord
    /// </summary>
    [JsonProperty("canRecord", NullValueHandling = NullValueHandling.Ignore)]
    public bool CanRecord { get; private set; }

    /// <summary>
    /// Constructor for MeetingInfo
    /// </summary>
    /// <param name="id">The unique identifier for the meeting</param>
    /// <param name="name">The name of the meeting</param>
    /// <param name="host">The host of the meeting</param>
    /// <param name="password">The password for the meeting</param>
    /// <param name="shareStatus">The share status of the meeting</param>
    /// <param name="isHost">Indicates whether the current user is the host</param>
    /// <param name="isSharingMeeting">Indicates whether the meeting is currently being shared</param>
    /// <param name="waitingForHost">Indicates whether the meeting is waiting for the host to join</param>
    /// <param name="isLocked">Indicates whether the meeting is locked</param>
    /// <param name="isRecording">Indicates whether the meeting is being recorded</param>
    /// <param name="canRecord">Indicates whether the meeting can be recorded</param>
    public MeetingInfo(string id, string name, string host, string password, string shareStatus, bool isHost, bool isSharingMeeting, bool waitingForHost, bool isLocked, bool isRecording, bool canRecord)
    {
      Id = id;
      Name = name;
      Host = host;
      Password = password;
      ShareStatus = shareStatus;
      IsHost = isHost;
      IsSharingMeeting = isSharingMeeting;
      WaitingForHost = waitingForHost;
      IsLocked = isLocked;
      IsRecording = isRecording;
      CanRecord = CanRecord;
    }
  }
}