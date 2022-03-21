using System.Collections.Generic;

namespace PepperDash.Essentials.Core.Interfaces
{
    public interface IHasSwitchedOutputs
    {
        Dictionary<uint, ISwitchedOutput> SwitchedOutputs { get; }
    }
}