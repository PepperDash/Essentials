using PepperDash.Essentials.Room.Config;
using System.Collections.Generic;

namespace PepperDash.Essentials.Core
{
    public interface IRoomEventSchedule
    {
        void AddOrUpdateScheduledEvent(ScheduledEventConfig eventConfig);

        List<ScheduledEventConfig> GetScheduledEvents();
    }
}
