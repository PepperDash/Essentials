using Crestron.SimplSharpPro;
using FluentAssertions;
using PepperDash.Essentials.Core.Devices;
using Xunit;

namespace PepperDash.Essentials.Tests.Devices;

/// <summary>
/// Registering a control-system port twice makes the Crestron runtime log an "already registered"
/// error on every cold boot (issue #1503), so a port is registered only if it isn't already.
/// </summary>
public class PortRegistrationTests
{
    [Fact]
    public void AnUnregisteredPort_IsRegisteredOnce()
    {
        var calls = 0;

        var result = PortRegistration.RegisterOnce(() => false, () =>
        {
            calls++;
            return eDeviceRegistrationUnRegistrationResponse.Success;
        });

        calls.Should().Be(1);
        result.Should().Be(eDeviceRegistrationUnRegistrationResponse.Success);
    }

    [Fact]
    public void AnAlreadyRegisteredPort_IsNotRegisteredAgain_AndCountsAsSuccess()
    {
        var calls = 0;

        var result = PortRegistration.RegisterOnce(() => true, () =>
        {
            calls++;
            return eDeviceRegistrationUnRegistrationResponse.Failure;
        });

        calls.Should().Be(0);
        result.Should().Be(eDeviceRegistrationUnRegistrationResponse.Success);
    }

    [Fact]
    public void AFailedRegistration_IsReportedToTheCaller()
    {
        var result = PortRegistration.RegisterOnce(() => false, () => eDeviceRegistrationUnRegistrationResponse.Failure);

        result.Should().Be(eDeviceRegistrationUnRegistrationResponse.Failure);
    }

    [Fact]
    public void TwoDevicesSharingAPort_RegisterItOnce()
    {
        // What a processor relay sees: the processor wrapper claims it, then a configured relayoutput asks.
        var registered = false;
        var registrations = 0;

        eDeviceRegistrationUnRegistrationResponse Register()
        {
            registrations++;
            registered = true;
            return eDeviceRegistrationUnRegistrationResponse.Success;
        }

        PortRegistration.RegisterOnce(() => registered, Register);
        PortRegistration.RegisterOnce(() => registered, Register);

        registrations.Should().Be(1);
    }
}
