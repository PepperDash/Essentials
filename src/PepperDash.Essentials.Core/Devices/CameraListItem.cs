using Newtonsoft.Json;
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
}
