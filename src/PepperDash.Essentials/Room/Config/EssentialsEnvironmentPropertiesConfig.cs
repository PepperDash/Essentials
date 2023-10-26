extern alias Full;
using System.Collections.Generic;
using Full::Newtonsoft.Json;

namespace PepperDash.Essentials.Room.Config
{
    public class EssentialsEnvironmentPropertiesConfig
    {
        public bool Enabled { get; set; }

        [JsonProperty("deviceKeys")]
        public List<string> DeviceKeys { get; set; }

        public EssentialsEnvironmentPropertiesConfig()
        {
            DeviceKeys = new List<string>();
        }

    }
}