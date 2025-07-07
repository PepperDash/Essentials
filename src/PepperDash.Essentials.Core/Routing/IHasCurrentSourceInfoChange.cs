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
namespace PepperDash.Essentials.Core;

/// <summary>
/// The handler type for a Room's SourceInfoChange
/// </summary>
public delegate void SourceInfoChangeHandler(SourceListItem info, ChangeType type);
//*******************************************************************************************
// Interfaces

/// <summary>
/// For rooms with a single presentation source, change event
/// </summary>
public interface IHasCurrentSourceInfoChange
{
    string CurrentSourceInfoKey { get; set; }
    SourceListItem CurrentSourceInfo { get; set; }
    event SourceInfoChangeHandler CurrentSourceChange;
}