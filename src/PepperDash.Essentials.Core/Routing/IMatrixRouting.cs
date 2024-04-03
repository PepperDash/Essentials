using System.Collections.Generic;

namespace PepperDash.Essentials.Core.Routing
{
    public interface IMatrixRouting<TInput, TOutput> where TInput : IRoutingInputSlot where TOutput : IRoutingOutputSlot<TInput>
    {
        Dictionary<string, TInput> InputSlots { get; }
        Dictionary<string, TOutput> OutputSlots { get; }

        void Route(string inputSlotKey, string outputSlotKey, eRoutingSignalType type);
    }
}
