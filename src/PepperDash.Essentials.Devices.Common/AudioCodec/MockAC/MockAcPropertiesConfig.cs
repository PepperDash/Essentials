using Newtonsoft.Json;

namespace PepperDash.Essentials.Devices.Common.AudioCodec;

public class MockAcPropertiesConfig
{
    [JsonProperty("phoneNumber")]
    public string PhoneNumber { get; set; }
}