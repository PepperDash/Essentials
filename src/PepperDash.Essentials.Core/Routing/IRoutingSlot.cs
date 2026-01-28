using PepperDash.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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
