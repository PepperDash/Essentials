using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PepperDash.Essentials.Core;

/// <summary>
/// Base class for items that can be added to an AudioControlList. Contains properties that are common to all items, such as the parent device key and an optional item key. Also includes properties for display purposes, such as a name and order.
/// </summary>
public abstract class AudioControlListItemBase
{
    /// <summary>
    /// Key of the parent device in the DeviceManager
    /// </summary>
    [JsonProperty("parentDeviceKey")]
    public string ParentDeviceKey { get; set; }

    /// <summary>
    /// Optional key of the item in the parent device
    /// </summary>
    [JsonProperty("itemKey")]
    public string ItemKey { get; set; }

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
