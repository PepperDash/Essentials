using System;
using System.Collections.Generic;

namespace PepperDash.Essentials.Devices.Common.VideoCodec
{
    /// <summary>
    /// Represents a CiscoCallHistory
    /// </summary>
    public class CiscoCallHistory
    {
        /// <summary>
        /// Represents a CallbackNumber
        /// </summary>
        public class CallbackNumber
        {
            /// <summary>
            /// Gets or sets the Value
            /// </summary>
            public string Value { get; set; }
        }

        /// <summary>
        /// Represents a DisplayName
        /// </summary>
        public class DisplayName
        {
            /// <summary>
            /// Gets or sets the Value
            /// </summary>
            public string Value { get; set; }
        }

        /// <summary>
        /// Represents a LastOccurrenceStartTime
        /// </summary>
        public class LastOccurrenceStartTime
        {
            /// <summary>
            /// Gets or sets the Value
            /// </summary>
            public DateTime Value { get; set; }
        }

        /// <summary>
        /// Represents a LastOccurrenceDaysAgo
        /// </summary>
        public class LastOccurrenceDaysAgo
        {
            /// <summary>
            /// Gets or sets the Value
            /// </summary>
            public string Value { get; set; }
        }

        /// <summary>
        /// Represents a LastOccurrenceHistoryId
        /// </summary>
        public class LastOccurrenceHistoryId
        {
            /// <summary>
            /// Gets or sets the Value
            /// </summary>
            public string Value { get; set; }
        }

        /// <summary>
        /// Represents a OccurrenceType
        /// </summary>
        public class OccurrenceType
        {
            /// <summary>
            /// Gets or sets the Value
            /// </summary>
            public string Value { get; set; }
        }

        /// <summary>
        /// Represents a IsAcknowledged
        /// </summary>
        public class IsAcknowledged
        {
            /// <summary>
            /// Gets or sets the Value
            /// </summary>
            public string Value { get; set; }
        }

        /// <summary>
        /// Represents a OccurrenceCount
        /// </summary>
        public class OccurrenceCount
        {
            /// <summary>
            /// Gets or sets the Value
            /// </summary>
            public string Value { get; set; }
        }

        /// <summary>
        /// Represents a Entry
        /// </summary>
        public class Entry
        {
            /// <summary>
            /// Gets or sets the id
            /// </summary>
            public string id { get; set; }
            /// <summary>
            /// Gets or sets the CallbackNumber
            /// </summary>
            public CallbackNumber CallbackNumber { get; set; }
            /// <summary>
            /// Gets or sets the DisplayName
            /// </summary>
            public DisplayName DisplayName { get; set; }
            /// <summary>
            /// Gets or sets the LastOccurrenceStartTime
            /// </summary>
            public LastOccurrenceStartTime LastOccurrenceStartTime { get; set; }
            /// <summary>
            /// Gets or sets the LastOccurrenceDaysAgo
            /// </summary>
            public LastOccurrenceDaysAgo LastOccurrenceDaysAgo { get; set; }
            /// <summary>
            /// Gets or sets the LastOccurrenceHistoryId
            /// </summary>
            public LastOccurrenceHistoryId LastOccurrenceHistoryId { get; set; }
            /// <summary>
            /// Gets or sets the OccurrenceType
            /// </summary>
            public OccurrenceType OccurrenceType { get; set; }
            /// <summary>
            /// Gets or sets the IsAcknowledged
            /// </summary>
            public IsAcknowledged IsAcknowledged { get; set; }
            /// <summary>
            /// Gets or sets the OccurrenceCount
            /// </summary>
            public OccurrenceCount OccurrenceCount { get; set; }
        }

        /// <summary>
        /// Represents a Offset
        /// </summary>
        public class Offset
        {
            /// <summary>
            /// Gets or sets the Value
            /// </summary>
            public string Value { get; set; }
        }

        /// <summary>
        /// Represents a Limit
        /// </summary>
        public class Limit
        {
            /// <summary>
            /// Gets or sets the Value
            /// </summary>
            public string Value { get; set; }
        }

        /// <summary>
        /// Represents a ResultInfo
        /// </summary>
        public class ResultInfo
        {
            /// <summary>
            /// Gets or sets the Offset
            /// </summary>
            public Offset Offset { get; set; }
            /// <summary>
            /// Gets or sets the Limit
            /// </summary>
            public Limit Limit { get; set; }
        }

        /// <summary>
        /// Represents a CallHistoryRecentsResult
        /// </summary>
        public class CallHistoryRecentsResult
        {
            /// <summary>
            /// Gets or sets the status
            /// </summary>
            public string status { get; set; }
            /// <summary>
            /// Gets or sets the Entry
            /// </summary>
            public List<Entry> Entry { get; set; }
            /// <summary>
            /// Gets or sets the ResultInfo
            /// </summary>
            public ResultInfo ResultInfo { get; set; }
        }

        /// <summary>
        /// Represents a CommandResponse
        /// </summary>
        public class CommandResponse
        {
            /// <summary>
            /// Gets or sets the CallHistoryRecentsResult
            /// </summary>
            public CallHistoryRecentsResult CallHistoryRecentsResult { get; set; }
        }

        /// <summary>
        /// Represents a RootObject
        /// </summary>
        public class RootObject
        {
            /// <summary>
            /// Gets or sets the CommandResponse
            /// </summary>
            public CommandResponse CommandResponse { get; set; }
        }
    }
}