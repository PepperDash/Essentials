extern alias Full;
using System;
using Full::Newtonsoft.Json;

namespace PepperDash.Essentials.Core
{
    /// <summary>
    /// Metadata describing the join
    /// </summary>
    public class JoinMetadata
    {
        private string _description;

        /// <summary>
        /// Join number (based on join offset value)
        /// </summary>
        [JsonProperty("joinNumber")]
        [Obsolete]
        public uint JoinNumber { get; set; }
        /// <summary>
        /// Join range span.  If join indicates the start of a range of joins, this indicated the maximum number of joins in the range
        /// </summary>
        [Obsolete]
        [JsonProperty("joinSpan")]
        public uint JoinSpan { get; set; }

        /// <summary>
        /// A label for the join to better describe its usage
        /// </summary>
        [Obsolete("Use Description instead")]
        [JsonProperty("label")]
        public string Label { get { return _description; } set { _description = value; } }

        /// <summary>
        /// A description for the join to better describe its usage
        /// </summary>
        [JsonProperty("description")]
        public string Description { get { return _description; } set { _description = value; } }
        /// <summary>
        /// Signal type(s)
        /// </summary>
        [JsonProperty("joinType")]
        public eJoinType JoinType { get; set; }
        /// <summary>
        /// Indicates whether the join is read and/or write
        /// </summary>
        [JsonProperty("joinCapabilities")]
        public eJoinCapabilities JoinCapabilities { get; set; }
        /// <summary>
        /// Indicates a set of valid values (particularly if this translates to an enum
        /// </summary>
        [JsonProperty("validValues")]
        public string[] ValidValues { get; set; }

    }
}