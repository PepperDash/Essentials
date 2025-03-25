using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using PepperDash.Core;
using PepperDash.Essentials.Core.DeviceTypeInterfaces;
using PepperDash.Essentials.Devices.Common.Codec;
using System;
using System.Collections.Generic;

namespace PepperDash.Essentials.AppServer.Messengers
{
    public class IHasScheduleAwarenessMessenger : MessengerBase
    {
        public IHasScheduleAwareness ScheduleSource { get; private set; }

        public IHasScheduleAwarenessMessenger(string key, IHasScheduleAwareness scheduleSource, string messagePath)
            : base(key, messagePath, scheduleSource as Device)
        {
            ScheduleSource = scheduleSource ?? throw new ArgumentNullException("scheduleSource");
            ScheduleSource.CodecSchedule.MeetingsListHasChanged += new EventHandler<EventArgs>(CodecSchedule_MeetingsListHasChanged);
            ScheduleSource.CodecSchedule.MeetingEventChange += new EventHandler<MeetingEventArgs>(CodecSchedule_MeetingEventChange);
        }

#if SERIES4
        protected override void RegisterActions()
#else
        protected override void CustomRegisterWithAppServer(MobileControlSystemController appServerController)
#endif
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

    public class FullScheduleMessage : DeviceStateMessageBase
    {
        [JsonProperty("meetings", NullValueHandling = NullValueHandling.Ignore)]
        public List<Meeting> Meetings { get; set; }

        [JsonProperty("meetingWarningMinutes", NullValueHandling = NullValueHandling.Ignore)]
        public int MeetingWarningMinutes { get; set; }
    }

    public class MeetingChangeMessage
    {
        [JsonProperty("meetingChange", NullValueHandling = NullValueHandling.Ignore)]
        public MeetingChange MeetingChange { get; set; }
    }

    public class MeetingChange
    {
        [JsonProperty("changeType", NullValueHandling = NullValueHandling.Ignore)]
        public string ChangeType { get; set; }

        [JsonProperty("meeting", NullValueHandling = NullValueHandling.Ignore)]
        public Meeting Meeting { get; set; }
    }
}