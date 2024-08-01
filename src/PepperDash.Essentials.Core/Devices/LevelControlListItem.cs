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
    public class LevelControlListItem : AudioControlListItemBase
    {


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
        public string DeviceKey => DeviceManager.AllDevices.
            Where(d => d.Key.Contains(ParentDeviceKey) && d.Key.Contains(ItemKey)).FirstOrDefault()?.Key ?? $"{ParentDeviceKey}--{ItemKey}";

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
        Level = 1,
        Mute = 2,
        LevelAndMute = Level | Mute,
    }
}
