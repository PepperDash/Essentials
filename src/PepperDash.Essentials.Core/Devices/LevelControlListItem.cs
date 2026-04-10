using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using PepperDash.Core;
using PepperDash.Essentials.Core.Devices;

namespace PepperDash.Essentials.Core
{
    /// <summary>
    /// Represents a level control item in a list, which can be used to control volume or mute functionality.
    /// </summary>
    public class LevelControlListItem : AudioControlListItemBase
    {

        /// <summary>
        /// A reference to the IBasicVolumeWithFeedback device for control.
        /// </summary>
        [JsonIgnore]
        public IBasicVolumeWithFeedback LevelControl
        {
            get
            {
                if (_levelControl == null)
                    _levelControl = DeviceManager.GetDeviceForKey(ParentDeviceKey) as IBasicVolumeWithFeedback;
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
        /// The key of the device in the DeviceManager for control
        /// </summary>
        [JsonProperty("deviceKey")]
        public string DeviceKey
        {
            get
            {
                if (string.IsNullOrEmpty(ItemKey)) return ParentDeviceKey;
                else
                {
                    return DeviceManager.AllDevices.
                    Where(d => d.Key.Contains(ParentDeviceKey) && d.Key.Contains(ItemKey)).FirstOrDefault()?.Key ?? $"{ParentDeviceKey}--{ItemKey}";
                }
            }
        }

        /// <summary>
        /// Indicates if the item is a level, mute , or both
        /// </summary>
        [JsonProperty("type")]
        [JsonConverter(typeof(Newtonsoft.Json.Converters.StringEnumConverter))]
        public eLevelControlType Type { get; set; }


        /// <summary>
        /// Indicates if the item is a mic or not.
        /// </summary>
        [JsonProperty("isMic", NullValueHandling = NullValueHandling.Ignore)]
        public bool? IsMic { get; set; }

        /// <summary>
        /// Indicates if the item should show the raw level in the UI.
        /// </summary>
        [JsonProperty("showRawLevel", NullValueHandling = NullValueHandling.Ignore)]
        public bool? ShowRawLevel { get; set; }
    }

    /// <summary>
    /// Indicates the type of level control item.
    /// </summary>
    [Flags]
    public enum eLevelControlType
    {
        /// <summary>
        /// Indicates that the item is a level control only
        /// </summary>
        Level = 1,
        /// <summary>
        /// Indicates that the item is a mute control only
        /// </summary>
        Mute = 2,
        /// <summary>
        /// Indicates that the item is both a level and mute control
        /// </summary>
        LevelAndMute = Level | Mute,
    }

}
