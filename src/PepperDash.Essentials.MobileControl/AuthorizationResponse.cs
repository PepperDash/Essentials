using Newtonsoft.Json;

namespace PepperDash.Essentials
{
    public class AuthorizationResponse
    {
        [JsonProperty("authorized")]
        public bool Authorized { get; set; }

        [JsonProperty("reason", NullValueHandling = NullValueHandling.Ignore)]
        public string Reason { get; set; } = null;
    }

    public class AuthorizationRequest
    {
        [JsonProperty("grantCode")]
        public string GrantCode { get; set; }
    }
}
