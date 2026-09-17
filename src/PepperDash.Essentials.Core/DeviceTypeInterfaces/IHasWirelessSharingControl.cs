namespace PepperDash.Essentials.Core.DeviceTypeInterfaces;

/// <summary>
/// Defines the contract for a wireless presentation endpoint whose session can be ended by the
/// system, rather than only by whoever is sharing.
/// </summary>
/// <remarks>
/// <para>Deliberately independent of <see cref="IHasWirelessSharing"/>. Ending a session and
/// reporting whether one is active are different capabilities, and a real endpoint may have
/// either without the other — some accept a reset command while exposing no session state at all.
/// An endpoint that does both implements both.</para>
/// <para>The case this exists for is a room shutting down: a shared laptop outlives the session
/// that shared it, and nothing about powering a room off disconnects it, so the next person in
/// finds the previous one's desktop still on the display.</para>
/// </remarks>
public interface IHasWirelessSharingControl
{
    /// <summary>
    /// Disconnects whoever is currently sharing.
    /// </summary>
    void EndSharingSession();
}
