using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Crestron.SimplSharp;

using PepperDash.Core;

namespace PepperDash.Essentials.Devices.Common.Codec
{
    public interface IHasCallHistory
    {
        CodecCallHistory CallHistory { get; }

        void RemoveCallHistoryEntry(CodecCallHistory.CallHistoryEntry entry);
    }

    public enum eCodecOccurrenceType
    {
        Unknown = 0,
        Placed,
        Received,
        NoAnswer
    }

    /// <summary>
    /// Represents the recent call history for a codec device
    /// </summary>
    public class CodecCallHistory
    {
        public event EventHandler<EventArgs> RecentCallsListHasChanged;

        public List<CallHistoryEntry> RecentCalls { get; private set; }

        /// <summary>
        /// Item that gets added to the list when there are no recent calls in history
        /// </summary>
        CallHistoryEntry ListEmptyEntry;

        public CodecCallHistory()
        {
            ListEmptyEntry = new CallHistoryEntry() { Name = "No Recent Calls" };

            RecentCalls = new List<CallHistoryEntry>();

            RecentCalls.Add(ListEmptyEntry);
        }

        void OnRecentCallsListChange()
        {
            var handler = RecentCallsListHasChanged;
            if (handler != null)
            {
                handler(this, new EventArgs());

                if (Debug.Level == 1)
                {
                    
                    Debug.Console(1, "RecentCalls:\n");

                    foreach (CodecCallHistory.CallHistoryEntry entry in RecentCalls)
                    {
                        Debug.Console(1, "\nName: {0}\nNumber: {1}\nStartTime: {2}\nType: {3}\n", entry.Name, entry.Number, entry.StartTime.ToString(), entry.OccurenceType);
                    }
                }
            }
        }

        public void RemoveEntry(CallHistoryEntry entry)
        {
            RecentCalls.Remove(entry);
            OnRecentCallsListChange();
        }

        /// <summary>
        /// Generic call history entry, not device specific
        /// </summary>
        public class CallHistoryEntry : CodecActiveCallItem
        {
            public DateTime StartTime { get; set; }
            public eCodecOccurrenceType OccurenceType { get; set; }
            public string OccurrenceHistoryId { get; set; }
        }

        /// <summary>
        /// Converts a list of call history entries returned by a Cisco codec to the generic list type
        /// </summary>
        /// <param name="entries"></param>
        /// <returns></returns>
        public void ConvertCiscoCallHistoryToGeneric(List<CiscoCallHistory.Entry> entries)
        {
            var genericEntries = new List<CallHistoryEntry>();

            foreach (CiscoCallHistory.Entry entry in entries)
            {
                genericEntries.Add(new CallHistoryEntry()
                {
                    Name = entry.DisplayName.Value,
                    Number = entry.CallbackNumber.Value,
                    StartTime = entry.LastOccurrenceStartTime.Value,
                    OccurrenceHistoryId = entry.LastOccurrenceHistoryId.Value,
                    OccurenceType = ConvertToOccurenceTypeEnum(entry.OccurrenceType.Value)          
                });
            }

            // Check if list is empty and if so, add an item to display No Recent Calls
            if(genericEntries.Count == 0)
                genericEntries.Add(ListEmptyEntry);

            RecentCalls = genericEntries;
            OnRecentCallsListChange();
        }

        /// <summary>
        /// Takes the Cisco occurence type and converts it to the matching enum
        /// </summary>
        /// <param name="s"></para
        public eCodecOccurrenceType ConvertToOccurenceTypeEnum(string s)
        {
            switch (s)
            {
                case "Placed":
                    {
                        return eCodecOccurrenceType.Placed;
                    }
                case "Received":
                    {
                        return eCodecOccurrenceType.Received;
                    }
                case "NoAnswer":
                    {
                        return eCodecOccurrenceType.NoAnswer;
                    }
                default:
                    return eCodecOccurrenceType.Unknown;
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