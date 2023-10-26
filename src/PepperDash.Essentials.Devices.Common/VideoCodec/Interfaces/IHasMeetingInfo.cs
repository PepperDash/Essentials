extern alias Full;

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
}