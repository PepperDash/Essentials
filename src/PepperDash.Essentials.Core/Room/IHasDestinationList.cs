using System.Collections.Generic;

namespace PepperDash.Essentials.Core
{

    /// <summary>
    /// Interface for rooms with a list of destinations
    /// </summary>
    public interface IHasDestinations
    {
        /// <summary>
        /// Gets the dictionary of destinations.
        /// </summary>
        Dictionary<string, IRoutingSink> Destinations { get; }
    }
}