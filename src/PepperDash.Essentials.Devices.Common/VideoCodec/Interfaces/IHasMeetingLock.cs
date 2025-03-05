using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Crestron.SimplSharp;
using PepperDash.Essentials.Core;

namespace PepperDash.Essentials.Devices.Common.VideoCodec.Interfaces
{
    public interface IHasMeetingLock
    {
        BoolFeedback MeetingIsLockedFeedback { get; }

        void LockMeeting();
        void UnLockMeeting();
        void ToggleMeetingLock();
    }
}