using System;

namespace PepperDash.Essentials.Core.Factory
{
    /// <summary>
    /// Represents a IsReadyEventArgs
    /// </summary>
    public class IsReadyEventArgs : EventArgs
    {
        /// <summary>
        /// Gets or sets the IsReady
        /// </summary>
        public bool IsReady { get; set; }

        public IsReadyEventArgs(bool data)
        {
            IsReady = data;
        }
    }

    /// <summary>
    /// Defines the contract for IHasReady
    /// </summary>
    public interface IHasReady
    {
        event EventHandler<IsReadyEventArgs> IsReadyEvent;
        bool IsReady { get; }
    }
}
