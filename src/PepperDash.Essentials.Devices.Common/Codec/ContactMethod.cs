extern alias Full;
using Full::Newtonsoft.Json;
using Full::Newtonsoft.Json.Converters;

namespace PepperDash.Essentials.Devices.Common.Codec
{
    /// <summary>
    /// Represents a method of contact for a contact
    /// </summary>
    public class ContactMethod
    {
        [JsonProperty("contactMethodId")]
        public string ContactMethodId { get; set; }

        [JsonProperty("number")]
        public string Number { get; set; }
        
        [JsonProperty("device")]
        [JsonConverter(typeof(StringEnumConverter))]
        public eContactMethodDevice Device { get; set; }

        [JsonProperty("callType")]
        [JsonConverter(typeof(StringEnumConverter))]
        public eContactMethodCallType CallType { get; set; }
    }
}