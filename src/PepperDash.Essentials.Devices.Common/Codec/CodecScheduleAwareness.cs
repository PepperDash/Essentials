using System;
using System.Collections.Generic;
using Crestron.SimplSharp;
using PepperDash.Core;

namespace PepperDash.Essentials.Devices.Common.Codec
{
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
}