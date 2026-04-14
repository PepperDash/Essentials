

using System.Collections.Generic;

namespace PepperDash.Essentials.Core.DeviceTypeInterfaces;

/// <summary>
/// Defines the contract for devices that can report initialization exceptions. 
/// </summary>
public interface IInitializationExceptions
{
    /// <summary>
    /// Gets a list of exceptions that occurred during the initialization of the program.
    /// </summary>
    List<System.Exception> InitializationExceptions { get; }
}