using Newtonsoft.Json;

namespace PepperDash.Essentials.Core.Web
{
 /// <summary>
 /// Represents a EssentialsWebApiPropertiesConfig
 /// </summary>
	public class EssentialsWebApiPropertiesConfig
	{
		[JsonProperty("basePath")]
  /// <summary>
  /// Gets or sets the BasePath
  /// </summary>
		public string BasePath { get; set; }
	}
}