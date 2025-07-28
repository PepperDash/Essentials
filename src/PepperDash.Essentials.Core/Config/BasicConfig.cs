

using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json;

using Newtonsoft.Json.Linq;
using PepperDash.Essentials.Core.Devices;
using PepperDash.Essentials.Core.Routing;

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

        [JsonProperty("audioControlPointLists")]
        public Dictionary<string, AudioControlPointListItem> AudioControlPointLists { get; set; }

        [JsonProperty("cameraLists")]
        public Dictionary<string, Dictionary<string, CameraListItem>> CameraLists { get; set; }

        [JsonProperty("tieLines")]
  /// <summary>
  /// Gets or sets the TieLines
  /// </summary>
		public List<TieLineConfig> TieLines { get; set; }

        [JsonProperty("joinMaps")]
        public Dictionary<string, JObject> JoinMaps { get; set; }

        public BasicConfig()
        {
            Info = new InfoConfig();
            Devices = new List<DeviceConfig>();
            SourceLists = new Dictionary<string, Dictionary<string, SourceListItem>>();
            DestinationLists = new Dictionary<string, Dictionary<string, DestinationListItem>>();
            AudioControlPointLists = new Dictionary<string, AudioControlPointListItem>();
            CameraLists = new Dictionary<string, Dictionary<string, CameraListItem>>();
            TieLines = new List<TieLineConfig>();
            JoinMaps = new Dictionary<string, JObject>();
        }

		/// <summary>
		/// Checks SourceLists for a given list and returns it if found. Otherwise, returns null
		/// </summary>
		public Dictionary<string, SourceListItem> GetSourceListForKey(string key)
		{
			if (SourceLists == null || string.IsNullOrEmpty(key) || !SourceLists.ContainsKey(key))
				return null;
			
			return SourceLists[key];
		}

        /// <summary>
        /// Retrieves a DestinationListItem based on the key
        /// </summary>
        /// <param name="key">key of the list to retrieve</param>
        /// <returns>DestinationList if the key exists, null otherwise</returns>
	    public Dictionary<string, DestinationListItem> GetDestinationListForKey(string key)
	    {
	        if (DestinationLists == null || string.IsNullOrEmpty(key) || !DestinationLists.ContainsKey(key))
	        {
	            return null;
	        }

	        return DestinationLists[key];
	    }

        /// <summary>
        /// Retrieves a AudioControlPointList based on the key
        /// </summary>
        /// <param name="key">key of the list to retrieve</param>
        /// <returns>AudioControlPointList if the key exists, null otherwise</returns>
        /// <summary>
        /// GetAudioControlPointListForKey method
        /// </summary>
        public AudioControlPointListItem GetAudioControlPointListForKey(string key)
        {
            if (AudioControlPointLists == null ||  string.IsNullOrEmpty(key) || !AudioControlPointLists.ContainsKey(key))
                return null;

            return AudioControlPointLists[key];
        }

        /// <summary>
        /// Checks CameraLists for a given list and returns it if found. Otherwise, returns null
        /// </summary>
        public Dictionary<string, CameraListItem> GetCameraListForKey(string key)
        {
            if (CameraLists == null || string.IsNullOrEmpty(key) || !CameraLists.ContainsKey(key))
                return null;

            return CameraLists[key];
        }

        /// <summary>
        /// Checks Devices for an item with a Key that matches and returns it if found. Otherwise, retunes null
        /// </summary>
        /// <param name="key">Key of desired device</param>
        /// <returns></returns>
        /// <summary>
        /// GetDeviceForKey method
        /// </summary>
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