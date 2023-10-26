using System;

namespace PepperDash.Essentials.Devices.Common.VideoCodec.Interfaces
{
    public class MeetingInfoEventArgs : EventArgs
    {
        public MeetingInfo Info { get; private set; }

        public MeetingInfoEventArgs(MeetingInfo info)
        {
            Info = info;
        }

    }
}