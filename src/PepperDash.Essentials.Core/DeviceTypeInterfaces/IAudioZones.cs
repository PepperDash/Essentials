using System.Collections.Generic;

namespace PepperDash.Essentials.Core
{
  /// <summary>
  /// Identifies a device that contains audio zones
  /// </summary>
  public interface IAudioZones : IRouting
  {
    Dictionary<uint, IAudioZone> Zone { get; }
  }
}