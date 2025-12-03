

using System;
using System.Collections.Generic;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using PepperDash.Essentials.Devices.Common.VideoCodec;

namespace PepperDash.Essentials.Devices.Common.Codec
{

    /// <summary>
    /// Defines the contract for IHasCallHistory
    /// </summary>
    public interface IHasCallHistory
    {
        /// <summary>
        /// Gets the call history for this device
        /// </summary>
        CodecCallHistory CallHistory { get; }

        /// <summary>
        /// Removes the specified call history entry
        /// </summary>
        /// <param name="entry">The call history entry to remove</param>
        void RemoveCallHistoryEntry(CodecCallHistory.CallHistoryEntry entry);
    }

    /// <summary>
    /// Enumeration of eCodecOccurrenceType values
    /// </summary>
    public enum eCodecOccurrenceType
    {
        /// <summary>
        /// Unknown occurrence type
        /// </summary>
        Unknown = 0,

        /// <summary>
        /// Call was placed (outgoing)
        /// </summary>
        Placed = 1,

        /// <summary>
        /// Call was received (incoming)
        /// </summary>
        Received = 2,

        /// <summary>
        /// Call received no answer
        /// </summary>
        NoAnswer = 3,
    }

    /// <summary>
    /// Represents a CodecCallHistory
    /// </summary>
    public class CodecCallHistory
    {
        /// <summary>
        /// Event that is raised when the recent calls list has changed
        /// </summary>
        public event EventHandler<EventArgs> RecentCallsListHasChanged;

        /// <summary>
        /// Gets or sets the RecentCalls
        /// </summary>
        public List<CallHistoryEntry> RecentCalls { get; private set; }

        /// <summary>
        /// Item that gets added to the list when there are no recent calls in history
        /// </summary>
        CallHistoryEntry ListEmptyEntry;

        /// <summary>
        /// Initializes a new instance of the CodecCallHistory class
        /// </summary>
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
            /// <summary>
            /// Gets or sets the StartTime
            /// </summary>
            [JsonConverter(typeof(IsoDateTimeConverter))]
            [JsonProperty("startTime")]
            public DateTime StartTime { get; set; }
            /// <summary>
            /// Gets or sets the occurrence type for this call history entry
            /// </summary>
            [JsonConverter(typeof(StringEnumConverter))]
            [JsonProperty("occurrenceType")]
            public eCodecOccurrenceType OccurrenceType { get; set; }

            /// <summary>
            /// Gets or sets the occurrence history identifier
            /// </summary>
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
            if (genericEntries.Count == 0)
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