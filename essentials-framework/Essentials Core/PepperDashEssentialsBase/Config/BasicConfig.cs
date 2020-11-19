using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Crestron.SimplSharp;
using Newtonsoft.Json;
using PepperDash.Core;
using PepperDash.Essentials.Core;
using PepperDash.Essentials.Core.Devices;

namespace PepperDash.Essentials.Core.Config
{
	/// <summary>
	///  Override this and splice on specific room type behavior, as well as other properties
	/// </summary>
	public class BasicConfig
	{
		[JsonProperty("info")]
		public InfoConfig Info { get; set; }

		[JsonProperty("devices")]
		public List<DeviceConfig> Devices { get; set; }

		[JsonProperty("sourceLists")]
		public Dictionary<string, Dictionary<string, SourceListItem>> SourceLists { get; set; }

        [JsonProperty("destinationLists")]
        public Dictionary<string, Dictionary<string, DestinationListItem>> DestinationLists { get; set; }
            
        [JsonProperty("tieLines")]
		public List<TieLineConfig> TieLines { get; set; }

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

	    public Dictionary<string, DestinationListItem> GetDestinationListForKey(string key)
	    {
	        if (string.IsNullOrEmpty(key) || !SourceLists.ContainsKey(key))
	            return null;

	        return DestinationLists[key];
	    }

        /// <summary>
        /// Checks Devices for an item with a Key that matches and returns it if found. Otherwise, retunes null
        /// </summary>
        /// <param name="key">Key of desired device</param>
        /// <returns></returns>
        public DeviceConfig GetDeviceForKey(string key)
        {
            if (string.IsNullOrEmpty(key))
                return null;

            var deviceConfig = Devices.FirstOrDefault(d => d.Key.Equals(key));

            //removed if statement that was here...
            //DeviceConfig will be null if it's not found in the list
            return deviceConfig;
        }
	}
}