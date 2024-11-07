using System;
using System.Collections.Generic;
using PepperDash.Essentials.Core.Room.Config;

namespace PepperDash.Essentials.Core.Room
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
