extern alias Full;
using System;
using System.Collections.Generic;
using Full::Newtonsoft.Json;

namespace PepperDash.Essentials.Devices.Common.Codec
{
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
            Calls                    = new List<Call>();
            _joinableCooldownSeconds = 300;
        }

        public Meeting(int joinableCooldownSeconds)
        {
            Calls                    = new List<Call>();
            _joinableCooldownSeconds = joinableCooldownSeconds;
        }



        #region Overrides of Object

        public override string ToString()
        {
            return String.Format("{0}:{1}: {2}-{3}", Title, Agenda, StartTime, EndTime);
        }

        #endregion
    }
}