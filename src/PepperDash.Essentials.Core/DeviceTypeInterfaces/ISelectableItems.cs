using Newtonsoft.Json;
using System;
using System.Collections.Generic;

namespace PepperDash.Essentials.Core.DeviceTypeInterfaces
{
    /// <summary>
    /// Describes a collection of items that can be selected
    /// </summary>
    /// <typeparam name="TKey">type for the keys in the collection.  Probably a string or enum</typeparam>
    public interface ISelectableItems<TKey>
    {
        event EventHandler ItemsUpdated;
        event EventHandler CurrentItemChanged;


        Dictionary<TKey, ISelectableItem> Items { get; }

        [JsonProperty("currentItem")]
        string CurrentItem { get; }
    }
}