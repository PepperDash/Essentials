using Newtonsoft.Json;

namespace PepperDash.Essentials.Core.Web
{
	public class EssentialsWebApiPropertiesConfig
	{
		[JsonProperty("basePath")]
		public string BasePath { get; set; }
	}
}