using PepperDash.Essentials.Core.Rooms.Config;
using System;
using System.Collections.Generic;

namespace PepperDash.Essentials.Core.Rooms
{
    /// <summary>
    /// Defines the contract for IRoomEventSchedule
    /// </summary>
    public interface IRoomEventSchedule
    {
        void AddOrUpdateScheduledEvent(ScheduledEventConfig eventConfig);

        List<ScheduledEventConfig> GetScheduledEvents();

        event EventHandler<ScheduledEventEventArgs> ScheduledEventsChanged;
    }

    /// <summary>
    /// Represents a ScheduledEventEventArgs
    /// </summary>
    public class ScheduledEventEventArgs : EventArgs
    {
        /// <summary>
        /// Gets or sets the ScheduledEvents
        /// </summary>
        public List<ScheduledEventConfig> ScheduledEvents;
    }
}
