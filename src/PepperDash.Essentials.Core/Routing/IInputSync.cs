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
        bool VideoSyncDetected { get; }

        event EventHandler VideoSyncChanged;
    }
}
