using Newtonsoft.Json;
using PepperDash.Core;
using PepperDash.Essentials.Core;
using PepperDash.Essentials.Core.DeviceTypeInterfaces;
using PepperDash.Essentials.Room.Config;
using System;
using System.Collections.Generic;

namespace PepperDash.Essentials.AppServer.Messengers
{
    public class RoomEventScheduleMessenger : MessengerBase
    {
        private readonly IRoomEventSchedule _room;


        public RoomEventScheduleMessenger(string key, string messagePath, IRoomEventSchedule room)
            : base(key, messagePath, room as Device)
        {
            _room = room;
        }

        #region Overrides of MessengerBase

#if SERIES4
        protected override void RegisterActions()
#else
        protected override void CustomRegisterWithAppServer(MobileControlSystemController appServerController)
#endif
        {
            AddAction("/saveScheduledEvents", (id, content) => SaveScheduledEvents(content.ToObject<List<ScheduledEventConfig>>()));
            AddAction("/status", (id, content) =>
            {
                var events = _room.GetScheduledEvents();

                SendFullStatus(events);
            });

            _room.ScheduledEventsChanged += (sender, args) => SendFullStatus(args.ScheduledEvents);
        }

        #endregion

        private void SaveScheduledEvents(List<ScheduledEventConfig> events)
        {
            foreach (var evt in events)
            {
                SaveScheduledEvent(evt);
            }
        }

        private void SaveScheduledEvent(ScheduledEventConfig eventConfig)
        {
            try
            {
                _room.AddOrUpdateScheduledEvent(eventConfig);
            }
            catch (Exception ex)
            {
                Debug.Console(0, this, "Exception saving event: {0}\r\n{1}", ex.Message, ex.StackTrace);
            }
        }

        private void SendFullStatus(List<ScheduledEventConfig> events)
        {

            var message = new RoomEventScheduleStateMessage
            {
                ScheduleEvents = events,
            };

            PostStatusMessage(message);
        }
    }

    public class RoomEventScheduleStateMessage : DeviceStateMessageBase
    {
        [JsonProperty("scheduleEvents")]
        public List<ScheduledEventConfig> ScheduleEvents { get; set; }
    }
}