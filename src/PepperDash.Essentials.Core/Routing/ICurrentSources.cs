using System.Collections.Generic;

namespace PepperDash.Essentials.Core.Routing
{
  /// <summary>
  /// The current sources for the room, keyed by eRoutingSignalType.
  /// This allows for multiple sources to be tracked, such as audio and video.
  /// </summary>
  /// <remarks>
  /// This interface is used to provide access to the current sources in a room,
  /// allowing for more complex routing scenarios where multiple signal types are involved.
  /// </remarks>
  public interface ICurrentSources
  {
    /// <summary>
    /// Gets the current sources for the room, keyed by eRoutingSignalType.
    /// This dictionary contains the current source for each signal type, such as audio, video,
    /// </summary>
    Dictionary<eRoutingSignalType, SourceListItem> CurrentSources { get; }

    /// <summary>
    /// Gets the current source keys for the room, keyed by eRoutingSignalType.
    /// This dictionary contains the keys for the current source for each signal type, such as audio,
    /// </summary>
    Dictionary<eRoutingSignalType, string> CurrentSourceKeys { get; }

  }
}
