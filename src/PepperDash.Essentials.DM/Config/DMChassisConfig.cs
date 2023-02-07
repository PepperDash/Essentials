extern alias Full;

using System;
using System.Collections.Generic;
using Crestron.SimplSharpPro.DM;
using Full.Newtonsoft.Json;
using Full.Newtonsoft.Json.Converters;
using PepperDash.Core;
using PepperDash.Essentials.Core;

namespace PepperDash.Essentials.DM.Config
{
	/// <summary>
	/// Represents the "properties" property of a DM device config
	/// </summary>
	public class DMChassisPropertiesConfig
	{
		[JsonProperty("control")]
		public ControlPropertiesConfig Control { get; set; }

        [JsonProperty("volumeControls")]
        public Dictionary<uint, DmCardAudioPropertiesConfig> VolumeControls { get; set; }

        [JsonProperty("inputSlots")]
        public Dictionary<uint, string> InputSlots { get; set; }

        [JsonProperty("outputSlots")]
        public Dictionary<uint, string> OutputSlots { get; set; }

		[JsonProperty("inputNames")]
		public Dictionary<uint, string> InputNames { get; set; }

		[JsonProperty("outputNames")]
		public Dictionary<uint, string> OutputNames { get; set; }

        [JsonProperty("noRouteText")]
        public string NoRouteText { get; set; }

        [JsonProperty("inputSlotSupportsHdcp2")]
        public Dictionary<uint, bool> InputSlotSupportsHdcp2 { get; set; }

        public DMChassisPropertiesConfig()
        {
            InputSlotSupportsHdcp2 = new Dictionary<uint, bool>();
        }
    }

    /// <summary>
    /// 
    /// </summary>
    public class DmCardAudioPropertiesConfig
    {
        [JsonProperty("outLevel")]
        public int OutLevel { get; set; }

        [JsonProperty("isVolumeControlPoint")]
        public bool IsVolumeControlPoint { get; set; }
    }
}