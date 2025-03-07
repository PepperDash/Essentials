using PepperDash.Essentials.Room.Config;
using System;
using System.Collections.Generic;

namespace PepperDash.Essentials.Core
{
    public interface IRoomEventSchedule
    {
        void AddOrUpdateScheduledEvent(ScheduledEventConfig eventConfig);

        List<ScheduledEventConfig> GetScheduledEvents();

        event EventHandler<ScheduledEventEventArgs> ScheduledEventsChanged;
    }

    public class ScheduledEventEventArgs : EventArgs
    {
        public List<ScheduledEventConfig> ScheduledEvents;
    }
}
