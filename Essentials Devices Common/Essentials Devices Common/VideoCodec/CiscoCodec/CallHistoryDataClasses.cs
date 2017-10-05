using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Crestron.SimplSharp;

namespace PepperDash.Essentials.Devices.Common.VideoCodec
{
    public class CiscoCallHistory
    {
        public class CallbackNumber
        {
            public string Value { get; set; }
        }

        public class DisplayName
        {
            public string Value { get; set; }
        }

        public class LastOccurrenceStartTime
        {
            public DateTime Value { get; set; }
        }

        public class LastOccurrenceDaysAgo
        {
            public string Value { get; set; }
        }

        public class LastOccurrenceHistoryId
        {
            public string Value { get; set; }
        }

        public class OccurrenceType
        {
            public string Value { get; set; }
        }

        public class IsAcknowledged
        {
            public string Value { get; set; }
        }

        public class OccurrenceCount
        {
            public string Value { get; set; }
        }

        public class Entry
        {
            public string id { get; set; }
            public CallbackNumber CallbackNumber { get; set; }
            public DisplayName DisplayName { get; set; }
            public LastOccurrenceStartTime LastOccurrenceStartTime { get; set; }
            public LastOccurrenceDaysAgo LastOccurrenceDaysAgo { get; set; }
            public LastOccurrenceHistoryId LastOccurrenceHistoryId { get; set; }
            public OccurrenceType OccurrenceType { get; set; }
            public IsAcknowledged IsAcknowledged { get; set; }
            public OccurrenceCount OccurrenceCount { get; set; }
        }

        public class Offset
        {
            public string Value { get; set; }
        }

        public class Limit
        {
            public string Value { get; set; }
        }

        public class ResultInfo
        {
            public Offset Offset { get; set; }
            public Limit Limit { get; set; }
        }

        public class CallHistoryRecentsResult
        {
            public string status { get; set; }
            public List<Entry> Entry { get; set; }
            public ResultInfo ResultInfo { get; set; }
        }

        public class CommandResponse
        {
            public CallHistoryRecentsResult CallHistoryRecentsResult { get; set; }
        }

        public class RootObject
        {
            public CommandResponse CommandResponse { get; set; }
        }
    }
}