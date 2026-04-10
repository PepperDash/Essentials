using System;
using System.Collections.Generic;

namespace PepperDash.Essentials.Core.Routing
{
    /// <summary>
    /// Defines the contract for IRoutingOutputSlot
    /// </summary>
    public interface IRoutingOutputSlot : IRoutingSlot
    {
        /// <summary>
        /// Event raised when output slot changes
        /// </summary>
        event EventHandler OutputSlotChanged;

        /// <summary>
        /// Gets the Rx device key
        /// </summary>
        string RxDeviceKey { get; }

        /// <summary>
        /// Gets the current routes
        /// </summary>
        Dictionary<eRoutingSignalType, IRoutingInputSlot> CurrentRoutes { get; }
    }    
}
