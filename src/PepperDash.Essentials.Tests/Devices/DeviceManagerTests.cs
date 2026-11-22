using System.Collections.Concurrent;
using FluentAssertions;
using PepperDash.Core;
using PepperDash.Essentials.Core;
using Serilog;
using Serilog.Core;
using Serilog.Events;
using Xunit;
using Debug = PepperDash.Core.Debug;

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

    // ---- devlist and devfb answer the command, as on 2.x (#1582, #1540) ----

    private sealed class FeedbackDevice : IHasFeedback
    {
        public FeedbackDevice(string key, params Feedback[] feedbacks)
        {
            Key = key;
            foreach (var feedback in feedbacks)
            {
                feedback.FireUpdate();
                Feedbacks.Add(feedback);
            }
        }

        public string Key { get; }
        public FeedbackCollection<Feedback> Feedbacks { get; } = new();
    }

    [Fact]
    public void Devlist_AnswersTheCommand_NotThroughTheLog()
    {
        DeviceManager.Initialize(null!);
        DeviceManager.AddDevice(_device);

        var reply = TestInitializer.Console.RunCommand("devlist");

        reply.Should().MatchRegex(@"^\d+ Devices registered with Device Manager:\r\n");
        reply.Should().Contain($"  [{_device.Key}] ---\r\n", "a device without a name shows ---");
    }

    [Fact]
    public void Devfb_ListsTheFeedbacksOfADeviceThatHasThem()
    {
        DeviceManager.Initialize(null!);
        var device = new FeedbackDevice($"feedbackDevice-{Guid.NewGuid():N}",
            new IntFeedback("volume", () => 40), new BoolFeedback("power", () => true));
        DeviceManager.AddDevice(device);
        try
        {
            var reply = TestInitializer.Console.RunCommand("devfb", device.Key);

            reply.Should().Be(
                "Available feedbacks:\r\n" +
                "  boolean      power                     True\r\n" +
                "  integer      volume                    40\r\n",
                "sorted by type, then key");
        }
        finally
        {
            DeviceManager.RemoveDevice(device);
        }
    }

    [Fact]
    public void Devfb_SaysSoForADeviceWithoutFeedbacks()
    {
        DeviceManager.Initialize(null!);
        var device = new FeedbackDevice($"noFeedbacks-{Guid.NewGuid():N}");
        DeviceManager.AddDevice(device);
        try
        {
            TestInitializer.Console.RunCommand("devfb", device.Key).Should().Be("No available feedbacks\r\n");
        }
        finally
        {
            DeviceManager.RemoveDevice(device);
        }
    }

    [Fact]
    public void Devfb_SaysSoForAnUnknownDevice_OrOneWithoutFeedbacksInterface()
    {
        DeviceManager.Initialize(null!);
        DeviceManager.AddDevice(_device);

        TestInitializer.Console.RunCommand("devfb", "noSuchDevice").Should().Be("Device 'noSuchDevice' not found\r\n");
        TestInitializer.Console.RunCommand("devfb", _device.Key).Should().Be($"Device '{_device.Key}' does not have visible feedbacks\r\n");
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

    private sealed class TieLinesAwareDevice : Device
    {
        public TieLinesAwareDevice(string key) : base(key) { }

        public bool? TieLinesLoadedWhenDeactivated { get; private set; }

        public override bool Deactivate()
        {
            TieLinesLoadedWhenDeactivated = DeviceManager.AreTieLinesLoaded;
            return true;
        }
    }

    [Fact]
    public void DeactivateAll_ResetsTheTieLinesBeforeDeactivatingDevices()
    {
        DeviceManager.Initialize(null!);
        var device = new TieLinesAwareDevice($"tieLinesAware-{Guid.NewGuid():N}");
        DeviceManager.AddDevice(device);
        try
        {
            DeviceManager.NotifyTieLinesLoaded();

            DeviceManager.DeactivateAll();

            device.TieLinesLoadedWhenDeactivated.Should().BeFalse(
                "callbacks that haven't started must already be stopped while devices are deactivated");
        }
        finally
        {
            DeviceManager.RemoveDevice(device);
        }
    }

    // ---- All devices initialized is raised once (#1570) ----

    private sealed class InitializingDevice : EssentialsDevice
    {
        private readonly Func<bool>? _initializeAfter;

        public InitializingDevice(string key, Func<bool>? initializeAfter = null) : base(key)
            => _initializeAfter = initializeAfter;

        protected override void Initialize()
        {
            if (_initializeAfter != null)
                SpinWait.SpinUntil(_initializeAfter, TimeSpan.FromSeconds(5));
        }
    }

    [Fact]
    public void AllDevicesInitialized_IsRaisedOnce_WhenTheLastDevicesFinishTogether()
    {
        DeviceManager.Initialize(null!);
        var first = new InitializingDevice($"initFirst-{Guid.NewGuid():N}");
        var second = new InitializingDevice($"initSecond-{Guid.NewGuid():N}", () => first.IsInitialized);

        // Runs before DeviceManager's handler for the first device (subscribed by AddDevice) and holds
        // it until the second device is initialized too, so both of DeviceManager's handlers see every
        // device initialized.
        first.Initialized += (_, _) => SpinWait.SpinUntil(() => second.IsInitialized, TimeSpan.FromSeconds(5));
        DeviceManager.AddDevice(first);
        DeviceManager.AddDevice(second);

        using var bothHandled = new CountdownEvent(2);
        first.Initialized += (_, _) => bothHandled.Signal();
        second.Initialized += (_, _) => bothHandled.Signal();

        var raised = 0;
        EventHandler<EventArgs> handler = (_, _) => Interlocked.Increment(ref raised);
        DeviceManager.AllDevicesInitialized += handler;
        try
        {
            DeviceManager.ActivateAll();

            bothHandled.Wait(TimeSpan.FromSeconds(5)).Should().BeTrue("both devices initialize");
            raised.Should().Be(1);
        }
        finally
        {
            DeviceManager.AllDevicesInitialized -= handler;
            DeviceManager.RemoveDevice(first);
            DeviceManager.RemoveDevice(second);
        }
    }

    // ---- What a failing tie-lines-loaded callback logs ----

    private sealed class CaptureSink : ILogEventSink
    {
        public ConcurrentQueue<LogEvent> Events { get; } = new();
        public void Emit(LogEvent logEvent) => Events.Enqueue(logEvent);
    }

    /// <summary>A device whose startup work fails, subscribed with its own methods.</summary>
    private sealed class FailingStartupDevice : IKeyed
    {
        public FailingStartupDevice(string key) => Key = key;
        public string Key { get; }
        public void StartUp() => throw new InvalidOperationException("startup failed");
        public void OnTieLinesLoaded(object? sender, EventArgs e) => throw new InvalidOperationException("handler failed");
    }

    private static List<LogEvent> LogDuring(Action act, string deviceKey)
    {
        var sink = new CaptureSink();
        Debug.UpdateLoggerConfiguration(new LoggerConfiguration()
            .MinimumLevel.Verbose()
            .Enrich.FromLogContext()
            .WriteTo.Sink(sink));
        try
        {
            act();
        }
        finally
        {
            Debug.UpdateLoggerConfiguration(new LoggerConfiguration().MinimumLevel.Verbose());
        }

        return sink.Events
            .Where(e => e.Properties.TryGetValue("Key", out var key) && key.ToString().Trim('"') == deviceKey)
            .ToList();
    }

    [Fact]
    public void AFailingAction_IsLoggedAsAnError_AgainstItsDeviceAndMethod()
    {
        DeviceManager.Initialize(null!);
        var device = new FailingStartupDevice($"failingStartup-{Guid.NewGuid():N}");
        DeviceManager.RunWhenTieLinesLoaded(device.StartUp);

        var entries = LogDuring(DeviceManager.NotifyTieLinesLoaded, device.Key);

        var entry = entries.Should().ContainSingle().Subject;
        entry.Level.Should().Be(LogEventLevel.Error);
        entry.Exception.Should().BeOfType<InvalidOperationException>();
        entry.RenderMessage().Should().Contain("RunWhenTieLinesLoaded action")
            .And.Contain($"{nameof(FailingStartupDevice)}.{nameof(FailingStartupDevice.StartUp)}")
            .And.NotContain("\"", "the fields are rendered as plain text");
    }

    [Fact]
    public void AFailingHandler_IsLoggedAgainstTheSubscriber_NotDeviceManager()
    {
        DeviceManager.Initialize(null!);
        var device = new FailingStartupDevice($"failingHandler-{Guid.NewGuid():N}");
        DeviceManager.AllTieLinesLoaded += device.OnTieLinesLoaded;
        List<LogEvent> entries;
        try
        {
            entries = LogDuring(DeviceManager.NotifyTieLinesLoaded, device.Key);
        }
        finally
        {
            DeviceManager.AllTieLinesLoaded -= device.OnTieLinesLoaded;
        }

        var entry = entries.Should().ContainSingle().Subject;
        entry.Level.Should().Be(LogEventLevel.Error);
        entry.RenderMessage().Should().Contain("AllTieLinesLoaded handler")
            .And.Contain($"{nameof(FailingStartupDevice)}.{nameof(FailingStartupDevice.OnTieLinesLoaded)}")
            .And.NotContain(nameof(DeviceManager) + "+");
    }
}
