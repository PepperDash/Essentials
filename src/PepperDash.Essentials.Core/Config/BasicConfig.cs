

using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json;

using Newtonsoft.Json.Linq;

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
        public Dictionary<string, Dictionary<string,DestinationListItem>> DestinationLists { get; set; }

        [JsonProperty("tieLines")]
		public List<TieLineConfig> TieLines { get; set; }

        [JsonProperty("joinMaps")]
        public Dictionary<string, JObject> JoinMaps { get; set; }

        public BasicConfig()
        {
            Info = new InfoConfig();
            Devices = new List<DeviceConfig>();
            SourceLists = new Dictionary<string, Dictionary<string, SourceListItem>>();
            DestinationLists = new Dictionary<string, Dictionary<string, DestinationListItem>>();
            TieLines = new List<TieLineConfig>();
            JoinMaps = new Dictionary<string, JObject>();
        }

		/// <summary>
		/// Checks SourceLists for a given list and returns it if found. Otherwise, returns null
		/// </summary>
		public Dictionary<string, SourceListItem> GetSourceListForKey(string key)
		{
			if (string.IsNullOrEmpty(key) || !SourceLists.ContainsKey(key))
				return null;
			
			return SourceLists[key];
		}

        /// <summary>
        /// Retrieves a DestinationListItem based on the key
        /// </summary>
        /// <param name="key">key of the item to retrieve</param>
        /// <returns>DestinationListItem if the key exists, null otherwise</returns>
	    public Dictionary<string, DestinationListItem> GetDestinationListForKey(string key)
	    {
	        if (string.IsNullOrEmpty(key) || !DestinationLists.ContainsKey(key))
	        {
	            return null;
	        }

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

            if (deviceConfig != null)
                return deviceConfig;
            else
            {
                return null;
            }
        }
	}
}