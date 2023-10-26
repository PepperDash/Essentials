using System;

namespace PepperDash.Essentials.Devices.Common.Codec
{
    [Flags]
    public enum eMeetingEventChangeType
    {
        Unknown = 0,
        MeetingStartWarning = 1,
        MeetingStart = 2,
        MeetingEndWarning = 4,
        MeetingEnd = 8
    }
}