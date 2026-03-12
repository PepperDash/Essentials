using System.Collections.Generic;
using PepperDash.Essentials.Core;


/// <summary>
/// Interface for rooms with a list of destinations
/// </summary>
public interface IHasDestinationList
{
    /// <summary>
    /// Gets the dictionary of destinations.
    /// </summary>
    Dictionary<string, IRoutingSink> Destinations { get; }
}