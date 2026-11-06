using FluentAssertions;
using PepperDash.Core;
using PepperDash.Essentials.Core;
using Xunit;

namespace PepperDash.Essentials.Tests.Devices;

/// <summary>
/// Devices can be added off a processor (issue #1553): <see cref="DeviceManager.Initialize"/> no
/// longer needs a control system or the Crestron console, so a test can call it to allow adding
/// devices.
/// </summary>
[Collection(GlobalRoutingStateCollection.Name)]
public sealed class DeviceManagerTests : IDisposable
{
    private sealed class Keyed : IKeyed
    {
        public Keyed(string key) => Key = key;
        public string Key { get; }
    }

    private readonly Keyed _device = new($"deviceManagerTests-{Guid.NewGuid():N}");

    public void Dispose() => DeviceManager.RemoveDevice(_device);

    [Fact]
    public void Initialize_AllowsAddingDevices()
    {
        DeviceManager.Initialize(null!);

        DeviceManager.AddDevice(_device);

        DeviceManager.AddDeviceEnabled.Should().BeTrue();
        DeviceManager.GetDeviceForKey(_device.Key).Should().BeSameAs(_device);
    }

    [Fact]
    public void Initialize_AddsItsConsoleCommandsToTheRegisteredConsole()
    {
        DeviceManager.Initialize(null!);

        TestInitializer.Console.HasCommand("devlist").Should().BeTrue();
        TestInitializer.Console.HasCommand("devjson").Should().BeTrue();
    }

    [Fact]
    public void Initialize_AllowsAddingDevicesAgainAfterActivation()
    {
        DeviceManager.Initialize(null!);
        DeviceManager.ActivateAll();
        DeviceManager.AddDeviceEnabled.Should().BeFalse("activation closes the device list");

        DeviceManager.Initialize(null!);
        DeviceManager.AddDevice(_device);

        DeviceManager.GetDeviceForKey(_device.Key).Should().BeSameAs(_device);
    }
}
