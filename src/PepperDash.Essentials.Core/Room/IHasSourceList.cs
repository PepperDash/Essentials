using System.Collections.Generic;
using PepperDash.Essentials.Core;


/// <summary>
/// Interface for rooms with a list of destinations
/// </summary>
interface IHasSourceList
{
    /// <summary>
    /// Gets the list of sources.
    /// </summary>
    Dictionary<string, SourceListItem> SourceList { get; }
}