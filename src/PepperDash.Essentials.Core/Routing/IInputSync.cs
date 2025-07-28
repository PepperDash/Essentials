using System;

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
