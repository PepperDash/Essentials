using System;

namespace PepperDash.Essentials.Core.DeviceTypeInterfaces;

/// <summary>
/// Defines the contract for a wireless presentation endpoint that reports whether a wireless
/// sharing session is currently active. Implemented by platforms such as Crestron AirMedia,
/// Mersive Solstice, Barco ClickShare, Miracast/Teams receivers, etc. Allows consumers (e.g.
/// room plugins) to react to wireless sharing activity without taking a dependency on any
/// concrete device implementation.
/// </summary>
/// <remarks>
/// This refers specifically to wireless screen/device mirroring, as distinct from in-call
/// content sharing on a video conference.
/// </remarks>
public interface IHasWirelessSharing
{
    /// <summary>
    /// Reports whether a wireless sharing session is currently active (content is being presented).
    /// </summary>
    BoolFeedback IsSharingFeedback { get; }

    /// <summary>
    /// Raised when wireless sharing starts or stops. The event args carry the new sharing state.
    /// </summary>
    event EventHandler<WirelessSharingEventArgs> SharingChanged;
}

/// <summary>
/// Event arguments describing a change in wireless sharing state.
/// </summary>
public class WirelessSharingEventArgs : EventArgs
{
    /// <summary>
    /// True if a wireless sharing session is active (content is being presented), false otherwise.
    /// </summary>
    public bool IsSharing { get; private set; }

    /// <summary>
    /// Creates a new <see cref="WirelessSharingEventArgs"/>.
    /// </summary>
    /// <param name="isSharing">True if a wireless sharing session is active, false otherwise.</param>
    public WirelessSharingEventArgs(bool isSharing)
    {
        IsSharing = isSharing;
    }
}
