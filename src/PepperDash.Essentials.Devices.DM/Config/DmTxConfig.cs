using System;
using System.Collections.Generic;
using Crestron.SimplSharpPro.DM;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using PepperDash.Core;

namespace PepperDash.Essentials.DM.Config
{
	/// <summary>
	/// Represents the "properties" property of a DM TX device config
	/// </summary>
	public class DmTxPropertiesConfig
	{
		[JsonProperty("control")]
		public ControlPropertiesConfig Control { get; set; }

		[JsonProperty("parentDeviceKey")]
		public string ParentDeviceKey { get; set; }

		[JsonProperty("parentInputNumber")]
		public uint ParentInputNumber { get; set; }

		[JsonProperty("autoSwitching")]
		public bool AutoSwitching { get; set; }
	}
}