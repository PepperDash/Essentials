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
        /// <summary>
        /// Raised when the items are updated
        /// </summary>
        event EventHandler ItemsUpdated;

        /// <summary>
        /// Raised when the current item changes
        /// </summary>
        event EventHandler CurrentItemChanged;

        /// <summary>
        /// Gets or sets the collection of selectable items
        /// </summary>
        [JsonProperty("items")]
        Dictionary<TKey, TValue> Items { get; set; }

        /// <summary>
        /// Gets or sets the current selected item key
        /// </summary>
        [JsonProperty("currentItem")]
        TKey CurrentItem { get; set; }

    }

    /// <summary>
    /// Describes a collection of items that can be selected
    /// </summary>
    /// <typeparam name="TKey">type for the keys in the collection.  Probably a string or enum</typeparam>
    public interface ISelectableItems<TKey> : ISelectableItems<TKey, ISelectableItem>
    {        
    }
}