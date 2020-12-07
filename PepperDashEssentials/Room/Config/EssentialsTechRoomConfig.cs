using System.Collections.Generic;

namespace PepperDash.Essentials.Room.Config
{
    public class EssentialsTechRoomConfig
    {
        public List<string> Displays;
        public List<string> Tuners;
        public string ScheduleProviderKey;
        public string UserPin;
        public string TechPin;
        public string PresetsFileName;
        public List<ScheduledEventConfig> RoomScheduledEvents;

        public EssentialsTechRoomConfig()
        {
            Displays = new List<string>();
            Tuners = new List<string>();
            RoomScheduledEvents = new List<ScheduledEventConfig>();
        }
    }
}