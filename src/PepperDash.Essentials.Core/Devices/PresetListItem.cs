using Newtonsoft.Json;
using PepperDash.Core;

namespace PepperDash.Essentials.Core;

/// <summary>
/// Represents a PresetListItem
/// </summary>

public class PresetListItem : AudioControlListItemBase
{
    /// <summary>
    /// Gets the preset associated with this list item
    /// </summary>
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
        // Computed, but not get-only: with [JsonExtensionData] on this type, Newtonsoft treats an
        // unwritable member as unmatched and diverts the incoming value into CustomProperties,
        // which then re-serializes as a duplicate JSON key. The no-op setter keeps this a known
        // member so the config value is read and discarded, as it was before.
        private set { }
    }
}
