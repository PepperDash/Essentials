using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Crestron.SimplSharp;

using PepperDash.Essentials.Core;

namespace PepperDash.Essentials.Core
{
    /// <summary>
    /// Defines the contract for IOccupancyStatusProvider
    /// </summary>
    public interface IOccupancyStatusProvider
    {
        /// <summary>
        /// Gets the RoomIsOccupiedFeedback
        /// </summary>
        BoolFeedback RoomIsOccupiedFeedback { get; }
    }
}