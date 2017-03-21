using System;

using Newtonsoft.Json;

namespace PepperDash.Essentials.Core.Config
{
	/// <summary>
	/// Represents the info section of a Config file
	/// </summary>
	public class InfoConfig
	{
		[JsonProperty("name")]
		public string Name { get; set; }

		[JsonProperty("date")]
		public DateTime Date { get; set; }
		
		[JsonProperty("type")]
		public string Type { get; set; }
		
		[JsonProperty("version")]
		public string Version { get; set; }
		
		[JsonProperty("comment")]
		public string Comment { get; set; }

		public InfoConfig()
		{
			Name = "";
			Date = DateTime.Now;
			Type = "";
			Version = "";
			Comment = "";
		}
	}
}