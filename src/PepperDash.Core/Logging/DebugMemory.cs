extern alias NewtonsoftJson;

using System.Collections.Generic;
using System.Threading;
using Crestron.SimplSharp;
using JsonProperty = NewtonsoftJson::Newtonsoft.Json.JsonPropertyAttribute;

namespace PepperDash.Core.Logging;

/// <summary>
/// Class to persist current Debug settings across program restarts
/// </summary>
public class DebugContextCollection
{
    /// <summary>
    /// To prevent threading issues with the DeviceDebugSettings collection
    /// </summary>
    private readonly object _deviceDebugSettingsLock = new();

    [JsonProperty("items")]
    private readonly Dictionary<string, DebugContextItem> _items = new Dictionary<string, DebugContextItem>();

    /// <summary>
    /// Collection of the debug settings for each device where the dictionary key is the device key
    /// </summary>
    [JsonProperty("deviceDebugSettings")]
    private Dictionary<string, object> DeviceDebugSettings { get; set; } = new Dictionary<string, object>();


    /// <summary>
    /// Default constructor
    /// </summary>
    public DebugContextCollection()
    {

    }

    /// <summary>
    /// Sets the level of a given context item, and adds that item if it does not 
    /// exist
    /// </summary>
    /// <param name="contextKey"></param>
    /// <param name="level"></param>
    /// <summary>
    /// SetLevel method
    /// </summary>
    public void SetLevel(string contextKey, int level)
    {
        if (level < 0 || level > 2)
            return;
        GetOrCreateItem(contextKey).Level = level;
    }

    /// <summary>
    /// Gets a level or creates it if not existing
    /// </summary>
    /// <param name="contextKey"></param>
    /// <returns></returns>
    /// <summary>
    /// GetOrCreateItem method
    /// </summary>
    public DebugContextItem GetOrCreateItem(string contextKey)
    {
        if (!_items.ContainsKey(contextKey))
            _items[contextKey] = new DebugContextItem { Level = 0 };
        return _items[contextKey];
    }


    /// <summary>
    /// sets the settings for a device or creates a new entry
    /// </summary>
    /// <param name="deviceKey"></param>
    /// <param name="settings"></param>
    /// <returns></returns>
    public void SetDebugSettingsForKey(string deviceKey, object settings)
    {
        lock (_deviceDebugSettingsLock)
        {
            if (DeviceDebugSettings.ContainsKey(deviceKey))
            {
                DeviceDebugSettings[deviceKey] = settings;
            }
            else
                DeviceDebugSettings.Add(deviceKey, settings);
        }
    }

    /// <summary>
    /// Gets the device settings for a device by key or returns null
    /// </summary>
    /// <param name="deviceKey"></param>
    /// <returns></returns>
    public object GetDebugSettingsForKey(string deviceKey)
    {
        return DeviceDebugSettings[deviceKey];
    }
}

/// <summary>
/// Contains information about 
/// </summary>
public class DebugContextItem
{
    /// <summary>
    /// The level of debug messages to print
    /// </summary>
    [JsonProperty("level")]
    public int Level { get; set; }

    /// <summary>
    /// Property to tell the program not to intitialize when it boots, if desired
    /// </summary>
    [JsonProperty("doNotLoadOnNextBoot")]
    public bool DoNotLoadOnNextBoot { get; set; }
}