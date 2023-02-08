extern alias Full;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Crestron.SimplSharp;

using PepperDash.Core;

using Full.Newtonsoft.Json;

namespace PepperDash.Essentials.DM.AirMedia
{
    public class AirMediaPropertiesConfig
    {
        [JsonProperty("control")]
        public ControlPropertiesConfig Control { get; set; }

        [JsonProperty("autoSwitching")]
        public bool AutoSwitchingEnabled { get; set; }
    }
}