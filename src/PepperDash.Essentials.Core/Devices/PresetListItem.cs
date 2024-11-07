using Newtonsoft.Json;
using PepperDash.Core;

namespace PepperDash.Essentials.Core.Devices
{
    public class PresetListItem : AudioControlListItemBase
    {
        [JsonIgnore]
        public IKeyName Preset
        {
            get
            {
                if (_preset == null)
                {
                    var parent = DeviceManager.GetDeviceForKey(ParentDeviceKey) as IDspPresets;
                    if (parent == null || !parent.Presets.ContainsKey(ItemKey))
                        return null;
                    _preset = parent.Presets[ItemKey];
                }
                return _preset;
            }
        }
        private IKeyName _preset;

        /// <summary>
        /// Gets the name from the device if it implements IKeyName or else returns the Name property
        /// </summary>
        [JsonProperty("preferredName")]
        public string PreferredName
        {
            get
            {
                if (!string.IsNullOrEmpty(Name)) return Name;

                else return Preset.Name;
            }
        }
    }
}
