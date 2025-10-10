using Newtonsoft.Json;

namespace PepperDash.Essentials
{
    /// <summary>
    /// Represents a AuthorizationResponse
    /// </summary>
    public class AuthorizationResponse
    {

        /// <summary>
        /// Gets or sets the Authorized
        /// </summary>
        [JsonProperty("authorized")]
        public bool Authorized { get; set; }


        /// <summary>
        /// Gets or sets the Reason
        /// </summary>
        [JsonProperty("reason", NullValueHandling = NullValueHandling.Ignore)]
        public string Reason { get; set; } = null;
    }

    /// <summary>
    /// Represents a AuthorizationRequest
    /// </summary>
    public class AuthorizationRequest
    {

        /// <summary>
        /// Gets or sets the GrantCode
        /// </summary>
        [JsonProperty("grantCode")]
        public string GrantCode { get; set; }
    }
}
