using System;
using System.Collections.Generic;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace PepperDash.Essentials.Core.Devices.Codec
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
        readonly CallHistoryEntry _listEmptyEntry;

        public CallHistoryEntry ListEmptyEntry
        {
            get { return _listEmptyEntry; }
        }

        public CodecCallHistory()
        {
            _listEmptyEntry = new CallHistoryEntry() { Name = "No Recent Calls" };

            RecentCalls = new List<CallHistoryEntry> {_listEmptyEntry};
        }

        private void OnRecentCallsListChange()
        {
            var handler = RecentCallsListHasChanged;
            if (handler != null)
            {
                handler(this, new EventArgs());
            }
        }

        public void RemoveEntry(CallHistoryEntry entry)
        {
            RecentCalls.Remove(entry);
            OnRecentCallsListChange();
        }

        public void UpdateCallHistory(List<CallHistoryEntry> newList)
        {
            RecentCalls = newList;

            OnRecentCallsListChange();
        }

        /// <summary>
        /// Generic call history entry, not device specific
        /// </summary>
        public class CallHistoryEntry : CodecActiveCallItem
        {
            [JsonConverter(typeof (IsoDateTimeConverter))]
            [JsonProperty("startTime")]
            public DateTime StartTime { get; set; }

            [JsonConverter(typeof (StringEnumConverter))]
            [JsonProperty("occurrenceType")]
            public eCodecOccurrenceType OccurrenceType { get; set; }

            [JsonProperty("occurrenceHistoryId")]
            public string OccurrenceHistoryId { get; set; }
        }
    }
}