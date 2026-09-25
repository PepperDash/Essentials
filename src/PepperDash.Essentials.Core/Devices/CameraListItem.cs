using System.Collections.Generic;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using PepperDash.Core;

namespace PepperDash.Essentials.Core;

/// <summary>
/// Represents an item in a camera list, which can be used to display camera sources in a user interface. Contains properties for the device key, preferred name, icon, and order of the item in the list. Also includes a property to get the associated camera device from the DeviceManager based on the device key.
/// </summary>
public class CameraListItem
{

    /// <summary>
    /// Key of the camera device in the DeviceManager
    /// </summary>
    [JsonProperty("deviceKey")]
    public string DeviceKey { get; set; }

    /// <summary>
    /// Returns the source Device for this, if it exists in DeviceManager
    /// </summary>
    [JsonIgnore]
    public Device CameraDevice
    {
        get
        {
            if (_cameraDevice == null)
                _cameraDevice = DeviceManager.GetDeviceForKey(DeviceKey) as Device;
            return _cameraDevice;
        }
    }
    Device _cameraDevice;

    /// <summary>
    /// Gets either the source's Name or this AlternateName property, if 
    /// defined.  If source doesn't exist, returns "Missing source"
    /// </summary>
    [JsonProperty("preferredName")]
    public string PreferredName
    {
        get
        {
            if (string.IsNullOrEmpty(Name))
            {
                if (CameraDevice == null)
                    return "---";
                return CameraDevice.Name;
            }
            return Name;
        }
        // Computed, but not get-only: with [JsonExtensionData] on this type, Newtonsoft treats an
        // unwritable member as unmatched and diverts the incoming value into CustomProperties,
        // which then re-serializes as a duplicate JSON key. The no-op setter keeps this a known
        // member so the config value is read and discarded, as it was before.
        private set { }
    }

    /// <summary>
    /// A name that will override the source's name on the UI
    /// </summary>
    [JsonProperty("name")]
    public string Name { get; set; }


    /// <summary>
    /// Specifies and icon for the source list item
    /// </summary>
    [JsonProperty("icon")]
    public string Icon { get; set; }

    /// <summary>
    /// Alternate icon
    /// </summary>
		[JsonProperty("altIcon", NullValueHandling = NullValueHandling.Ignore)]
    public string AltIcon { get; set; }

    /// <summary>
    /// Indicates if the item should be included in the user facing list
    /// </summary>
		[JsonProperty("includeInUserList")]
    public bool IncludeInUserList { get; set; }

    /// <summary>
    /// Used to specify the order of the items in the source list when displayed
    /// </summary>
		[JsonProperty("order")]
    public int Order { get; set; }

    /// <summary>
    /// Captures any properties present in the config JSON that this class does not define, so
    /// project-specific values survive deserialization instead of being discarded.
    /// </summary>
    /// <remarks>
    /// Newtonsoft writes these back out as top-level properties rather than nesting them under a
    /// "customProperties" object, so they round-trip through config and reach consuming clients
    /// alongside the framework's own properties. Null when the JSON contains no unrecognized
    /// properties.
    /// </remarks>
    [JsonExtensionData]
    public Dictionary<string, JToken> CustomProperties { get; set; }
}
