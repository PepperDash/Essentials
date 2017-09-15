using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Crestron.SimplSharp;

namespace PepperDash.Essentials.Devices.Common.VideoCodec.Cisco
{
    public class CiscoCodecEvents
    {
        public class CauseValue
        {
            public string id { get; set; }
            public string Value { get; set; }
        }

        public class CauseType
        {
            public string id { get; set; }
            public string Value { get; set; }
        }

        public class CauseString
        {
            public string id { get; set; }
            public string Value { get; set; }
        }

        public class OrigCallDirection
        {
            public string id { get; set; }
            public string Value { get; set; }
        }

        public class RemoteURI
        {
            public string id { get; set; }
            public string Value { get; set; }
        }

        public class DisplayName
        {
            public string id { get; set; }
            public string Value { get; set; }
        }

        public class CallId
        {
            public string id { get; set; }
            public string Value { get; set; }
        }

        public class CauseCode
        {
            public string id { get; set; }
            public string Value { get; set; }
        }

        public class CauseOrigin
        {
            public string id { get; set; }
            public string Value { get; set; }
        }

        public class Protocol
        {
            public string id { get; set; }
            public string Value { get; set; }
        }

        public class Duration
        {
            public string id { get; set; }
            public string Value { get; set; }
        }

        public class CallType
        {
            public string id { get; set; }
            public string Value { get; set; }
        }

        public class CallRate
        {
            public string id { get; set; }
            public string Value { get; set; }
        }

        public class Encryption
        {
            public string id { get; set; }
            public string Value { get; set; }
        }

        public class RequestedURI
        {
            public string id { get; set; }
            public string Value { get; set; }
        }

        public class PeopleCountAverage
        {
            public string id { get; set; }
            public string Value { get; set; }
        }

        public class CallDisconnect
        {
            public string id { get; set; }
            public CauseValue CauseValue { get; set; }
            public CauseType CauseType { get; set; }
            public CauseString CauseString { get; set; }
            public OrigCallDirection OrigCallDirection { get; set; }
            public RemoteURI RemoteURI { get; set; }
            public DisplayName DisplayName { get; set; }
            public CallId CallId { get; set; }
            public CauseCode CauseCode { get; set; }
            public CauseOrigin CauseOrigin { get; set; }
            public Protocol Protocol { get; set; }
            public Duration Duration { get; set; }
            public CallType CallType { get; set; }
            public CallRate CallRate { get; set; }
            public Encryption Encryption { get; set; }
            public RequestedURI RequestedURI { get; set; }
            public PeopleCountAverage PeopleCountAverage { get; set; }
        }

        public class Event
        {
            public CallDisconnect CallDisconnect { get; set; }
        }

        public class RootObject
        {
            public Event Event { get; set; }
        }
    }
}