using Newtonsoft.Json;

namespace PepperDash.Essentials
{
	public class SourceListConfigProperties
	{
		[JsonProperty(Required= Required.Always)]
		public uint Number { get; set; }
		[JsonProperty(Required= Required.Always)]
		public string SourceKey { get; set; }
		public string AltName { get; set; }
		public string AltIcon { get; set; }

		public SourceListConfigProperties()
		{
			AltName = "";
			AltIcon = "";
		}
	}
}