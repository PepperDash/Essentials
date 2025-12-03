using Newtonsoft.Json;

namespace PepperDash.Essentials
{
    /// <summary>
    /// Represents a UserCodeChangedContent
    /// </summary>
    public class UserCodeChangedContent
    {

        /// <summary>
        /// Gets or sets the UserCode
        /// </summary>
        [JsonProperty("userCode")]
        public string UserCode { get; set; }


        /// <summary>
        /// Gets or sets the QrChecksum
        /// </summary>
        [JsonProperty("qrChecksum", NullValueHandling = NullValueHandling.Include)]
        public string QrChecksum { get; set; }
    }
}
