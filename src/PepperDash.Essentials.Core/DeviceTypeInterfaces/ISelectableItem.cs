using System;
using Newtonsoft.Json;
using PepperDash.Core;

namespace PepperDash.Essentials.Core.DeviceTypeInterfaces
{

    /// <summary>
    /// Defines the contract for ISelectableItem
    /// </summary>
    public interface ISelectableItem : IKeyName
    {
        /// <summary>
        /// Raised when the item is updated
        /// </summary>
        event EventHandler ItemUpdated;

        /// <summary>
        /// Gets or sets whether the item is selected
        /// </summary>
        [JsonProperty("isSelected")]
        bool IsSelected { get; set;  }

        /// <summary>
        /// Selects the item
        /// </summary>
        void Select();
    }
}