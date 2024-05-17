using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using PepperDash.Core;

namespace PepperDash.Essentials.Core
{
    public class LevelControlListItem
    {
        [JsonProperty("deviceKey")]
        public string DeviceKey { get; set; }

        [JsonIgnore]
        public IBasicVolumeWithFeedback LevelControl
        {
            get
            {
                if (_levelControl == null)
                    _levelControl = DeviceManager.GetDeviceForKey(DeviceKey) as IBasicVolumeWithFeedback;
                return _levelControl;
            }
        }
        IBasicVolumeWithFeedback _levelControl;

        /// <summary>
        /// Gets the name from the device if it implements IKeyName or else returns the Name property
        /// </summary>
        [JsonProperty("preferredName")]
        public string PreferredName
        {
            get
            {
                if (!string.IsNullOrEmpty(Name)) return Name;
                else 
                { 
                    if (LevelControl is IKeyName namedLevelControl)
                    {
                        if (namedLevelControl == null)
                            return "---";
                        return namedLevelControl.Name;
                    }
                    else return "---";
                }
            }
        }

        /// <summary>
        /// A name that will override the items's name on the UI
        /// </summary>
        [JsonProperty("name")]
        public string Name { get; set; }

        /// <summary>
        /// Indicates if the item should be included in the user accessible list
        /// </summary>
        [JsonProperty("includeInUserList")]
        public bool IncludeInUserList { get; set; }

        /// <summary>
        /// Used to specify the order of the items in the source list when displayed
        /// </summary>
        [JsonProperty("order")]
        public int Order { get; set; }

        /// <summary>
        /// Indicates if the item is a level, mute , or both
        /// </summary>
        [JsonProperty("type")]
        [JsonConverter(typeof(Newtonsoft.Json.Converters.StringEnumConverter))]
        public eLevelControlType Type { get; set; }
    }

    [Flags]
    public enum eLevelControlType
    {
        Level = 0,
        Mute = 1,
        LevelAndMute = Level | Mute,
    }
}
