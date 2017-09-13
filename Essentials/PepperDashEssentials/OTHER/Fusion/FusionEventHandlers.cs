using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Crestron.SimplSharp;

namespace PepperDash.Essentials.Fusion
{
    public class ScheduleChangeEventArgs : EventArgs
    {
        public RoomSchedule Schedule { get; set; }
    }

    public class MeetingChangeEventArgs : EventArgs
    {
        public Event Meeting { get; set; }
    }
}