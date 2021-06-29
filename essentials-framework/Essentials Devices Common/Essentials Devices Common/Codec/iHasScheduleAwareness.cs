using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Crestron.SimplSharp;

using Newtonsoft.Json;

namespace PepperDash.Essentials.Devices.Common.Codec
{
    public enum eMeetingEventChangeType
    {
        Unkown = 0,
        MeetingStartWarning,
        MeetingStart,
        MeetingEndWarning,
        MeetingEnd
    }

    public interface IHasScheduleAwareness
    {
        CodecScheduleAwareness CodecSchedule { get; }

        void GetSchedule();
    }

    public class CodecScheduleAwareness
    {
        List<Meeting> _meetings;

        public event EventHandler<MeetingEventArgs> MeetingEventChange;

        public event EventHandler<EventArgs> MeetingsListHasChanged;

        private int _meetingWarningMinutes = 5;

        public int MeetingWarningMinutes
        {
            get { return _meetingWarningMinutes; }
            set { _meetingWarningMinutes = value; }
        }

        /// <summary>
		/// Setter triggers MeetingsListHasChanged event
		/// </summary>
        public List<Meeting> Meetings
        {
            get
            {
                return _meetings;
            }
            set
            {
                _meetings = value;

                var handler = MeetingsListHasChanged;
                if (handler != null)
                {
                    handler(this, new EventArgs());
                }
            }
        }

        private CTimer _scheduleChecker;

        public CodecScheduleAwareness()
        {
            Meetings = new List<Meeting>();

            _scheduleChecker = new CTimer(CheckSchedule, null, 1000, 1000);
        }

        public CodecScheduleAwareness(long pollTime)
        {
            Meetings = new List<Meeting>();

            _scheduleChecker = new CTimer(CheckSchedule, null, pollTime, pollTime);
        }

        private void OnMeetingChange(eMeetingEventChangeType changeType, Meeting meeting)
        {
            var handler = MeetingEventChange;
            if (handler != null)
            {
                handler(this, new MeetingEventArgs() { ChangeType = changeType, Meeting = meeting });
            }
        }

        private void CheckSchedule(object o)
        {
            //  Iterate the meeting list and check if any meeting need to do anythingk

            const double meetingTimeEpsilon = 0.0001;
            foreach (var m in Meetings)
            {
                var changeType = eMeetingEventChangeType.Unkown;

                if (m.TimeToMeetingStart.TotalMinutes <= m.MeetingWarningMinutes.TotalMinutes)       // Meeting is about to start
                    changeType = eMeetingEventChangeType.MeetingStartWarning;
                else if (Math.Abs(m.TimeToMeetingStart.TotalMinutes) < meetingTimeEpsilon)           // Meeting Start
                    changeType = eMeetingEventChangeType.MeetingStart;
                else if (m.TimeToMeetingEnd.TotalMinutes <= m.MeetingWarningMinutes.TotalMinutes)    // Meeting is about to end
                    changeType = eMeetingEventChangeType.MeetingEndWarning;
                else if (Math.Abs(m.TimeToMeetingEnd.TotalMinutes) < meetingTimeEpsilon)             // Meeting has ended
                    changeType = eMeetingEventChangeType.MeetingEnd;

                if (changeType != eMeetingEventChangeType.Unkown)
                    OnMeetingChange(changeType, m);
            }


        }
    }

    /// <summary>
    /// Generic class to represent a meeting (Cisco or Polycom OBTP or Fusion)
    /// </summary>
    public class Meeting
    {
        [JsonProperty("minutesBeforeMeeting")]
        public int MinutesBeforeMeeting;

        [JsonProperty("id")]
        public string Id { get; set; }
        [JsonProperty("organizer")]
        public string Organizer { get; set; }
        [JsonProperty("title")]
        public string Title { get; set; }
        [JsonProperty("agenda")]
        public string Agenda { get; set; }

        [JsonProperty("meetingWarningMinutes")]
        public TimeSpan MeetingWarningMinutes
        {
            get { return TimeSpan.FromMinutes(MinutesBeforeMeeting); }
        }
        [JsonProperty("timeToMeetingStart")]
        public TimeSpan TimeToMeetingStart
        {
            get
            {
                return StartTime - DateTime.Now;
            }
        }
        [JsonProperty("timeToMeetingEnd")]
        public TimeSpan TimeToMeetingEnd
        {
            get
            {
                return EndTime - DateTime.Now;
            }
        }
        [JsonProperty("startTime")]
        public DateTime StartTime { get; set; }
        [JsonProperty("endTime")]
        public DateTime EndTime { get; set; }
        [JsonProperty("duration")]
        public TimeSpan Duration
        {
            get
            {
                return EndTime - StartTime;
            }
        }
        [JsonProperty("privacy")]
        public eMeetingPrivacy Privacy { get; set; }
        [JsonProperty("joinable")]
        public bool Joinable
        {
            get
            {
                return StartTime.AddMinutes(-MinutesBeforeMeeting) <= DateTime.Now
                    && DateTime.Now <= EndTime; //.AddMinutes(-5);
            }
        }
        //public string ConferenceNumberToDial { get; set; }
        [JsonProperty("conferencePassword")]
        public string ConferencePassword { get; set; }
        [JsonProperty("isOneButtonToPushMeeting")]
        public bool IsOneButtonToPushMeeting { get; set; }

        [JsonProperty("calls")]
        public List<Call> Calls { get; private set; }

        public Meeting()
        {
            Calls = new List<Call>();
        }
    }

    public class Call
    {
        public string Number { get; set; }
        public string Protocol { get; set; }
        public string CallRate { get; set; }
        public string CallType { get; set; }
    }

    public class MeetingEventArgs : EventArgs
    {
        public eMeetingEventChangeType ChangeType { get; set; }
        public Meeting Meeting { get; set; }
    }

}
