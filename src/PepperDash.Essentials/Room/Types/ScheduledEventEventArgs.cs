using System;
using System.Collections.Generic;
using PepperDash.Essentials.Room.Config;

namespace PepperDash.Essentials
{
    public class ScheduledEventEventArgs : EventArgs
    {
        public List<ScheduledEventConfig> ScheduledEvents;
    }
}