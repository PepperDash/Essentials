using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Crestron.SimplSharp;

namespace PepperDash.Essentials.Devices.Common.Codec
{
    public interface IHasScheduleAwareness
    {
        CodecScheduleAwareness CodecSchedule { get; }
    }

    public class CodecScheduleAwareness
    {
        public List<Meeting> Meetings { get; set; }

        public CodecScheduleAwareness()
        {
            Meetings = new List<Meeting>();
        }
    }

    /// <summary>
    /// Generic class to represent a meeting (Cisco or Polycom OBTP or Fusion)
    /// </summary>
    public class Meeting
    {
        public string Id { get; set; }
        public string Organizer { get; set; }
        public string Title { get; set; }
        public string Agenda { get; set; }
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
                var timeToMeetingStart = StartTime - DateTime.Now;

                var timetoMeetingEnd = DateTime.Now - EndTime;

                // Meeting is joinable from 5 minutes before start until 5 minutes before end
                if (timeToMeetingStart.Minutes <= 5 && timetoMeetingEnd.Minutes >= 5)
                    return true;
                else
                    return false;
            }
        }
        public string ConferenceNumberToDial { get; set; }
        public string ConferencePassword { get; set; }
    }
}