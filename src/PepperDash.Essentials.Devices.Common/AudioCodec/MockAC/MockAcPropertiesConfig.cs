extern alias Full;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Crestron.SimplSharp;

using Full.Newtonsoft.Json;

namespace PepperDash.Essentials.Devices.Common.AudioCodec
{
    public class MockAcPropertiesConfig
    {
        [JsonProperty("phoneNumber")]
        public string PhoneNumber { get; set; }
    }
}