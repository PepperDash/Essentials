using System.Collections.Generic;

namespace PepperDash.Essentials.Core.CrestronIO
{
    public interface ISwitchedOutputCollection
    {
        Dictionary<uint, ISwitchedOutput> SwitchedOutputs { get; }
    }
}