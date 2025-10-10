using System.Collections.Generic;
using Newtonsoft.Json;

namespace PepperDash.Essentials
{
    /// <summary>
    /// Represents a Volumes
    /// </summary>
    public class Volumes
    {

        /// <summary>
        /// Gets or sets the Master
        /// </summary>
        [JsonProperty("master", NullValueHandling = NullValueHandling.Ignore)]
        public Volume Master { get; set; }

        [JsonProperty("auxFaders", NullValueHandling = NullValueHandling.Ignore)]
        public Dictionary<string, Volume> AuxFaders { get; set; }

        [JsonProperty("numberOfAuxFaders", NullValueHandling = NullValueHandling.Ignore)]
        public int? NumberOfAuxFaders { get; set; }

        public Volumes()
        {
        }
    }

    /// <summary>
    /// Represents a Volume
    /// </summary>
    public class Volume
    {

        /// <summary>
        /// Gets or sets the Key
        /// </summary>
        [JsonProperty("key", NullValueHandling = NullValueHandling.Ignore)]
        public string Key { get; set; }

        [JsonProperty("level", NullValueHandling = NullValueHandling.Ignore)]
        public int? Level { get; set; }

        [JsonProperty("muted", NullValueHandling = NullValueHandling.Ignore)]
        public bool? Muted { get; set; }


        /// <summary>
        /// Gets or sets the Label
        /// </summary>
        [JsonProperty("label", NullValueHandling = NullValueHandling.Ignore)]
        public string Label { get; set; }

        [JsonProperty("hasMute", NullValueHandling = NullValueHandling.Ignore)]
        public bool? HasMute { get; set; }

        [JsonProperty("hasPrivacyMute", NullValueHandling = NullValueHandling.Ignore)]
        public bool? HasPrivacyMute { get; set; }

        [JsonProperty("privacyMuted", NullValueHandling = NullValueHandling.Ignore)]
        public bool? PrivacyMuted { get; set; }



        /// <summary>
        /// Gets or sets the MuteIcon
        /// </summary>
        [JsonProperty("muteIcon", NullValueHandling = NullValueHandling.Ignore)]
        public string MuteIcon { get; set; }

        public Volume(string key, int level, bool muted, string label, bool hasMute, string muteIcon)
            : this(key)
        {
            Level = level;
            Muted = muted;
            Label = label;
            HasMute = hasMute;
            MuteIcon = muteIcon;
        }

        public Volume(string key, int level)
            : this(key)
        {
            Level = level;
        }

        public Volume(string key, bool muted)
            : this(key)
        {
            Muted = muted;
        }

        public Volume(string key)
        {
            Key = key;
        }
    }
}