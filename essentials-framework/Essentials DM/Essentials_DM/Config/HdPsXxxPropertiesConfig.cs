using System.Collections.Generic;
using Newtonsoft.Json;
using PepperDash.Core;

namespace PepperDash_Essentials_DM.Config
{
	public class HdPsXxxPropertiesConfig
	{
		[JsonProperty("control")]
		public ControlPropertiesConfig Control { get; set; }

		[JsonProperty("inputs")]
		public Dictionary<uint, string> Inputs { get; set; } 

		[JsonProperty("outputs")]
		public Dictionary<uint, string> Outputs { get; set; } 
	}
}