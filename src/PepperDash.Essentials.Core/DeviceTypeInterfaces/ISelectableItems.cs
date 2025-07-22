using Newtonsoft.Json;
using System;
using System.Collections.Generic;

namespace PepperDash.Essentials.Core.DeviceTypeInterfaces
{
    /// <summary>
    /// Defines the contract for ISelectableItems
    /// </summary>
    public interface ISelectableItems<TKey, TValue> where TValue : ISelectableItem
    {
        event EventHandler ItemsUpdated;
        event EventHandler CurrentItemChanged;

        [JsonProperty("items")]
        Dictionary<TKey, TValue> Items { get; set; }

        [JsonProperty("currentItem")]
        TKey CurrentItem { get; set; }

    }

    /// <summary>
    /// Describes a collection of items that can be selected
    /// </summary>
    /// <typeparam name="TKey">type for the keys in the collection.  Probably a string or enum</typeparam>
    /// <summary>
    /// Defines the contract for ISelectableItems
    /// </summary>
    public interface ISelectableItems<TKey> : ISelectableItems<TKey, ISelectableItem>
    {        
    }
}