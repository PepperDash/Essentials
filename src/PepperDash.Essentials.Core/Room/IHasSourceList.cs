using System.Collections.Generic;
namespace PepperDash.Essentials.Core
{

    /// <summary>
    /// Interface for rooms with a list of destinations
    /// </summary>
    public interface IHasSourceList
    {
        /// <summary>
        /// Gets the list of sources.
        /// </summary>
        Dictionary<string, SourceListItem> SourceList { get; }
    }
}