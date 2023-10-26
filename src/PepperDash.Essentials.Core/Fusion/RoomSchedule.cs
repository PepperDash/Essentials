using System.Collections.Generic;

namespace PepperDash.Essentials.Core.Fusion
{
    public class RoomSchedule
    {
        public List<Event> Meetings { get; set; }

        public RoomSchedule()
        {
            Meetings = new List<Event>();
        }
    }
}