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

        /// <summary>
        /// Aux Faders as configured in the room
        /// </summary>
        [JsonProperty("auxFaders", NullValueHandling = NullValueHandling.Ignore)]
        public Dictionary<string, Volume> AuxFaders { get; set; }

        /// <summary>
        /// Count of aux faders for this system
        /// </summary>
        [JsonProperty("numberOfAuxFaders", NullValueHandling = NullValueHandling.Ignore)]
        public int? NumberOfAuxFaders { get; set; }
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

        /// <summary>
        /// Level for this volume object
        /// </summary>
        [JsonProperty("level", NullValueHandling = NullValueHandling.Ignore)]
        public int? Level { get; set; }

        /// <summary>
        /// True if this volume control is muted
        /// </summary>
        [JsonProperty("muted", NullValueHandling = NullValueHandling.Ignore)]
        public bool? Muted { get; set; }


        /// <summary>
        /// Gets or sets the Label
        /// </summary>
        [JsonProperty("label", NullValueHandling = NullValueHandling.Ignore)]
        public string Label { get; set; }

        /// <summary>
        /// True if this volume object has mute control
        /// </summary>
        [JsonProperty("hasMute", NullValueHandling = NullValueHandling.Ignore)]
        public bool? HasMute { get; set; }

        /// <summary>
        /// True if this volume object has Privacy mute control
        /// </summary>
        [JsonProperty("hasPrivacyMute", NullValueHandling = NullValueHandling.Ignore)]
        public bool? HasPrivacyMute { get; set; }

        /// <summary>
        /// True if the privacy mute is muted
        /// </summary>
        [JsonProperty("privacyMuted", NullValueHandling = NullValueHandling.Ignore)]
        public bool? PrivacyMuted { get; set; }



        /// <summary>
        /// Gets or sets the MuteIcon
        /// </summary>
        [JsonProperty("muteIcon", NullValueHandling = NullValueHandling.Ignore)]
        public string MuteIcon { get; set; }

        /// <summary>
        /// Create an instance of the <see cref="Volume" /> class
        /// </summary>
        /// <param name="key">The key for this volume object</param>
        /// <param name="level">The level for this volume object</param>
        /// <param name="muted">True if this volume control is muted</param>
        /// <param name="label">The label for this volume object</param>
        /// <param name="hasMute">True if this volume object has mute control</param>
        /// <param name="muteIcon">The mute icon for this volume object</param>
        public Volume(string key, int level, bool muted, string label, bool hasMute, string muteIcon)
            : this(key)
        {
            Level = level;
            Muted = muted;
            Label = label;
            HasMute = hasMute;
            MuteIcon = muteIcon;
        }

        /// <summary>
        /// Create an instance of the <see cref="Volume" /> class
        /// </summary>
        /// <param name="key">The key for this volume object</param>
        /// <param name="level">The level for this volume object</param>
        public Volume(string key, int level)
            : this(key)
        {
            Level = level;
        }

        /// <summary>
        /// Create an instance of the <see cref="Volume" /> class
        /// </summary>
        /// <param name="key">The key for this volume object</param>
        /// <param name="muted">True if this volume control is muted</param>
        public Volume(string key, bool muted)
            : this(key)
        {
            Muted = muted;
        }

        /// <summary>
        /// Create an instance of the <see cref="Volume" /> class
        /// </summary>
        /// <param name="key">The key for this volume object</param>
        public Volume(string key)
        {
            Key = key;
        }
    }
}