

using System;

using Newtonsoft.Json;

namespace PepperDash.Essentials.Devices.Common.VideoCodec.Interfaces
{
    /// <summary>
    /// Describes a device that provides meeting information (like a ZoomRoom)
    /// </summary>
    public interface IHasMeetingInfo
    {
        event EventHandler<MeetingInfoEventArgs> MeetingInfoChanged;

        MeetingInfo MeetingInfo { get; }
    }

    /// <summary>
    /// Represents a MeetingInfo
    /// </summary>
    public class MeetingInfo
    {
        [JsonProperty("id", NullValueHandling = NullValueHandling.Ignore)]
        /// <summary>
        /// Gets or sets the Id
        /// </summary>
        public string Id { get; private set; }
        [JsonProperty("name", NullValueHandling = NullValueHandling.Ignore)]
        public string Name { get; private set; }
        [JsonProperty("host", NullValueHandling = NullValueHandling.Ignore)]
        public string Host { get; private set; }
        [JsonProperty("password", NullValueHandling = NullValueHandling.Ignore)]
        public string Password { get; private set; }
        [JsonProperty("shareStatus", NullValueHandling = NullValueHandling.Ignore)]
        /// <summary>
        /// Gets or sets the ShareStatus
        /// </summary>
        public string ShareStatus { get; private set; }
        [JsonProperty("isHost", NullValueHandling = NullValueHandling.Ignore)]
        /// <summary>
        /// Gets or sets the IsHost
        /// </summary>
        public Boolean IsHost { get; private set; }
        [JsonProperty("isSharingMeeting", NullValueHandling = NullValueHandling.Ignore)]
        /// <summary>
        /// Gets or sets the IsSharingMeeting
        /// </summary>
        public Boolean IsSharingMeeting { get; private set; }
        [JsonProperty("waitingForHost", NullValueHandling = NullValueHandling.Ignore)]
        /// <summary>
        /// Gets or sets the WaitingForHost
        /// </summary>
        public Boolean WaitingForHost { get; private set; }
        [JsonProperty("isLocked", NullValueHandling = NullValueHandling.Ignore)]
        /// <summary>
        /// Gets or sets the IsLocked
        /// </summary>
        public Boolean IsLocked { get; private set; }
        [JsonProperty("isRecording", NullValueHandling = NullValueHandling.Ignore)]
        /// <summary>
        /// Gets or sets the IsRecording
        /// </summary>
        public Boolean IsRecording { get; private set; }
        [JsonProperty("canRecord", NullValueHandling = NullValueHandling.Ignore)]
        /// <summary>
        /// Gets or sets the CanRecord
        /// </summary>
        public Boolean CanRecord { get; private set; }


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

    /// <summary>
    /// Represents a MeetingInfoEventArgs
    /// </summary>
    public class MeetingInfoEventArgs : EventArgs
    {
        /// <summary>
        /// Gets or sets the Info
        /// </summary>
        public MeetingInfo Info { get; private set; }

        public MeetingInfoEventArgs(MeetingInfo info)
        {
            Info = info;
        }

    }
}