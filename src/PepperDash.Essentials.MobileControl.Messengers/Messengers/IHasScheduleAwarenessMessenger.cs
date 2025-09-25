using System;
using System.Collections.Generic;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using PepperDash.Core;
using PepperDash.Essentials.Devices.Common.Codec;

namespace PepperDash.Essentials.AppServer.Messengers
{
    /// <summary>
    /// Represents a IHasScheduleAwarenessMessenger
    /// </summary>
    public class IHasScheduleAwarenessMessenger : MessengerBase
    {
        /// <summary>
        /// Gets or sets the ScheduleSource
        /// </summary>
        public IHasScheduleAwareness ScheduleSource { get; private set; }

        public IHasScheduleAwarenessMessenger(string key, IHasScheduleAwareness scheduleSource, string messagePath)
            : base(key, messagePath, scheduleSource as IKeyName)
        {
            ScheduleSource = scheduleSource ?? throw new ArgumentNullException("scheduleSource");
            ScheduleSource.CodecSchedule.MeetingsListHasChanged += new EventHandler<EventArgs>(CodecSchedule_MeetingsListHasChanged);
            ScheduleSource.CodecSchedule.MeetingEventChange += new EventHandler<MeetingEventArgs>(CodecSchedule_MeetingEventChange);
        }

        protected override void RegisterActions()
        {
            AddAction("/schedule/fullStatus", (id, content) => SendFullScheduleObject(id));

            AddAction("/schedule/status", (id, content) => SendFullScheduleObject(id));
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
        private void SendFullScheduleObject(string id = null)
        {
            PostStatusMessage(new FullScheduleMessage
            {
                Meetings = ScheduleSource.CodecSchedule.Meetings,
                MeetingWarningMinutes = ScheduleSource.CodecSchedule.MeetingWarningMinutes
            }, id);
        }
    }

    /// <summary>
    /// Represents a FullScheduleMessage
    /// </summary>
    public class FullScheduleMessage : DeviceStateMessageBase
    {

        /// <summary>
        /// Gets or sets the Meetings
        /// </summary>
        [JsonProperty("meetings", NullValueHandling = NullValueHandling.Ignore)]
        public List<Meeting> Meetings { get; set; }


        /// <summary>
        /// Gets or sets the MeetingWarningMinutes
        /// </summary>
        [JsonProperty("meetingWarningMinutes", NullValueHandling = NullValueHandling.Ignore)]
        public int MeetingWarningMinutes { get; set; }
    }

    /// <summary>
    /// Represents a MeetingChangeMessage
    /// </summary>
    public class MeetingChangeMessage
    {

        /// <summary>
        /// Gets or sets the MeetingChange
        /// </summary>
        [JsonProperty("meetingChange", NullValueHandling = NullValueHandling.Ignore)]
        public MeetingChange MeetingChange { get; set; }
    }

    /// <summary>
    /// Represents a MeetingChange
    /// </summary>
    public class MeetingChange
    {

        /// <summary>
        /// Gets or sets the ChangeType
        /// </summary>
        [JsonProperty("changeType", NullValueHandling = NullValueHandling.Ignore)]
        public string ChangeType { get; set; }


        /// <summary>
        /// Gets or sets the Meeting
        /// </summary>
        [JsonProperty("meeting", NullValueHandling = NullValueHandling.Ignore)]
        public Meeting Meeting { get; set; }
    }
}