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

    // ---- Tie lines loaded (#1568): devices activate before the tie lines load ----

    [Fact]
    public void RunWhenTieLinesLoaded_WaitsUntilTheTieLinesAreLoaded()
    {
        DeviceManager.Initialize(null!);
        var ran = 0;

        DeviceManager.RunWhenTieLinesLoaded(() => ran++);
        ran.Should().Be(0, "the tie lines aren't loaded yet");

        DeviceManager.NotifyTieLinesLoaded();

        ran.Should().Be(1);
        DeviceManager.TieLinesLoaded.Should().BeTrue();
    }

    [Fact]
    public void RunWhenTieLinesLoaded_RunsAtOnceWhenTheTieLinesAreAlreadyLoaded()
    {
        DeviceManager.Initialize(null!);
        DeviceManager.NotifyTieLinesLoaded();
        var ran = 0;

        DeviceManager.RunWhenTieLinesLoaded(() => ran++);

        ran.Should().Be(1);
    }

    [Fact]
    public void RunWhenTieLinesLoaded_RunsEachActionOnce()
    {
        DeviceManager.Initialize(null!);
        var ran = 0;
        DeviceManager.RunWhenTieLinesLoaded(() => ran++);

        DeviceManager.NotifyTieLinesLoaded();
        DeviceManager.NotifyTieLinesLoaded();

        ran.Should().Be(1);
    }

    [Fact]
    public void NotifyTieLinesLoaded_RaisesAllTieLinesLoaded()
    {
        DeviceManager.Initialize(null!);
        var raised = 0;
        EventHandler<EventArgs> handler = (_, _) => raised++;
        DeviceManager.AllTieLinesLoaded += handler;
        try
        {
            DeviceManager.NotifyTieLinesLoaded();
        }
        finally
        {
            DeviceManager.AllTieLinesLoaded -= handler;
        }

        raised.Should().Be(1);
    }

    [Fact]
    public void DeactivateAll_ResetsTheTieLinesAndDropsWaitingActions()
    {
        DeviceManager.Initialize(null!);
        var ran = 0;
        DeviceManager.RunWhenTieLinesLoaded(() => ran++);

        DeviceManager.DeactivateAll();
        DeviceManager.TieLinesLoaded.Should().BeFalse("a teardown clears the tie lines");
        DeviceManager.NotifyTieLinesLoaded();

        ran.Should().Be(0, "an action from before the teardown belongs to a torn-down device");
    }

    [Fact]
    public void Initialize_ResetsTheTieLines()
    {
        DeviceManager.Initialize(null!);
        DeviceManager.NotifyTieLinesLoaded();

        DeviceManager.Initialize(null!);

        DeviceManager.TieLinesLoaded.Should().BeFalse();
    }

    [Fact]
    public void RunWhenTieLinesLoaded_RejectsNull()
    {
        var act = () => DeviceManager.RunWhenTieLinesLoaded(null!);

        act.Should().Throw<ArgumentNullException>();
    }
}
