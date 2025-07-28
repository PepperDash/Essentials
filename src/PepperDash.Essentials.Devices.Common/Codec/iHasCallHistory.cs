

using System;
using System.Collections.Generic;
using PepperDash.Essentials.Devices.Common.VideoCodec;

using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace PepperDash.Essentials.Devices.Common.Codec
{

    /// <summary>
    /// Defines the contract for IHasCallHistory
    /// </summary>
    public interface IHasCallHistory
    {
        CodecCallHistory CallHistory { get; }

        void RemoveCallHistoryEntry(CodecCallHistory.CallHistoryEntry entry);
    }

    /// <summary>
    /// Enumeration of eCodecOccurrenceType values
    /// </summary>
    public enum eCodecOccurrenceType
    {
        Unknown = 0,
        Placed = 1,
        Received = 2,
        NoAnswer = 3,
    }

    /// <summary>
    /// Represents a CodecCallHistory
    /// </summary>
    public class CodecCallHistory
    {
        public event EventHandler<EventArgs> RecentCallsListHasChanged;

        /// <summary>
        /// Gets or sets the RecentCalls
        /// </summary>
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
            }
        }

        /// <summary>
        /// RemoveEntry method
        /// </summary>
        public void RemoveEntry(CallHistoryEntry entry)
        {
            RecentCalls.Remove(entry);
            OnRecentCallsListChange();
        }

        /// <summary>
        /// Represents a CallHistoryEntry
        /// </summary>
        public class CallHistoryEntry : CodecActiveCallItem
        {
            [JsonConverter(typeof(IsoDateTimeConverter))]
            [JsonProperty("startTime")]
            /// <summary>
            /// Gets or sets the StartTime
            /// </summary>
            public DateTime StartTime { get; set; }
            [JsonConverter(typeof(StringEnumConverter))]
            [JsonProperty("occurrenceType")]
            public eCodecOccurrenceType OccurrenceType { get; set; }
            [JsonProperty("occurrenceHistoryId")]
            public string OccurrenceHistoryId { get; set; }
        }

        /// <summary>
        /// Converts a list of call history entries returned by a Cisco codec to the generic list type
        /// </summary>
        /// <param name="entries"></param>
        /// <returns></returns>
        /// <summary>
        /// ConvertCiscoCallHistoryToGeneric method
        /// </summary>
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
                    OccurrenceType = ConvertToOccurenceTypeEnum(entry.OccurrenceType.Value)
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
        /// <param name="s"></param>
        /// <summary>
        /// ConvertToOccurenceTypeEnum method
        /// </summary>
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
}