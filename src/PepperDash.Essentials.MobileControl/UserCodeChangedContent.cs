using Newtonsoft.Json;

namespace PepperDash.Essentials
{
    /// <summary>
    /// Represents a UserCodeChangedContent
    /// </summary>
    public class UserCodeChangedContent
    {
        [JsonProperty("userCode")]
        /// <summary>
        /// Gets or sets the UserCode
        /// </summary>
        public string UserCode { get; set; }

        [JsonProperty("qrChecksum", NullValueHandling = NullValueHandling.Include)]
        /// <summary>
        /// Gets or sets the QrChecksum
        /// </summary>
        public string QrChecksum { get; set; }
    }
}
