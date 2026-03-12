using System.Collections.Generic;
using PepperDash.Essentials.Core;


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