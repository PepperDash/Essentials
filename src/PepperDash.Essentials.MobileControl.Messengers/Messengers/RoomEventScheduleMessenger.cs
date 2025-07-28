using Newtonsoft.Json;
using PepperDash.Core;
using PepperDash.Core.Logging;
using PepperDash.Essentials.Core;
using System;
using System.Collections.Generic;

namespace PepperDash.Essentials.AppServer.Messengers
{
    /// <summary>
    /// Represents a RoomEventScheduleMessenger
    /// </summary>
    public class RoomEventScheduleMessenger : MessengerBase
    {
        private readonly IRoomEventSchedule _room;


        public RoomEventScheduleMessenger(string key, string messagePath, IRoomEventSchedule room)
            : base(key, messagePath, room as IKeyName)
        {
            _room = room;
        }

        #region Overrides of MessengerBase

        protected override void RegisterActions()
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
                this.LogException(ex,"Exception saving event");
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

    /// <summary>
    /// Represents a RoomEventScheduleStateMessage
    /// </summary>
    public class RoomEventScheduleStateMessage : DeviceStateMessageBase
    {
        [JsonProperty("scheduleEvents")]
        /// <summary>
        /// Gets or sets the ScheduleEvents
        /// </summary>
        public List<ScheduledEventConfig> ScheduleEvents { get; set; }
    }
}