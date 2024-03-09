using System;
using PepperDash.Core;

namespace PepperDash.Essentials.Core.DeviceTypeInterfaces
{
    public interface IInput : IKeyName
    {
        event EventHandler InputUpdated;
        bool IsSelected { get; }
        void Select();
    }
}