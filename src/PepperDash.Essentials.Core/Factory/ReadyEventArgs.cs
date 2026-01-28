using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Crestron.SimplSharp;
using PepperDash.Essentials.Core;

namespace PepperDash.Essentials.Core
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

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="data">indicates if the object is ready</param>
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
        /// <summary>
        /// Fires when the IsReady property changes
        /// </summary>
        event EventHandler<IsReadyEventArgs> IsReadyEvent;

        /// <summary>
        /// indicates whether the object is ready
        /// </summary>
        bool IsReady { get; }
    }
}
