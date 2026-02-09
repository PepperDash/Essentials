using PepperDash.Essentials.Room.Config;
using System;
using System.Collections.Generic;

namespace PepperDash.Essentials.Core
{
    /// <summary>
    /// Defines the contract for IRoomEventSchedule
    /// </summary>
    public interface IRoomEventSchedule
    {
        /// <summary>
        /// Adds or updates a scheduled event
        /// </summary>
        /// <param name="eventConfig"></param>
        void AddOrUpdateScheduledEvent(ScheduledEventConfig eventConfig);

        /// <summary>
        /// Removes a scheduled event by its key
        /// </summary>
        /// <returns></returns>
        List<ScheduledEventConfig> GetScheduledEvents();

        /// <summary>
        /// Removes a scheduled event by its key
        /// </summary>
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
