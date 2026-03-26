using System;
using System.Collections.Generic;

namespace PepperDash.Essentials.Core.Routing
{
  /// <summary>
  /// The current sources for the room, keyed by eRoutingSignalType.
  /// This allows for multiple sources to be tracked, such as audio and video.
  /// Intended to be implemented on a DestinationListItem to provide access to the current sources for the room in its context.
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
    Dictionary<eRoutingSignalType, IRoutingSource> CurrentSources { get; }

    /// <summary>
    /// Gets the current source keys for the room, keyed by eRoutingSignalType.
    /// This dictionary contains the keys for the current source for each signal type, such as audio, video, and control signals.
    /// </summary>
    Dictionary<eRoutingSignalType, string> CurrentSourceKeys { get; }

    /// <summary>
    /// Event raised when the current sources change.
    /// </summary>
    event EventHandler<CurrentSourcesChangedEventArgs> CurrentSourcesChanged;

    /// <summary>
    /// Sets the current source for a specific signal type.
    /// This method updates the current source for the specified signal type and notifies any subscribers of the change.
    /// </summary>
    /// <param name="signalType">The signal type to update.</param>
    /// <param name="sourceDevice">The source device to set as the current source.</param>
    void SetCurrentSource(eRoutingSignalType signalType, IRoutingSource sourceDevice);


  }


  /// <summary>
  /// Event arguments for the CurrentSourcesChanged event, providing details about the signal type and the previous and new sources.
  /// </summary>
  public class CurrentSourcesChangedEventArgs : EventArgs
  {
    /// <summary>
    /// Gets the signal type for which the current source has changed.
    /// </summary>
    public eRoutingSignalType SignalType { get; }

    /// <summary>
    /// Gets the previous source for the signal type before the change occurred.
    /// </summary>
    public IRoutingSource PreviousSource { get; }

    /// <summary>
    /// Gets the new source for the signal type after the change occurred.
    /// </summary>
    public IRoutingSource NewSource { get; }

    /// <summary>
    /// Initializes a new instance of the CurrentSourcesChangedEventArgs class with the specified signal type, previous source, and new source.
    /// </summary>
    /// <param name="signalType">The signal type for which the current source has changed.</param>
    /// <param name="previousSource">The previous source for the signal type before the change occurred.</param>
    /// <param name="newSource">The new source for the signal type after the change occurred.</param>
    public CurrentSourcesChangedEventArgs(eRoutingSignalType signalType, IRoutingSource previousSource, IRoutingSource newSource)
    {
      SignalType = signalType;
      PreviousSource = previousSource;
      NewSource = newSource;
    }
  }
}