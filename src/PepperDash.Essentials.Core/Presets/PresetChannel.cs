extern alias Full;

using System;
using System.Linq;
using System.Text;
using Crestron.SimplSharp;
using Full.Newtonsoft.Json;

namespace PepperDash.Essentials.Core.Presets
{
	public class PresetChannel
	{

		[JsonProperty(Required = Required.Always,PropertyName = "name")]
		public string Name { get; set; }

		[JsonProperty(Required = Required.Always, PropertyName = "iconUrl")]
		public string IconUrl { get; set; }

		[JsonProperty(Required = Required.Always, PropertyName = "channel")]
		public string Channel { get; set; }
	}
}