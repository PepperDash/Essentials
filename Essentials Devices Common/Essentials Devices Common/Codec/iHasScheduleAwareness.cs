using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Crestron.SimplSharp;

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
    }

    public class CodecScheduleAwareness
    {
        List<Meeting> _Meetings;

        public event EventHandler<MeetingEventArgs> MeetingEventChange;

        public event EventHandler<EventArgs> MeetingsListHasChanged;

        public List<Meeting> Meetings
        {
            get
            {
                return _Meetings;
            }
            set
            {
                _Meetings = value;

                var handler = MeetingsListHasChanged;
                if (handler != null)
                {
                    handler(this, new EventArgs());
                }
            }
        }

        private CTimer ScheduleChecker;

        public CodecScheduleAwareness()
        {
            Meetings = new List<Meeting>();

            ScheduleChecker = new CTimer(CheckSchedule, null, 1000, 1000);
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

            foreach (Meeting m in Meetings)
            {
                eMeetingEventChangeType changeType = eMeetingEventChangeType.Unkown;

                if (m.TimeToMeetingStart.TotalMinutes == m.MeetingWarningMinutes.TotalMinutes)       // Meeting is about to start
                    changeType = eMeetingEventChangeType.MeetingStartWarning;
                else if (m.TimeToMeetingStart.TotalMinutes == 0)                                    // Meeting Start
                    changeType = eMeetingEventChangeType.MeetingStart;
                else if (m.TimeToMeetingEnd.TotalMinutes == m.MeetingWarningMinutes.TotalMinutes)    // Meeting is about to end
                    changeType = eMeetingEventChangeType.MeetingEndWarning;
                else if (m.TimeToMeetingEnd.TotalMinutes == 0)                                      // Meeting has ended
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
        public TimeSpan MeetingWarningMinutes = TimeSpan.FromMinutes(5);

        public string Id { get; set; }
        public string Organizer { get; set; }
        public string Title { get; set; }
        public string Agenda { get; set; }
        public TimeSpan TimeToMeetingStart
        {
            get
            {
                return StartTime - DateTime.Now;
            }
        }
        public TimeSpan TimeToMeetingEnd
        {
            get
            {
                return EndTime - DateTime.Now;
            }
        }
        public DateTime StartTime { get; set; }
        public DateTime EndTime { get; set; }
        public TimeSpan Duration
        {
            get
            {
                return EndTime - StartTime;
            }
        }
        public eMeetingPrivacy Privacy { get; set; }
        public bool Joinable
        {
            get
            {
                return StartTime.AddMinutes(-5) <= DateTime.Now
                    && DateTime.Now <= EndTime.AddMinutes(-5);
            }
        }
        public string ConferenceNumberToDial { get; set; }
        public string ConferencePassword { get; set; }
    }

    public class MeetingEventArgs : EventArgs
    {
        public eMeetingEventChangeType ChangeType { get; set; }
        public Meeting Meeting { get; set; }
    }

}
