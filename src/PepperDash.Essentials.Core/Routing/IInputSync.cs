using PepperDash.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PepperDash.Essentials.Core.Routing
{
    /// <summary>
    /// Defines the contract for IVideoSync
    /// </summary>
    public interface IVideoSync : IKeyed
    {
        /// <summary>
        /// Gets whether or not video sync is detected
        /// </summary>
        bool VideoSyncDetected { get; }

        /// <summary>
        /// Event raised when video sync changes
        /// </summary>
        event EventHandler VideoSyncChanged;
    }
}
