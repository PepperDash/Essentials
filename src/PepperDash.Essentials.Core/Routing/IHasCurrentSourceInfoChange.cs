/* Unmerged change from project 'PepperDash.Essentials.Core (net6)'
Before:
namespace PepperDash.Essentials.Core.Routing.Interfaces
After:
using PepperDash;
using PepperDash.Essentials;
using PepperDash.Essentials.Core;
using PepperDash.Essentials.Core.Routing;
using PepperDash.Essentials.Core.Routing;
using PepperDash.Essentials.Core.Routing.Interfaces
*/
using PepperDash.Essentials.Core.Devices;
using System;

namespace PepperDash.Essentials.Core.Routing
{
    /// <summary>
    /// Delegate for SourceInfoChangeHandler
    /// </summary>
    public delegate void SourceInfoChangeHandler(SourceListItem info, ChangeType type);
    //*******************************************************************************************
    // Interfaces

    /// <summary>
    /// For rooms with a single presentation source, change event
    /// </summary>
    [Obsolete("Use ICurrentSources instead")]
    public interface IHasCurrentSourceInfoChange
    {
        /// <summary>
        /// The key for the current source info, used to look up the source in the SourceList
        /// </summary>
        string CurrentSourceInfoKey { get; set; }

        /// <summary>
        /// The current source info for the room, used to look up the source in the SourceList
        /// </summary>
        SourceListItem CurrentSourceInfo { get; set; }

        /// <summary>
        /// Event that is raised when the current source info changes.
        /// This is used to notify the system of changes to the current source info.
        /// The event handler receives the new source info and the type of change that occurred.
        /// </summary>
        event SourceInfoChangeHandler CurrentSourceChange;
    }
}