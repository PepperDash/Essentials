namespace PepperDash.Essentials.Core.DeviceTypeInterfaces;

/// <summary>
/// Defines the contract for a wireless presentation endpoint whose session can be ended by the
/// system, rather than only by whoever is sharing.
/// </summary>
/// <remarks>
/// Extends <see cref="IHasWirelessSharing"/> rather than adding to it, so that endpoints which can
/// only report sharing state continue to compile. The case this exists for is a room shutting
/// down: a shared laptop outlives the session that shared it, and nothing about powering a room
/// off disconnects it, so the next person in finds the last person's desktop still on the display.
/// </remarks>
public interface IHasWirelessSharingControl : IHasWirelessSharing
{
    /// <summary>
    /// Disconnects whoever is currently sharing.
    /// </summary>
    void EndSharingSession();
}
