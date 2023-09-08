using System.Collections.Generic;
using Newtonsoft.Json;
using PepperDash.Core;
using PepperDash.Essentials.DM.Config;

namespace PepperDash_Essentials_DM.Config
{
	public class HdPsXxxPropertiesConfig
	{
		[JsonProperty("control")]
		public ControlPropertiesConfig Control { get; set; }

		[JsonProperty("inputs")]
		//public Dictionary<uint, InputPropertiesConfig> Inputs { get; set; } 
		public Dictionary<uint, string> Inputs { get; set; } 

		[JsonProperty("outputs")]
		//public Dictionary<uint, InputPropertiesConfig> Outputs { get; set; }
		public Dictionary<uint, string> Outputs { get; set; }

		public HdPsXxxPropertiesConfig()
		{
			//Inputs = new Dictionary<uint, InputPropertiesConfig>();
			//Outputs = new Dictionary<uint, InputPropertiesConfig>();

			Inputs = new Dictionary<uint, string>();
			Outputs = new Dictionary<uint, string>();
		}
	}
}