using FluentAssertions;
using PepperDash.Core.Abstractions;
using PepperDash.Core.Tests.Fakes;
using Xunit;

namespace PepperDash.Core.Tests.Logging;

/// <summary>
/// Tests for Debug-related service interfaces and implementations.
/// These tests verify the behaviour of the abstractions in isolation (no Crestron SDK required).
/// </summary>
public class DebugServiceTests
{
    // -----------------------------------------------------------------------
    // ICrestronDataStore — InMemoryCrestronDataStore
    // -----------------------------------------------------------------------

    [Fact]
    public void DataStore_InitStore_SetsInitializedFlag()
    {
        var store = new InMemoryCrestronDataStore();
        store.Initialized.Should().BeFalse("not yet initialized");

        store.InitStore();

        store.Initialized.Should().BeTrue();
    }

    [Fact]
    public void DataStore_SetAndGetLocalInt_RoundTrips()
    {
        var store = new InMemoryCrestronDataStore();

        store.SetLocalInt("MyKey", 42).Should().BeTrue();
        store.TryGetLocalInt("MyKey", out var value).Should().BeTrue();
        value.Should().Be(42);
    }

    [Fact]
    public void DataStore_TryGetLocalInt_ReturnsFalse_WhenKeyAbsent()
    {
        var store = new InMemoryCrestronDataStore();

        store.TryGetLocalInt("Missing", out var value).Should().BeFalse();
        value.Should().Be(0);
    }

    [Fact]
    public void DataStore_SetAndGetLocalBool_RoundTrips()
    {
        var store = new InMemoryCrestronDataStore();

        store.SetLocalBool("FlagKey", true).Should().BeTrue();
        store.TryGetLocalBool("FlagKey", out var value).Should().BeTrue();
        value.Should().BeTrue();
    }

    [Fact]
    public void DataStore_TryGetLocalBool_ReturnsFalse_WhenKeyAbsent()
    {
        var store = new InMemoryCrestronDataStore();

        store.TryGetLocalBool("Missing", out var value).Should().BeFalse();
        value.Should().BeFalse();
    }

    [Fact]
    public void DataStore_SetLocalUint_CanBeReadBackAsInt()
    {
        var store = new InMemoryCrestronDataStore();

        store.SetLocalUint("UintKey", 3u).Should().BeTrue();
        store.TryGetLocalInt("UintKey", out var value).Should().BeTrue();
        value.Should().Be(3);
    }

    [Fact]
    public void DataStore_Seed_AllowsTestSetupOfReadPaths()
    {
        var store = new InMemoryCrestronDataStore();
        store.Seed("MyLevel", 2);

        store.TryGetLocalInt("MyLevel", out var level).Should().BeTrue();
        level.Should().Be(2);
    }

    // -----------------------------------------------------------------------
    // DebugServiceRegistration
    // -----------------------------------------------------------------------

    [Fact]
    public void ServiceRegistration_Register_StoresAllThreeServices()
    {
        var env = new FakeCrestronEnvironment();
        var console = new NoOpCrestronConsole();
        var store = new InMemoryCrestronDataStore();

        DebugServiceRegistration.Register(env, console, store);

        DebugServiceRegistration.Environment.Should().BeSameAs(env);
        DebugServiceRegistration.Console.Should().BeSameAs(console);
        DebugServiceRegistration.DataStore.Should().BeSameAs(store);
    }

    [Fact]
    public void ServiceRegistration_Register_AcceptsNullsWithoutThrowing()
    {
        var act = () => DebugServiceRegistration.Register(null, null, null);
        act.Should().NotThrow();
    }

    // -----------------------------------------------------------------------
    // ICrestronEnvironment — FakeCrestronEnvironment
    // -----------------------------------------------------------------------

    [Fact]
    public void FakeEnvironment_DefaultsToAppliance()
    {
        var env = new FakeCrestronEnvironment();
        env.DevicePlatform.Should().Be(DevicePlatform.Appliance);
    }

    [Fact]
    public void FakeEnvironment_RaiseProgramStatus_FiresEvent()
    {
        var env = new FakeCrestronEnvironment();
        ProgramStatusEventType? received = null;
        env.ProgramStatusChanged += (_, e) => received = e.EventType;

        env.RaiseProgramStatus(ProgramStatusEventType.Stopping);

        received.Should().Be(ProgramStatusEventType.Stopping);
    }

    [Fact]
    public void FakeEnvironment_RaiseEthernetEvent_FiresEvent()
    {
        var env = new FakeCrestronEnvironment();
        EthernetEventType? received = null;
        env.EthernetEventReceived += (_, e) => received = e.EthernetEventType;

        env.RaiseEthernetEvent(EthernetEventType.LinkUp, adapter: 0);

        received.Should().Be(EthernetEventType.LinkUp);
    }

    // -----------------------------------------------------------------------
    // ICrestronConsole — CapturingCrestronConsole
    // -----------------------------------------------------------------------

    [Fact]
    public void CapturingConsole_PrintLine_CapturesMessage()
    {
        var console = new CapturingCrestronConsole();

        console.PrintLine("hello world");

        console.Lines.Should().ContainSingle().Which.Should().Be("hello world");
    }

    [Fact]
    public void CapturingConsole_AddNewConsoleCommand_RecordsCommandName()
    {
        var console = new CapturingCrestronConsole();

        console.AddNewConsoleCommand(_ => { }, "appdebug", "Sets debug level", ConsoleAccessLevel.AccessOperator);

        console.RegisteredCommands.Should().ContainSingle()
            .Which.Command.Should().Be("appdebug");
    }
}
