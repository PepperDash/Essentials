using System;
using System.Collections.Generic;

namespace PepperDash.Essentials.Core.DeviceTypeInterfaces
{
    public interface IHasInputs
    {
        event EventHandler InputsUpdated;
        IDictionary<string, IInput> Inputs { get; } 
    }
}