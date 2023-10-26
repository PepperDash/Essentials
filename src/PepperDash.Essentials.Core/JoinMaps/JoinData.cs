extern alias Full;
using Full::Newtonsoft.Json;

namespace PepperDash.Essentials.Core
{
    /// <summary>
    /// Data describing the join.  Can be overridden from configuratino
    /// </summary>
    public class JoinData
    {
        /// <summary>
        /// Join number (based on join offset value)
        /// </summary>
        [JsonProperty("joinNumber")]
        public uint JoinNumber { get; set; }
        /// <summary>
        /// Join range span.  If join indicates the start of a range of joins, this indicated the maximum number of joins in the range
        /// </summary>
        [JsonProperty("joinSpan")]
        public uint JoinSpan { get; set; }
        /// <summary>
        /// Fusion Attribute Name (optional)
        /// </summary>
        [JsonProperty("attributeName")]
        public string AttributeName { get; set; }
    }
}