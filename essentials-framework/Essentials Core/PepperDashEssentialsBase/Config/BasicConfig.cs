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

        /// <summary>
        /// Defines the devices in the system
        /// </summary>
		[JsonProperty("devices")]
		public List<DeviceConfig> Devices { get; set; }

        /// <summary>
        /// Defines the source list for the system
        /// </summary>
		[JsonProperty("sourceLists")]
		public Dictionary<string, Dictionary<string, SourceListItem>> SourceLists { get; set; }

        /// <summary>
        /// Defines all the tie lines for system routing
        /// </summary>
		[JsonProperty("tieLines")]
		public List<TieLineConfig> TieLines { get; set; }

        /// <summary>
        /// Defines any join maps to override the default join maps
        /// </summary>
        [JsonProperty("joinMaps")]
        public Dictionary<string, string> JoinMaps { get; set; }

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