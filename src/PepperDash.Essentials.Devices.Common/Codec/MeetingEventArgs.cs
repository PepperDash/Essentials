using System;

namespace PepperDash.Essentials.Devices.Common.Codec
{
    public class MeetingEventArgs : EventArgs
    {
        public eMeetingEventChangeType ChangeType { get; set; }
        public Meeting Meeting { get; set; }
    }
}