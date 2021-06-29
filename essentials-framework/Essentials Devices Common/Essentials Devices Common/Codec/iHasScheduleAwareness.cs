using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Crestron.SimplSharp;

using PepperDash.Core;

using Newtonsoft.Json;

namespace PepperDash.Essentials.Devices.Common.Codec
{
    public enum eMeetingEventChangeType
    {
        Unknown = 0,
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

        private Meeting _previousChangedMeeting;

        private eMeetingEventChangeType _previousChangeType = eMeetingEventChangeType.Unknown;

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

            const double meetingTimeEpsilon = 0.05;
            foreach (var m in Meetings)
            {
                var changeType = eMeetingEventChangeType.Unknown;

                //Debug.Console(2, "Math.Abs(m.TimeToMeetingEnd.TotalMinutes) = {0}", Math.Abs(m.TimeToMeetingEnd.TotalMinutes));
                if (_previousChangeType != eMeetingEventChangeType.MeetingStartWarning && m.TimeToMeetingStart.TotalMinutes <= m.MeetingWarningMinutes.TotalMinutes && m.TimeToMeetingStart.Seconds > 0)       // Meeting is about to start
                {
                    Debug.Console(2, "MeetingStartWarning. TotalMinutes: {0}  Seconds: {1}", m.TimeToMeetingStart.TotalMinutes, m.TimeToMeetingStart.Seconds);
                    changeType = eMeetingEventChangeType.MeetingStartWarning;
                }
                else if (_previousChangeType != eMeetingEventChangeType.MeetingStart && Math.Abs(m.TimeToMeetingEnd.TotalMinutes) < meetingTimeEpsilon)           // Meeting Start
                {
                    Debug.Console(2, "MeetingStart");
                    changeType = eMeetingEventChangeType.MeetingStart;
                }
                else if (_previousChangeType != eMeetingEventChangeType.MeetingEndWarning && m.TimeToMeetingEnd.TotalMinutes <= m.MeetingWarningMinutes.TotalMinutes && m.TimeToMeetingEnd.Seconds > 0)    // Meeting is about to end
                    changeType = eMeetingEventChangeType.MeetingEndWarning;
                else if (_previousChangeType != eMeetingEventChangeType.MeetingEnd && Math.Abs(m.TimeToMeetingEnd.TotalMinutes) < meetingTimeEpsilon)             // Meeting has ended
                    changeType = eMeetingEventChangeType.MeetingEnd;

                if (changeType != eMeetingEventChangeType.Unknown)
                {
                    // check to make sure this is not a redundant event for one that was fired last
                    if (_previousChangedMeeting == null || (_previousChangedMeeting != m && _previousChangeType != changeType))
                    {
                        _previousChangeType = changeType;
                        _previousChangedMeeting = m;

                        OnMeetingChange(changeType, m);
                    }
                }
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
                var joinable = StartTime.AddMinutes(-MinutesBeforeMeeting) <= DateTime.Now
                    && DateTime.Now <= EndTime; //.AddMinutes(-5);
                Debug.Console(2, "Meeting Id: {0} joinable: {1}", Id, joinable);
                return joinable;
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
