using System.Collections.Generic;

namespace PepperDash.Essentials.Core
{
    /// <summary>
    /// For rooms with multiple displays
    /// </summary>
    public interface IHasMultipleDisplays
    {
        Dictionary<eSourceListItemDestinationTypes, IRoutingSinkWithSwitching> Displays { get; }
    }
}