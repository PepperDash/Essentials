extern alias Full;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Crestron.SimplSharp;
using Full.Newtonsoft.Json;

namespace PepperDash.Essentials.Core.CrestronIO
{
    public class IOPortConfig
    {
        [JsonProperty("portDeviceKey")]
        public string PortDeviceKey { get; set; }
        [JsonProperty("portNumber")]
        public uint PortNumber { get; set; }
        [JsonProperty("disablePullUpResistor")]
        public bool DisablePullUpResistor { get; set; }
        [JsonProperty("minimumChange")]
        public int MinimumChange { get; set; }
    }
}