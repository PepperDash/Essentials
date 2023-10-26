extern alias Full;
using System.Collections.Generic;
using Full::Newtonsoft.Json;

namespace PepperDash.Essentials.Core.Bridges
{
    public class EiscApiPropertiesConfig
    {
        [JsonProperty("control")]
        public EssentialsControlPropertiesConfig Control { get; set; }

        [JsonProperty("devices")]
        public List<ApiDevicePropertiesConfig> Devices { get; set; }

        [JsonProperty("rooms")]
        public List<ApiRoomPropertiesConfig> Rooms { get; set; } 


        public class ApiDevicePropertiesConfig
        {
            [JsonProperty("deviceKey")]
            public string DeviceKey { get; set; }

            [JsonProperty("joinStart")]
            public uint JoinStart { get; set; }

            [JsonProperty("joinMapKey")]
            public string JoinMapKey { get; set; }
        }

        public class ApiRoomPropertiesConfig
        {
            [JsonProperty("roomKey")]
            public string RoomKey { get; set; }

            [JsonProperty("joinStart")]
            public uint JoinStart { get; set; }

            [JsonProperty("joinMapKey")]
            public string JoinMapKey { get; set; }
        }

    }
}