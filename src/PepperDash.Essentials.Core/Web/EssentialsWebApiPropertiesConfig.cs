using Newtonsoft.Json;

namespace PepperDash.Essentials.Core.Web
{
	/// <summary>
	/// Represents a EssentialsWebApiPropertiesConfig
	/// </summary>
	public class EssentialsWebApiPropertiesConfig
	{
		/// <summary>
		/// Gets or sets the BasePath
		/// </summary>
		[JsonProperty("basePath")]
		public string BasePath { get; set; }
	}
}