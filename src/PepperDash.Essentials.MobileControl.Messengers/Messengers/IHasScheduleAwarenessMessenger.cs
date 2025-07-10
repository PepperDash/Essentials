using System;
using System.Collections.Generic;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using PepperDash.Core;
using PepperDash.Essentials.Devices.Common.Codec;

namespace PepperDash.Essentials.AppServer.Messengers
{
    /// <summary>
    /// Messenger for devices that implement IHasScheduleAwareness interface
    /// </summary>
    public class IHasScheduleAwarenessMessenger : MessengerBase
    {
        /// <summary>
        /// Gets the schedule source device
        /// </summary>
        public IHasScheduleAwareness ScheduleSource { get; private set; }

        /// <summary>
        /// Initializes a new instance of the IHasScheduleAwarenessMessenger class
        /// </summary>
        /// <param name="key">Unique identifier for the messenger</param>
        /// <param name="scheduleSource">Device that implements IHasScheduleAwareness</param>
        /// <param name="messagePath">Path for message routing</param>
        public IHasScheduleAwarenessMessenger(string key, IHasScheduleAwareness scheduleSource, string messagePath)
            : base(key, messagePath, scheduleSource as IKeyName)
        {
            ScheduleSource = scheduleSource ?? throw new ArgumentNullException("scheduleSource");
            ScheduleSource.CodecSchedule.MeetingsListHasChanged += new EventHandler<EventArgs>(CodecSchedule_MeetingsListHasChanged);
            ScheduleSource.CodecSchedule.MeetingEventChange += new EventHandler<MeetingEventArgs>(CodecSchedule_MeetingEventChange);
        }

        /// <inheritdoc />
        protected override void RegisterActions()
        {
            AddAction("/schedule/fullStatus", (id, content) => SendFullScheduleObject());
        }

        private void CodecSchedule_MeetingEventChange(object sender, MeetingEventArgs e)
        {
            PostStatusMessage(JToken.FromObject(new MeetingChangeMessage
            {
                MeetingChange = new MeetingChange
                {
                    ChangeType = e.ChangeType.ToString(),
                    Meeting = e.Meeting
                }
            })
            );
        }

        private void CodecSchedule_MeetingsListHasChanged(object sender, EventArgs e)
        {
            SendFullScheduleObject();
        }

        /// <summary>
        /// Helper method to send the full schedule data
        /// </summary>
        private void SendFullScheduleObject()
        {
            PostStatusMessage(new FullScheduleMessage
            {
                Meetings = ScheduleSource.CodecSchedule.Meetings,
                MeetingWarningMinutes = ScheduleSource.CodecSchedule.MeetingWarningMinutes
            });
        }
    }

    /// <summary>
    /// Full schedule message containing meetings and warning minutes
    /// </summary>
    public class FullScheduleMessage : DeviceStateMessageBase
    {
        /// <summary>
        /// Gets or sets the list of meetings
        /// </summary>
        [JsonProperty("meetings", NullValueHandling = NullValueHandling.Ignore)]
        public List<Meeting> Meetings { get; set; }

        /// <summary>
        /// Gets or sets the meeting warning minutes
        /// </summary>
        [JsonProperty("meetingWarningMinutes", NullValueHandling = NullValueHandling.Ignore)]
        public int MeetingWarningMinutes { get; set; }
    }

    /// <summary>
    /// Message containing meeting change information
    /// </summary>
    public class MeetingChangeMessage
    {
        /// <summary>
        /// Gets or sets the meeting change details
        /// </summary>
        [JsonProperty("meetingChange", NullValueHandling = NullValueHandling.Ignore)]
        public MeetingChange MeetingChange { get; set; }
    }

    /// <summary>
    /// Represents a meeting change with type and meeting details
    /// </summary>
    public class MeetingChange
    {
        /// <summary>
        /// Gets or sets the change type
        /// </summary>
        [JsonProperty("changeType", NullValueHandling = NullValueHandling.Ignore)]
        public string ChangeType { get; set; }

        /// <summary>
        /// Gets or sets the meeting details
        /// </summary>
        [JsonProperty("meeting", NullValueHandling = NullValueHandling.Ignore)]
        public Meeting Meeting { get; set; }
    }
}