using System.Collections.Generic;
using Newtonsoft.Json;

namespace PepperDash.Essentials.Core.Presets
{
 /// <summary>
 /// Represents a PresetChannel
 /// </summary>
	public class PresetChannel
	{

		[JsonProperty(Required = Required.Always,PropertyName = "name")]
  /// <summary>
  /// Gets or sets the Name
  /// </summary>
		public string Name { get; set; }

		[JsonProperty(Required = Required.Always, PropertyName = "iconUrl")]
  /// <summary>
  /// Gets or sets the IconUrl
  /// </summary>
		public string IconUrl { get; set; }

		[JsonProperty(Required = Required.Always, PropertyName = "channel")]
  /// <summary>
  /// Gets or sets the Channel
  /// </summary>
		public string Channel { get; set; }
	}

 /// <summary>
 /// Represents a PresetsList
 /// </summary>
	public class PresetsList
	{
		[JsonProperty(Required=Required.Always,PropertyName = "name")]
  /// <summary>
  /// Gets or sets the Name
  /// </summary>
		public string Name { get; set; }

		[JsonProperty(Required = Required.Always, PropertyName = "channels")]
  /// <summary>
  /// Gets or sets the Channels
  /// </summary>
		public List<PresetChannel> Channels { get; set; }
	}
}