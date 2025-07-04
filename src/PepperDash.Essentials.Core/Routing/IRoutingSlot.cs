using PepperDash.Core;

namespace PepperDash.Essentials.Core.Routing
{
    /// <summary>
    /// Defines the contract for IRoutingSlot
    /// </summary>
    public interface IRoutingSlot:IKeyName
    {
        /// <summary>
        /// Gets the slot number
        /// </summary>
        int SlotNumber { get; }

        /// <summary>
        /// Gets the supported signal types
        /// </summary>
        eRoutingSignalType SupportedSignalTypes { get; }
    }
}
