using System.Collections.Generic;

namespace PepperDash.Essentials.Core
{
  /// <summary>
  /// Identifies a device that contains audio zones
  /// </summary>
  public interface IAudioZones : IRouting
  {
    /// <summary>
    /// Gets the collection of audio zones
    /// </summary>
    Dictionary<uint, IAudioZone> Zone { get; }
  }
}