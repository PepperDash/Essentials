using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Crestron.SimplSharp;

namespace PepperDash.Essentials.Devices.Common.Codec
{
    public interface IHasCallHistory
    {
        List<CallHistory.CallHistoryEntry> RecentCalls { get; }

        void RemoveEntry(CallHistory.CallHistoryEntry entry);
    }

    public enum eCodecOccurrenctType
    {
        Unknown = 0,
        Placed,
        Received,
        NoAnswer
    }

    public class CallHistory
    {

        /// <summary>
        /// Generic call history entry, not device specific
        /// </summary>
        public class CallHistoryEntry
        {
            public string DisplayName { get; set; }
            public string CallBackNumber { get; set; }
            public DateTime StartTime { get; set; }
            public eCodecOccurrenctType OccurenceType { get; set; }
            public string OccurrenceHistoryId { get; set; }
        }

        /// <summary>
        /// Converts a list of call history entries returned by a Cisco codec to the generic list type
        /// </summary>
        /// <param name="entries"></param>
        /// <returns></returns>
        public static List<CallHistoryEntry> ConvertCiscoCallHistoryToGeneric(List<CiscoCallHistory.Entry> entries)
        {
            var genericEntries = new List<CallHistoryEntry>();

            foreach (CiscoCallHistory.Entry entry in entries)
            {
                genericEntries.Add(new CallHistoryEntry()
                {
                    DisplayName = entry.DisplayName.Value,
                    CallBackNumber = entry.CallbackNumber.Value,
                    StartTime = entry.LastOccurrenceStartTime.Value,
                    OccurrenceHistoryId = entry.LastOccurrenceHistoryId.Value,
                    OccurenceType = ConvertToOccurenceTypeEnum(entry.OccurrenceType.Value)          
                });
            }

            return genericEntries;

        }

        /// <summary>
        /// Takes the Cisco occurence type and converts it to the matching enum
        /// </summary>
        /// <param name="s"></para
        public static eCodecOccurrenctType ConvertToOccurenceTypeEnum(string s)
        {
            switch (s)
            {
                case "Placed":
                    {
                        return eCodecOccurrenctType.Placed;
                    }
                case "Received":
                    {
                        return eCodecOccurrenctType.Received;
                    }
                case "NoAnswer":
                    {
                        return eCodecOccurrenctType.NoAnswer;
                    }
                default:
                    return eCodecOccurrenctType.Unknown;
            }

        }

    }




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