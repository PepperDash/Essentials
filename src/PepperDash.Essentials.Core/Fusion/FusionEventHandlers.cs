using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Crestron.SimplSharp;

namespace PepperDash.Essentials.Core.Fusion
{
    /// <summary>
    /// Represents a ScheduleChangeEventArgs
    /// </summary>
    public class ScheduleChangeEventArgs : EventArgs
    {
        /// <summary>
        /// Gets or sets the Schedule
        /// </summary>
        public RoomSchedule Schedule { get; set; }
    }

    /// <summary>
    /// Represents a MeetingChangeEventArgs
    /// </summary>
    public class MeetingChangeEventArgs : EventArgs
    {
        /// <summary>
        /// Gets or sets the Meeting
        /// </summary>
        public Event Meeting { get; set; }
    }
}