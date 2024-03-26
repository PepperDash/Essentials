using System.Collections.Generic;

namespace PepperDash.Essentials.Core.Routing
{
    public interface IMatrixRouting
    {
        Dictionary<string, RoutingInputSlotBase> InputSlots { get; }
        Dictionary<string, RoutingOutputSlotBase> OutputSlots { get; }

        void Route(string inputSlotKey, string outputSlotKey, eRoutingSignalType type);
    }
}
