using PepperDash.Essentials.Room.Config;
using System.Collections.Generic;

namespace PepperDash.Essentials.Core.Room
{
    public interface IRoomEventSchedule
    {
        void AddOrUpdateScheduledEvent(ScheduledEventConfig eventConfig);

        List<ScheduledEventConfig> GetScheduledEvents();
    }
}
