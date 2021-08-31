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
        [JsonProperty("id")]
        public string Id { get; private set; }
        [JsonProperty("name")]
        public string Name { get; private set; }
        [JsonProperty("host")]
        public string Host { get; private set; }
        [JsonProperty("password")]
        public string Password { get; private set; }
        [JsonProperty("shareStatus")]
        public string ShareStatus { get; private set; }
        [JsonProperty("isHost")]
        public Boolean IsHost { get; private set; }

        public MeetingInfo(string id, string name, string host, string password, string shareStatus, bool isHost)
        {
            Id = id;
            Name = name;
            Host = host;
            Password = password;
            ShareStatus = shareStatus;
            IsHost = isHost;
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