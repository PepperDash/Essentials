using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Crestron.SimplSharp;
using Newtonsoft.Json;

namespace PepperDash.Essentials.Core.Presets
{
	public class PresetChannel
	{

		[JsonProperty(Required = Required.Always)]
		public string Name { get; set; }
		[JsonProperty(Required = Required.Always)]
		public string IconUrl { get; set; }
		[JsonProperty(Required = Required.Always)]
		public string Channel { get; set; }
	}

	public class PresetsList
	{
		[JsonProperty(Required=Required.Always)]
		public string Name { get; set; }
		[JsonProperty(Required = Required.Always)]
		public List<PresetChannel> Channels { get; set; }
	}
}