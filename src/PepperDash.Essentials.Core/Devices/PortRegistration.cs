using System;
using Crestron.SimplSharpPro;

namespace PepperDash.Essentials.Core.Devices;

/// <summary>
/// Registers control-system ports (relays, COM ports) without registering one twice.
/// </summary>
/// <remarks>
/// Registering a port that is already registered makes the Crestron runtime log an "already
/// registered" error, and several Essentials devices can end up with the same port: the processor
/// wrapper claims every relay before a configured <c>relayoutput</c> asks for the same one, and a COM
/// port controller registers its port once. Checking first keeps those errors out of the log, so a
/// real registration failure stands out.
/// </remarks>
public static class PortRegistration
{
    /// <summary>
    /// Registers a port unless it is already registered.
    /// </summary>
    /// <param name="isRegistered">Whether the port is currently registered (the port's <c>Registered</c>).</param>
    /// <param name="register">Registers the port (the port's <c>Register</c>); not called when it is already registered.</param>
    /// <returns>
    /// <see cref="eDeviceRegistrationUnRegistrationResponse.Success"/> when the port was already
    /// registered, otherwise the result of <paramref name="register"/>.
    /// </returns>
    public static eDeviceRegistrationUnRegistrationResponse RegisterOnce(
        Func<bool> isRegistered,
        Func<eDeviceRegistrationUnRegistrationResponse> register)
    {
        if (isRegistered == null) throw new ArgumentNullException(nameof(isRegistered));
        if (register == null) throw new ArgumentNullException(nameof(register));

        return isRegistered()
            ? eDeviceRegistrationUnRegistrationResponse.Success
            : register();
    }
}
