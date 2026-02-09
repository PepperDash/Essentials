

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Crestron.SimplSharp;
using Newtonsoft.Json;

namespace PepperDash.Essentials.Core.Presets
{
 /// <summary>
 /// Represents a PresetChannel
 /// </summary>
	public class PresetChannel
	{

		/// <summary>
		/// Gets or sets the Name
		/// </summary>
		[JsonProperty(Required = Required.Always,PropertyName = "name")]
		public string Name { get; set; }

		/// <summary>
		/// Gets or sets the IconUrl
		/// </summary>
		[JsonProperty(Required = Required.Always, PropertyName = "iconUrl")]
		public string IconUrl { get; set; }

		/// <summary>
		/// Gets or sets the Channel
		/// </summary>
		[JsonProperty(Required = Required.Always, PropertyName = "channel")]
		public string Channel { get; set; }
	}

	/// <summary>
	/// Represents a PresetsList
	/// </summary>
	public class PresetsList
	{
		/// <summary>
		/// Gets or sets the Name
		/// </summary>
		[JsonProperty(Required=Required.Always,PropertyName = "name")]
		public string Name { get; set; }

		/// <summary>
		/// Gets or sets the Channels
		/// </summary>
		[JsonProperty(Required = Required.Always, PropertyName = "channels")]
		public List<PresetChannel> Channels { get; set; }
	}
}