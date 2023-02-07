using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Crestron.SimplSharp;

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
    /// Represents the information about a meeting in progress
    /// Currently used for Zoom meetings
    /// </summary>
    public class MeetingInfo
    {
        [JsonProperty("id", NullValueHandling = NullValueHandling.Ignore)]
        public string Id { get; private set; }
        [JsonProperty("name", NullValueHandling = NullValueHandling.Ignore)]
        public string Name { get; private set; }
        [JsonProperty("host", NullValueHandling = NullValueHandling.Ignore)]
        public string Host { get; private set; }
        [JsonProperty("password", NullValueHandling = NullValueHandling.Ignore)]
        public string Password { get; private set; }
        [JsonProperty("shareStatus", NullValueHandling = NullValueHandling.Ignore)]
        public string ShareStatus { get; private set; }
        [JsonProperty("isHost", NullValueHandling = NullValueHandling.Ignore)]
        public Boolean IsHost { get; private set; }
        [JsonProperty("isSharingMeeting", NullValueHandling = NullValueHandling.Ignore)]
        public Boolean IsSharingMeeting { get; private set; }
        [JsonProperty("waitingForHost", NullValueHandling = NullValueHandling.Ignore)]
        public Boolean WaitingForHost { get; private set; }
        [JsonProperty("isLocked", NullValueHandling = NullValueHandling.Ignore)]
        public Boolean IsLocked { get; private set; }
        [JsonProperty("isRecording", NullValueHandling = NullValueHandling.Ignore)]
        public Boolean IsRecording { get; private set; }
        [JsonProperty("canRecord", NullValueHandling = NullValueHandling.Ignore)]
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

    public class MeetingInfoEventArgs : EventArgs
    {
        public MeetingInfo Info { get; private set; }

        public MeetingInfoEventArgs(MeetingInfo info)
        {
            Info = info;
        }

    }
}