using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Crestron.SimplSharp;

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
    }
}