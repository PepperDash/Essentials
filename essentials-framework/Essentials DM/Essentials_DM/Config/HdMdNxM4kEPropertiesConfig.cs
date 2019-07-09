using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Crestron.SimplSharp;
using Newtonsoft.Json;

using PepperDash.Core;

namespace PepperDash.Essentials.DM.Config
{
	/// <summary>
	/// Defines the properties section of HdMdNxM boxes
	/// </summary>
	public class HdMdNxM4kEPropertiesConfig
	{
		[JsonProperty("control")]
		public ControlPropertiesConfig Control { get; set; }

		[JsonProperty("inputs")]
		public Dictionary<string, InputPropertiesConfig> Inputs { get; set; }
	}
}