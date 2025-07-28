using System.Collections.Generic;

namespace PepperDash.Essentials.Core.Routing
{
    /// <summary>
    /// Defines the contract for IMatrixRouting
    /// </summary>
    public interface IMatrixRouting
    {
        Dictionary<string, IRoutingInputSlot> InputSlots { get; }
        Dictionary<string, IRoutingOutputSlot> OutputSlots { get; }

        void Route(string inputSlotKey, string outputSlotKey, eRoutingSignalType type);
    }
}
