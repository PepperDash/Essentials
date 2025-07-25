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
        event EventHandler ItemUpdated;

        [JsonProperty("isSelected")]
        bool IsSelected { get; set;  }
        void Select();
    }
}