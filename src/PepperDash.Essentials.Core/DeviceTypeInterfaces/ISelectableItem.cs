using System;
using Newtonsoft.Json;
using PepperDash.Core;

namespace PepperDash.Essentials.Core.DeviceTypeInterfaces
{

    /// <summary>
    /// Describes an item that can be selected
    /// </summary>
    public interface ISelectableItem : IKeyName
    {
        event EventHandler ItemUpdated;

        [JsonProperty("isSelected")]
        bool IsSelected { get; }
        void Select();
    }
}