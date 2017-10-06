using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Crestron.SimplSharp;

using PepperDash.Core;
using PepperDash.Essentials.Devices.Common.Codec;

namespace PepperDash.Essentials.Devices.Common.VideoCodec
{
    public class CiscoCodecBookings
    {
        public class TotalRows
        {
            public string Value { get; set; }
        }

        public class ResultInfo
        {
            public TotalRows TotalRows { get; set; }
        }

        public class LastUpdated
        {
            public DateTime Value { get; set; }
        }

        public class Id
        {
            public string Value { get; set; }
        }

        public class Title
        {
            public string Value { get; set; }
        }

        public class Agenda
        {
            public string Value { get; set; }
        }

        public class Privacy
        {
            public string Value { get; set; }
        }

        public class FirstName
        {
            public string Value { get; set; }
        }

        public class LastName
        {
            public string Value { get; set; }
        }

        public class Email
        {
            public string Value { get; set; }
        }

        public class Id2
        {
            public string Value { get; set; }
        }

        public class Organizer
        {
            public FirstName FirstName { get; set; }
            public LastName LastName { get; set; }
            public Email Email { get; set; }
            public Id2 Id { get; set; }

            public Organizer()
            {
                FirstName = new FirstName();
                LastName = new LastName();
                Email = new Email();
            }
        }

        public class StartTime
        {
            public DateTime Value { get; set; }
        }

        public class StartTimeBuffer
        {
            public string Value { get; set; }
        }

        public class EndTime
        {
            public DateTime Value { get; set; }
        }

        public class EndTimeBuffer
        {
            public string Value { get; set; }
        }

        public class Time
        {
            public StartTime StartTime { get; set; }
            public StartTimeBuffer StartTimeBuffer { get; set; }
            public EndTime EndTime { get; set; }
            public EndTimeBuffer EndTimeBuffer { get; set; }

            public Time()
            {
                StartTime = new StartTime();
                EndTime = new EndTime();
            }
        }

        public class MaximumMeetingExtension
        {
            public string Value { get; set; }
        }

        public class MeetingExtensionAvailability
        {
            public string Value { get; set; }
        }

        public class BookingStatus
        {
            public string Value { get; set; }
        }

        public class BookingStatusMessage
        {
            public string Value { get; set; }
        }

        public class Enabled
        {
            public string Value { get; set; }
        }

        public class Url
        {
            public string Value { get; set; }
        }

        public class MeetingNumber
        {
            public string Value { get; set; }
        }

        public class Password
        {
            public string Value { get; set; }
        }

        public class HostKey
        {
            public string Value { get; set; }
        }

        public class DialInNumbers
        {
        }

        public class Webex
        {
            public Enabled Enabled { get; set; }
            public Url Url { get; set; }
            public MeetingNumber MeetingNumber { get; set; }
            public Password Password { get; set; }
            public HostKey HostKey { get; set; }
            public DialInNumbers DialInNumbers { get; set; }
        }

        public class Encryption
        {
            public string Value { get; set; }
        }

        public class Role
        {
            public string Value { get; set; }
        }

        public class Recording
        {
            public string Value { get; set; }
        }

        public class Calls
        {
        }

        public class ConnectMode
        {
            public string Value { get; set; }
        }

        public class DialInfo
        {
            public Calls Calls { get; set; }
            public ConnectMode ConnectMode { get; set; }
        }

        public class Booking
        {
            public string id { get; set; }
            public Id Id { get; set; }
            public Title Title { get; set; }
            public Agenda Agenda { get; set; }
            public Privacy Privacy { get; set; }
            public Organizer Organizer { get; set; }
            public Time Time { get; set; }
            public MaximumMeetingExtension MaximumMeetingExtension { get; set; }
            public MeetingExtensionAvailability MeetingExtensionAvailability { get; set; }
            public BookingStatus BookingStatus { get; set; }
            public BookingStatusMessage BookingStatusMessage { get; set; }
            public Webex Webex { get; set; }
            public Encryption Encryption { get; set; }
            public Role Role { get; set; }
            public Recording Recording { get; set; }
            public DialInfo DialInfo { get; set; }

            public Booking()
            {
                Time = new Time();
                Id = new Id();
                Organizer = new Organizer();
                Title = new Title();
                Agenda = new Agenda();
                Privacy = new Privacy();
            }
        }

        public class BookingsListResult
        {
            public string status { get; set; }
            public ResultInfo ResultInfo { get; set; }
            public LastUpdated LastUpdated { get; set; }
            public List<Booking> Booking { get; set; }
        }

        public class CommandResponse
        {
            public BookingsListResult BookingsListResult { get; set; }
        }

        public class RootObject
        {
            public CommandResponse CommandResponse { get; set; }
        }

        /// <summary>
        /// Extracts the necessary meeting values from the Cisco bookings response ans converts them to the generic class
        /// </summary>
        /// <param name="bookings"></param>
        /// <returns></returns>
        public static List<Meeting> GetGenericMeetingsFromBookingResult(List<Booking> bookings)
        {
            var meetings = new List<Meeting>();

            if (Debug.Level > 0)
            {
                Debug.Console(1, "Meetings List:\n"); 
            }

            foreach(Booking b in bookings)
            {
                var meeting = new Meeting();

                meeting.Id = b.Id.Value;
                meeting.Organizer = string.Format("{0}, {1}", b.Organizer.LastName.Value, b.Organizer.FirstName.Value);
                meeting.Title = b.Title.Value;
                meeting.Agenda = b.Agenda.Value;
                meeting.StartTime = b.Time.StartTime.Value;
                meeting.EndTime = b.Time.EndTime.Value;
                meeting.Privacy = CodecCallPrivacy.ConvertToDirectionEnum(b.Privacy.Value);

                meetings.Add(meeting);

                if(Debug.Level > 0)
                {
                    Debug.Console(1, "Title: {0}, ID: {1}, Organizer: {2}, Agenda: {3}", meeting.Title, meeting.Id, meeting.Organizer, meeting.Agenda);
                    Debug.Console(1, "    Start Time: {0}, End Time: {1}, Duration: {2}", meeting.StartTime, meeting.EndTime, meeting.Duration);
                    Debug.Console(1, "    Joinable: {0}", meeting.Joinable);
                }
            }

            meetings.OrderBy(m => m.StartTime);

            return meetings;
        }
    }
}