extern alias Full;

using System;
using System.Collections.Generic;
using Crestron.SimplSharpPro.DM;
using Full.Newtonsoft.Json;
using Full.Newtonsoft.Json.Converters;
using PepperDash.Core;

namespace PepperDash.Essentials.DM.Config
{
	/// <summary>
	/// Represents the "properties" property of a DM TX device config
	/// </summary>
	public class DmRmcPropertiesConfig
	{
		[JsonProperty("control")]
		public ControlPropertiesConfig Control { get; set; }

		[JsonProperty("parentDeviceKey")]
		public string ParentDeviceKey { get; set; }

		[JsonProperty("parentOutputNumber")]
		public uint ParentOutputNumber { get; set; }
	}
}