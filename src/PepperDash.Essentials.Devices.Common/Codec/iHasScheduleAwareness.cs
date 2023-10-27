extern alias Full;

using System;
using System.Collections.Generic;
using Crestron.SimplSharp;

using PepperDash.Core;

using Full.Newtonsoft.Json;

namespace PepperDash.Essentials.Devices.Common.Codec
{
    [Flags]
    public enum eMeetingEventChangeType
    {
        Unknown = 0,
        MeetingStartWarning = 1,
        MeetingStart = 2,
        MeetingEndWarning = 4,
        MeetingEnd = 8
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

        /// <summary>
        /// Helper method to fire MeetingEventChange.  Should only fire once for each changeType on each meeting
        /// </summary>
        /// <param name="changeType"></param>
        /// <param name="meeting"></param>
        private void OnMeetingChange(eMeetingEventChangeType changeType, Meeting meeting)
        {
            Debug.Console(2, "*****************OnMeetingChange.  id: {0} changeType: {1}**********************", meeting.Id, changeType);
            if (changeType != (changeType & meeting.NotifiedChangeTypes))
            {
                // Add this change type to the NotifiedChangeTypes
                meeting.NotifiedChangeTypes |= changeType;

                var handler = MeetingEventChange;
                if (handler != null)
                {
                    handler(this, new MeetingEventArgs() { ChangeType = changeType, Meeting = meeting });
                }
            }
            else
            {
                Debug.Console(2, "Meeting: {0} already notified of changeType: {1}", meeting.Id, changeType);
            }
        }


        /// <summary>
        /// Checks the schedule to see if any MeetingEventChange updates should be fired
        /// </summary>
        /// <param name="o"></param>
        private void CheckSchedule(object o)
        {
            //  Iterate the meeting list and check if any meeting need to do anything

            const double meetingTimeEpsilon = 0.05;
            foreach (var m in Meetings)
            {
                var changeType = eMeetingEventChangeType.Unknown;

                if (eMeetingEventChangeType.MeetingStartWarning != (m.NotifiedChangeTypes & eMeetingEventChangeType.MeetingStartWarning) && m.TimeToMeetingStart.TotalMinutes <= m.MeetingWarningMinutes.TotalMinutes && m.TimeToMeetingStart.Seconds > 0)       // Meeting is about to start
                {
                    Debug.Console(2, "********************* MeetingStartWarning. TotalMinutes: {0}  Seconds: {1}", m.TimeToMeetingStart.TotalMinutes, m.TimeToMeetingStart.Seconds);
                    changeType = eMeetingEventChangeType.MeetingStartWarning;
                }
                else if (eMeetingEventChangeType.MeetingStart != (m.NotifiedChangeTypes & eMeetingEventChangeType.MeetingStart) && Math.Abs(m.TimeToMeetingStart.TotalMinutes) < meetingTimeEpsilon)           // Meeting Start
                {
                    Debug.Console(2, "********************* MeetingStart");
                    changeType = eMeetingEventChangeType.MeetingStart;
                }
                else if (eMeetingEventChangeType.MeetingEndWarning != (m.NotifiedChangeTypes & eMeetingEventChangeType.MeetingEndWarning) && m.TimeToMeetingEnd.TotalMinutes <= m.MeetingWarningMinutes.TotalMinutes && m.TimeToMeetingEnd.Seconds > 0)    // Meeting is about to end
                {
                    Debug.Console(2, "********************* MeetingEndWarning. TotalMinutes: {0}  Seconds: {1}", m.TimeToMeetingEnd.TotalMinutes, m.TimeToMeetingEnd.Seconds);
                    changeType = eMeetingEventChangeType.MeetingEndWarning;
                }
                else if (eMeetingEventChangeType.MeetingEnd != (m.NotifiedChangeTypes & eMeetingEventChangeType.MeetingEnd) && Math.Abs(m.TimeToMeetingEnd.TotalMinutes) < meetingTimeEpsilon)             // Meeting has ended
                {
                    Debug.Console(2, "********************* MeetingEnd");
                    changeType = eMeetingEventChangeType.MeetingEnd;
                }

                if (changeType != eMeetingEventChangeType.Unknown)
                {
                    OnMeetingChange(changeType, m);                    
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
                    && DateTime.Now <= EndTime.AddSeconds(-_joinableCooldownSeconds);
                //Debug.Console(2, "Meeting Id: {0} joinable: {1}", Id, joinable);
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

        /// <summary>
        /// Tracks the change types that have already been notified for
        /// </summary>
        [JsonIgnore]
        public eMeetingEventChangeType NotifiedChangeTypes { get; set; }

        [JsonIgnore] private readonly int _joinableCooldownSeconds;


        public Meeting()
        {
            Calls = new List<Call>();
            _joinableCooldownSeconds = 300;
        }

        public Meeting(int joinableCooldownSeconds)
        {
            Calls = new List<Call>();
            _joinableCooldownSeconds = joinableCooldownSeconds;
        }



        #region Overrides of Object

        public override string ToString()
        {
            return String.Format("{0}:{1}: {2}-{3}", Title, Agenda, StartTime, EndTime);
        }

        #endregion
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
