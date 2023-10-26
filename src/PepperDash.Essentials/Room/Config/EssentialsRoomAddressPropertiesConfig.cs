extern alias Full;
using Full::Newtonsoft.Json;

namespace PepperDash.Essentials.Room.Config
{
    public class EssentialsRoomAddressPropertiesConfig
    {
        [JsonProperty("phoneNumber")]
        public string PhoneNumber { get; set; }

        [JsonProperty("sipAddress")]
        public string SipAddress { get; set; }
    }
}