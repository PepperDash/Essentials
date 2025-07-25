using Newtonsoft.Json;

namespace PepperDash.Essentials
{
    /// <summary>
    /// Represents a AuthorizationResponse
    /// </summary>
    public class AuthorizationResponse
    {
        [JsonProperty("authorized")]
        /// <summary>
        /// Gets or sets the Authorized
        /// </summary>
        public bool Authorized { get; set; }

        [JsonProperty("reason", NullValueHandling = NullValueHandling.Ignore)]
        /// <summary>
        /// Gets or sets the Reason
        /// </summary>
        public string Reason { get; set; } = null;
    }

    /// <summary>
    /// Represents a AuthorizationRequest
    /// </summary>
    public class AuthorizationRequest
    {
        [JsonProperty("grantCode")]
        /// <summary>
        /// Gets or sets the GrantCode
        /// </summary>
        public string GrantCode { get; set; }
    }
}
