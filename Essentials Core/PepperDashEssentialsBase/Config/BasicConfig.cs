using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Crestron.SimplSharp;
using Newtonsoft.Json;
using PepperDash.Core;
using PepperDash.Essentials.Core;

namespace PepperDash.Essentials.Core.Config
{
	/// <summary>
	///  Override this and splice on specific room type behavior, as well as other properties
	/// </summary>
	public class BasicConfig
	{
		[JsonProperty("info")]
		public InfoConfig Info { get; set; }

		//[JsonProperty("roomLists")]
		//public Dictionary<string, List<string>> RoomLists { get; set; }

		[JsonProperty("devices")]
		public List<DeviceConfig> Devices { get; set; }

		[JsonProperty("sourceLists")]
		public Dictionary<string, Dictionary<string, SourceListItem>> SourceLists { get; set; }

		[JsonProperty("tieLines")]
		public List<TieLineConfig> TieLines { get; set; }

		/// <summary>
		/// Checks SourceLists for a given list and returns it if found. Otherwise, returns null
		/// </summary>
		public Dictionary<string, SourceListItem> GetSourceListForKey(string key)
		{
			if (string.IsNullOrEmpty(key) || !SourceLists.ContainsKey(key))
				return null;
			
			return SourceLists[key];
		}
	}
}