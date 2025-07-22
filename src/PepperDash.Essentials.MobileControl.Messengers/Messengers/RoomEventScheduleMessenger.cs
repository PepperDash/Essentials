using System;
using System.Collections.Generic;
using Newtonsoft.Json;
using PepperDash.Core;
using PepperDash.Core.Logging;
using PepperDash.Essentials.Core;
using PepperDash.Essentials.Room.Config;

namespace PepperDash.Essentials.AppServer.Messengers
{
    /// <summary>
    /// Messenger for managing scheduled events in a room.
    /// This class handles saving scheduled events and sending the current schedule state to clients.
    /// It listens for changes in the scheduled events and updates clients accordingly.
    /// </summary>
    public class RoomEventScheduleMessenger : MessengerBase
    {
        private readonly IRoomEventSchedule _room;

        /// <summary>
        /// Initializes a new instance of the <see cref="RoomEventScheduleMessenger"/> class.
        /// This constructor sets up the messenger with a unique key, message path, and the room event schedule interface.
        /// It registers actions for saving scheduled events and sending the current schedule state.
        /// </summary>
        /// <param name="key">Unique identifier for the messenger</param>
        /// <param name="messagePath">Path for message routing</param>
        /// <param name="room">Room event schedule interface</param>
        public RoomEventScheduleMessenger(string key, string messagePath, IRoomEventSchedule room)
            : base(key, messagePath, room as IKeyName)
        {
            _room = room;
        }

        #region Overrides of MessengerBase

        /// <inheritdoc />
        protected override void RegisterActions()
        {
            AddAction("/saveScheduledEvents", (id, content) => SaveScheduledEvents(content.ToObject<List<ScheduledEventConfig>>()));
            AddAction("/status", (id, content) =>
            {
                var events = _room.GetScheduledEvents();

                SendFullStatus(events, id);
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
                this.LogException(ex, "Exception saving event");
            }
        }

        private void SendFullStatus(List<ScheduledEventConfig> events, string id = null)
        {

            var message = new RoomEventScheduleStateMessage
            {
                ScheduleEvents = events,
            };

            PostStatusMessage(message, id);
        }
    }

    /// <summary>
    /// Represents the state message for room event schedules.
    /// This message contains a list of scheduled events configured for the room.
    /// It is used to communicate the current schedule state to clients.
    /// </summary>
    public class RoomEventScheduleStateMessage : DeviceStateMessageBase
    {
        /// <summary>
        /// Gets or sets the list of scheduled events for the room.
        /// This property contains the configuration of scheduled events that are set up in the room.
        /// Each event includes details such as the event name, start time, end time, and recurrence pattern.
        /// </summary>
        [JsonProperty("scheduleEvents")]
        public List<ScheduledEventConfig> ScheduleEvents { get; set; }
    }
}