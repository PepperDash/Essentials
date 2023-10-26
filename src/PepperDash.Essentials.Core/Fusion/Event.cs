using System;

namespace PepperDash.Essentials.Core.Fusion
{
    public class Event
    {
        //[XmlElement(ElementName = "MeetingID")]
        public string MeetingID { get; set; }
        //[XmlElement(ElementName = "RVMeetingID")]
        public string RVMeetingID { get; set; }
        //[XmlElement(ElementName = "Recurring")]
        public string Recurring { get; set; }
        //[XmlElement(ElementName = "InstanceID")]
        public string InstanceID { get; set; }
        //[XmlElement(ElementName = "dtStart")]
        public DateTime dtStart { get; set; }
        //[XmlElement(ElementName = "dtEnd")]
        public DateTime dtEnd { get; set; }
        //[XmlElement(ElementName = "Organizer")]
        public string Organizer { get; set; }
        //[XmlElement(ElementName = "Attendees")]
        public Attendees Attendees { get; set; }
        //[XmlElement(ElementName = "Resources")]
        public Resources Resources { get; set; }
        //[XmlElement(ElementName = "IsEvent")]
        public string IsEvent { get; set; }
        //[XmlElement(ElementName = "IsRoomViewMeeting")]
        public string IsRoomViewMeeting { get; set; }
        //[XmlElement(ElementName = "IsPrivate")]
        public string IsPrivate { get; set; }
        //[XmlElement(ElementName = "IsExchangePrivate")]
        public string IsExchangePrivate { get; set; }
        //[XmlElement(ElementName = "MeetingTypes")]
        public MeetingTypes MeetingTypes { get; set; }
        //[XmlElement(ElementName = "ParticipantCode")]
        public string ParticipantCode { get; set; }
        //[XmlElement(ElementName = "PhoneNo")]
        public string PhoneNo { get; set; }
        //[XmlElement(ElementName = "WelcomeMsg")]
        public string WelcomeMsg { get; set; }
        //[XmlElement(ElementName = "Subject")]
        public string Subject { get; set; }
        //[XmlElement(ElementName = "LiveMeeting")]
        public LiveMeeting LiveMeeting { get; set; }
        //[XmlElement(ElementName = "ShareDocPath")]
        public string ShareDocPath { get; set; }
        //[XmlElement(ElementName = "HaveAttendees")]
        public string HaveAttendees { get; set; }
        //[XmlElement(ElementName = "HaveResources")]
        public string HaveResources { get; set; }

        /// <summary>
        /// Gets the duration of the meeting
        /// </summary>
        public string DurationInMinutes
        {
            get
            {
                string duration;

                var    timeSpan       = dtEnd.Subtract(dtStart);
                int    hours          = timeSpan.Hours;
                double minutes        = timeSpan.Minutes;
                double roundedMinutes = Math.Round(minutes);
                if (hours > 0)
                {
                    duration = string.Format("{0} hours {1} minutes", hours, roundedMinutes);
                }
                else
                {
                    duration = string.Format("{0} minutes", roundedMinutes);
                }

                return duration;
            }
        }

        /// <summary>
        /// Gets the remaining time in the meeting.  Returns null if the meeting is not currently in progress.
        /// </summary>
        public string RemainingTime
        {
            get
            {
                var now = DateTime.Now;

                string remainingTime;

                if (GetInProgress())
                {
                    var    timeSpan       = dtEnd.Subtract(now);
                    int    hours          = timeSpan.Hours;
                    double minutes        = timeSpan.Minutes;
                    double roundedMinutes = Math.Round(minutes);
                    if (hours > 0)
                    {
                        remainingTime = string.Format("{0} hours {1} minutes", hours, roundedMinutes);
                    }
                    else
                    {
                        remainingTime = string.Format("{0} minutes", roundedMinutes);
                    }

                    return remainingTime;
                }
                else
                    return null;
            }

        }

        /// <summary>
        /// Indicates that the meeting is in progress
        /// </summary>
        public bool isInProgress
        {
            get
            {
                return GetInProgress();
            }
        }

        /// <summary>
        /// Determines if the meeting is in progress
        /// </summary>
        /// <returns>Returns true if in progress</returns>
        bool GetInProgress()
        {
            var now = DateTime.Now;

            if (now > dtStart && now < dtEnd)
            {
                return true;
            }
            else
                return false;
        }
    }
}