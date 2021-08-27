using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Crestron.SimplSharp;

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
        public string Id { get; private set; }
        public string Name { get; private set; }
        public string Host { get; set; }
        public string Password { get; private set; }
        public string ShareStatus { get; private set; }

        public MeetingInfo(string id, string name, string host, string password, string shareStatus)
        {
            Id = id;
            Name = name;
            Host = host;
            Password = password;
            ShareStatus = shareStatus;
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