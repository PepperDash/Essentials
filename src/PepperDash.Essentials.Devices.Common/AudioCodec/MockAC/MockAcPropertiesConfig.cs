using Newtonsoft.Json;

namespace PepperDash.Essentials.Devices.Common.AudioCodec
{
    /// <summary>
    /// Represents a MockAcPropertiesConfig
    /// </summary>
    public class MockAcPropertiesConfig
    {
        /// <summary>
        /// Gets or sets the PhoneNumber
        /// </summary>
        [JsonProperty("phoneNumber")]
        public string PhoneNumber { get; set; }
    }
}