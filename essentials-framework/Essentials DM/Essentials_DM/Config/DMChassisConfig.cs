using System;
using System.Collections.Generic;
using Crestron.SimplSharpPro.DM;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using PepperDash.Core;
using PepperDash.Essentials.Core;
using PepperDash.Essentials.DM.Cards;

namespace PepperDash.Essentials.DM.Config
{
	/// <summary>
	/// Represents the "properties" property of a DM device config
	/// </summary>
	public class DMChassisPropertiesConfig
	{
		[JsonProperty("control")]
		public ControlPropertiesConfig Control { get; set; }

        /// <summary>
        /// The available volume controls
        /// </summary>
        [JsonProperty("volumeControls")]
        public Dictionary<uint, DmCardAudioPropertiesConfig> VolumeControls { get; set; }

        /// <summary>
        /// The input cards
        /// </summary>
        [JsonProperty("inputSlots")]
        public Dictionary<uint, string> InputSlots { get; set; }

        /// <summary>
        /// The output cards (each card represents a pair of outputs)
        /// </summary>
        [JsonProperty("outputSlots")]
        public Dictionary<uint, string> OutputSlots { get; set; }

        /// <summary>
        /// The names of the Inputs
        /// </summary>
		[JsonProperty("inputNames")]
		public Dictionary<uint, string> InputNames { get; set; }

        /// <summary>
        /// The names of the Outputs
        /// </summary>
		[JsonProperty("outputNames")]
		public Dictionary<uint, string> OutputNames { get; set; }

        /// <summary>
        /// The string to use when no route is set for a given output
        /// </summary>
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
        /// <summary>
        /// The level to set on the output
        /// </summary>
        [JsonProperty("outLevel")]
        public int OutLevel { get; set; }

        /// <summary>
        /// Defines if this level is adjustable or not
        /// </summary>
        [JsonProperty("isVolumeControlPoint")]
        public bool IsVolumeControlPoint { get; set; }
    }
}