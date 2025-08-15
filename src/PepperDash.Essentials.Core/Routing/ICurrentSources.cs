using System;
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
    /// This dictionary contains the current source for each signal type, such as audio, video, and control signals.
    /// </summary>
    Dictionary<eRoutingSignalType, SourceListItem> CurrentSources { get; }

    /// <summary>
    /// Gets the current source keys for the room, keyed by eRoutingSignalType.
    /// This dictionary contains the keys for the current source for each signal type, such as audio, video, and control signals.
    /// </summary>
    Dictionary<eRoutingSignalType, string> CurrentSourceKeys { get; }

    /// <summary>
    /// Event raised when the current sources change.
    /// </summary>
    event EventHandler CurrentSourcesChanged;

    /// <summary>
    /// Sets the current source for a specific signal type.
    /// This method updates the current source for the specified signal type and notifies any subscribers of the change.
    /// </summary>
    /// <param name="signalType">The signal type to update.</param>
    /// <param name="sourceListKey">The key for the source list.</param>
    /// <param name="sourceListItem">The source list item to set as the current source.</param>
    void SetCurrentSource(eRoutingSignalType signalType, string sourceListKey, SourceListItem sourceListItem);

  }
}
