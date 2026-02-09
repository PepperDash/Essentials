using System.Collections.Generic;

namespace PepperDash.Essentials.Core.Routing
{
    /// <summary>
    /// Defines the contract for IMatrixRouting
    /// </summary>
    public interface IMatrixRouting
    {
        /// <summary>
        /// Gets the input slots
        /// </summary>
        Dictionary<string, IRoutingInputSlot> InputSlots { get; }

        /// <summary>
        /// Gets the output slots
        /// </summary>
        Dictionary<string, IRoutingOutputSlot> OutputSlots { get; }

        /// <summary>
        /// Routes the specified input slot to the specified output slot for the specified signal type
        /// </summary>
        /// <param name="inputSlotKey">key of the input slot</param>
        /// <param name="outputSlotKey">key of the output slot</param>
        /// <param name="type">signal type</param>
        void Route(string inputSlotKey, string outputSlotKey, eRoutingSignalType type);
    }
}
