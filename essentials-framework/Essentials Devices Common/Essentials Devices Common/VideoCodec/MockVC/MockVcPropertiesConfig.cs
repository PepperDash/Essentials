using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Crestron.SimplSharp;
using Newtonsoft.Json;

using PepperDash.Core;
using PepperDash.Essentials.Core;
using PepperDash.Essentials.Devices.Common.Codec;

namespace PepperDash.Essentials.Devices.Common.VideoCodec
{
    public class MockVcPropertiesConfig
    {
        [JsonProperty("favorites")]
        public List<CodecActiveCallItem> Favorites { get; set; }

        [JsonProperty("presets")]
        public List<CodecRoomPreset> Presets { get; set; }
    }
}