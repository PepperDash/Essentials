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

    public void Dispose()
    {
        DeviceManager.RemoveDevice(_device);
        // Leave the static tie-lines state as a fresh program would have it, for the next test.
        DeviceManager.Initialize(null!);
    }

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
        DeviceManager.AreTieLinesLoaded.Should().BeTrue();
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
        DeviceManager.AreTieLinesLoaded.Should().BeFalse("a teardown clears the tie lines");
        DeviceManager.NotifyTieLinesLoaded();

        ran.Should().Be(0, "an action from before the teardown belongs to a torn-down device");
    }

    [Fact]
    public void A_TeardownWhileNotifying_SkipsTheActionsNotYetRun()
    {
        DeviceManager.Initialize(null!);
        var ran = new List<string>();
        DeviceManager.RunWhenTieLinesLoaded(() =>
        {
            ran.Add("first");
            DeviceManager.DeactivateAll(); // e.g. a teardown started from an earlier action
        });
        DeviceManager.RunWhenTieLinesLoaded(() => ran.Add("second"));

        DeviceManager.NotifyTieLinesLoaded();

        ran.Should().Equal("first");
        DeviceManager.AreTieLinesLoaded.Should().BeFalse();
    }

    [Fact]
    public void A_TeardownFromAnAllTieLinesLoadedHandler_SkipsTheWaitingActionsAndLaterHandlers()
    {
        DeviceManager.Initialize(null!);
        var ran = new List<string>();
        DeviceManager.RunWhenTieLinesLoaded(() => ran.Add("action"));
        EventHandler<EventArgs> tearDown = (_, _) => { ran.Add("handler 1"); DeviceManager.DeactivateAll(); };
        EventHandler<EventArgs> later = (_, _) => ran.Add("handler 2");
        DeviceManager.AllTieLinesLoaded += tearDown;
        DeviceManager.AllTieLinesLoaded += later;
        try
        {
            DeviceManager.NotifyTieLinesLoaded();
        }
        finally
        {
            DeviceManager.AllTieLinesLoaded -= tearDown;
            DeviceManager.AllTieLinesLoaded -= later;
        }

        ran.Should().Equal("handler 1");
    }

    [Fact]
    public void Initialize_ResetsTheTieLines()
    {
        DeviceManager.Initialize(null!);
        DeviceManager.NotifyTieLinesLoaded();

        DeviceManager.Initialize(null!);

        DeviceManager.AreTieLinesLoaded.Should().BeFalse();
    }

    [Fact]
    public void RunWhenTieLinesLoaded_RejectsNull()
    {
        var act = () => DeviceManager.RunWhenTieLinesLoaded(null!);

        act.Should().Throw<ArgumentNullException>();
    }

    [Fact]
    public void NotifyTieLinesLoaded_AThrowingActionDoesNotStopTheRest()
    {
        DeviceManager.Initialize(null!);
        var ran = new List<string>();
        DeviceManager.RunWhenTieLinesLoaded(() => throw new InvalidOperationException("one device's startup failed"));
        DeviceManager.RunWhenTieLinesLoaded(() => ran.Add("second"));

        var act = () => DeviceManager.NotifyTieLinesLoaded();

        act.Should().NotThrow("a failing callback is logged, not thrown into the load");
        ran.Should().Equal("second");
    }

    [Fact]
    public void NotifyTieLinesLoaded_AThrowingHandlerDoesNotStopTheRest()
    {
        DeviceManager.Initialize(null!);
        var ran = new List<string>();
        DeviceManager.RunWhenTieLinesLoaded(() => ran.Add("action"));
        EventHandler<EventArgs> throwing = (_, _) => throw new InvalidOperationException("handler failed");
        EventHandler<EventArgs> later = (_, _) => ran.Add("handler 2");
        DeviceManager.AllTieLinesLoaded += throwing;
        DeviceManager.AllTieLinesLoaded += later;
        try
        {
            DeviceManager.NotifyTieLinesLoaded();
        }
        finally
        {
            DeviceManager.AllTieLinesLoaded -= throwing;
            DeviceManager.AllTieLinesLoaded -= later;
        }

        ran.Should().Equal("handler 2", "action");
    }

    [Fact]
    public void RunWhenTieLinesLoaded_WhenAlreadyLoaded_ThrowsToTheCaller()
    {
        DeviceManager.Initialize(null!);
        DeviceManager.NotifyTieLinesLoaded();

        var act = () => DeviceManager.RunWhenTieLinesLoaded(() => throw new InvalidOperationException("failed"));

        act.Should().Throw<InvalidOperationException>("it runs on the caller's thread then");
    }

    [Fact]
    public void RunWhenTieLinesLoaded_CalledFromACallback_RunsAtOnce()
    {
        DeviceManager.Initialize(null!);
        var ran = new List<string>();
        DeviceManager.RunWhenTieLinesLoaded(() =>
        {
            DeviceManager.RunWhenTieLinesLoaded(() => ran.Add("nested"));
            ran.Add("outer done");
        });

        DeviceManager.NotifyTieLinesLoaded();

        ran.Should().Equal("nested", "outer done");
    }

    [Fact]
    public void NotifyTieLinesLoaded_ASecondCallInTheSameLoadDoesNothing()
    {
        DeviceManager.Initialize(null!);
        var raised = 0;
        EventHandler<EventArgs> handler = (_, _) => raised++;
        DeviceManager.AllTieLinesLoaded += handler;
        try
        {
            DeviceManager.NotifyTieLinesLoaded();
            DeviceManager.NotifyTieLinesLoaded();
        }
        finally
        {
            DeviceManager.AllTieLinesLoaded -= handler;
        }

        raised.Should().Be(1);
    }

    [Fact]
    public void ResetTieLinesLoaded_StartsANewLoad_WhichSignalsAgain()
    {
        // As ControlSystem.Load() does at the start of each load, including a reload without a teardown.
        DeviceManager.Initialize(null!);
        DeviceManager.NotifyTieLinesLoaded();
        var raised = 0;
        var ran = 0;
        EventHandler<EventArgs> handler = (_, _) => raised++;
        DeviceManager.AllTieLinesLoaded += handler;
        try
        {
            DeviceManager.ResetTieLinesLoaded();
            DeviceManager.RunWhenTieLinesLoaded(() => ran++);
            ran.Should().Be(0, "the new load's tie lines aren't loaded yet");

            DeviceManager.NotifyTieLinesLoaded();
        }
        finally
        {
            DeviceManager.AllTieLinesLoaded -= handler;
        }

        raised.Should().Be(1);
        ran.Should().Be(1);
    }
}
